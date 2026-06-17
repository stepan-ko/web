using Microsoft.EntityFrameworkCore;
using OpenCvSharp;
using System.Collections.Concurrent;
using System.Diagnostics;

public class CameraManager
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly FrameBuffer _frameBuffer;
    private readonly Dictionary<int, Task> _cameraTasks = new();
    private readonly ConcurrentDictionary<int, CameraWorker> _workers = new();
    private CancellationTokenSource? _cts;
    private readonly ILogger<CameraManager> _logger;
    public CameraManager(IServiceScopeFactory scopeFactory, ILogger<CameraManager> logger, FrameBuffer buffer)
    {
        _scopeFactory = scopeFactory;
        _frameBuffer = buffer;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken token)
    {
        _cts = CancellationTokenSource.CreateLinkedTokenSource(token);

        var cameras = await LoadCameras();

        foreach (var camera in cameras)
        {
            StartCamera(camera, _cts.Token);
        }
    }

    public void StartCamera(Camera camera, CancellationToken token)
    {
        if (_cameraTasks.ContainsKey(camera.Id))
            return;

        var task = Task.Run(() => ProcessCamera(camera, token), token);

        _cameraTasks[camera.Id] = task;
    }

    public async Task StopAsync()
    {
        if (_cts != null)
            _cts.Cancel();

        await Task.WhenAll(_cameraTasks.Values);

        _cameraTasks.Clear();
    }

    private async Task<List<Camera>> LoadCameras()
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        return await db.Cameras
            .Include(c => c.Option)
            .Where(c => c.Enable)
            .ToListAsync();
    }



    private async Task ProcessCamera(Camera camera, CancellationToken token)
    {
        _logger.LogDebug($"Запуск обработки видео камеры: {camera.Name}");
        _logger.LogDebug($"Адрес видеопотока: {camera.StreamUrl}");         
         // 1. открыть поток
        
        
        try
        {  
                Console.WriteLine($">{camera.StreamUrl}<"); 
                // using var capture = new VideoCapture();

                // if (!capture.Open(camera.StreamUrl))
                // {               
                //     _logger.LogError($"Видеопоток #{camera.Id}: '{camera.Name}' не открылся");
                // }
                // else {
                //     _logger.LogInformation($"Видеопоток '{camera.Name}' открыт");
                // }
             
            // Console.WriteLine(Cv2.GetBuildInformation());
                var psi = new ProcessStartInfo
                {
                    FileName = "ffmpeg",
                    Arguments =
                        $"-rtsp_transport tcp " +
                        $"-i \"{camera.StreamUrl}\" " +
                        $"-an -sn -dn " +
                        $"-f rawvideo " +
                        $"-pix_fmt bgr24 " +
                        $"-vf scale=1280:720 " +
                        $"-stream_loop -1" +
                        $"-",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

            using var process = Process.Start(psi);
            var stream = process.StandardOutput.BaseStream;

            int width = 1280;
            int height = 720;
            int frameSize = width * height * 3;

            byte[] buffer = new byte[frameSize];



            while (!token.IsCancellationRequested)
            {
                // 2. получить кадр
                // using var frame = new Mat();
                // capture.Read(frame);
                // if (token.IsCancellationRequested) break;
                            
                // if (frame.Empty())
                // {
                //     _logger.LogInformation($"Видеопоток #{camera.Id}: '{camera.Name}' завершился");
                //     break;
                // }

                 int read = 0;
                while (read < frameSize)
                {
                    int r = stream.Read(buffer, read, frameSize - read);
                    if (r == 0) break;
                    read += r;
                }

                // if (read != frameSize)
                //     continue;
                if (read == 0)
                {
                    _logger.LogWarning("FFmpeg stream broken");
                    break;
                }


                // using var frame = Mat.FromImageData(buffer, ImreadModes.Color);
               using var frame = Mat.FromPixelData(height, width, MatType.CV_8UC3, buffer);

                if (token.IsCancellationRequested) break;
                
                // Рисуем рамку Arae
                if (camera.Option.UseArea)
                {
                Rect border = new Rect(camera.Option.AreaX, camera.Option.AreaY, camera.Option.AreaWidth, camera.Option.AreaHeight);
                Cv2.Rectangle(frame, border, Scalar.Blue, 1);
                }

                // 3. распознать номер
                

                // 4. передать видео на страницу
                // Cv2.ImEncode(".jpg", frame, out var bytes);
                Cv2.ImEncode(".jpg", frame, out var bytes, new int[] { (int)ImwriteFlags.JpegQuality, 80 });
                
                _frameBuffer.SetFrame(camera.Id, bytes);

                // await Task.Delay(100, token);
            }
        }        
        catch (Exception ex) 
        {
            _logger.LogError($"Ошибка видеопотока: {ex}");
        }

    }

    public void StartCamera(Camera camera)
    {
        if (_workers.TryGetValue(camera.Id, out var worker))
        {
            if (worker.IsRunning)
                return;
        }

        var cts = new CancellationTokenSource();

        worker = new CameraWorker
        {
            Camera = camera,
            Cts = cts
        };

        worker.Task = Task.Run(async () =>
        {
            try
            {
                await ProcessCamera(camera, cts.Token);
            }
            catch (Exception ex)
            {
                worker.LastError = ex.Message;
                _logger.LogError(ex, "Camera error");
            }
        });

        _workers[camera.Id] = worker;
    }

    public async Task StopCamera(int cameraId)
    {
        if (!_workers.TryGetValue(cameraId, out var worker))
            return;
        worker.Cts?.Cancel();
        if (worker.Task != null)
        {
            try
            {
                await worker.Task;
            }
            catch
            {
            }
        }
        _workers.TryRemove(cameraId, out _);
    }


    public async Task RestartCamera(Camera camera)
    {
        await StopCamera(camera.Id);

        StartCamera(camera);
    }

    public bool IsRunning(int cameraId)
    {
        return _workers.TryGetValue(cameraId, out var worker)
            && worker.IsRunning;
    }


}
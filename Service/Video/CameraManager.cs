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

        var rtspOpt = camera.StreamUrl.StartsWith("rtsp://", StringComparison.OrdinalIgnoreCase)
            ? "-rtsp_transport tcp "
            : "";

        int width = camera.Width != 0 ? camera.Width : 1280;
        int height = camera.Height != 0 ? camera.Height : 720;
        int fps = camera.Fps != 0 ? camera.Fps : 5; // частота кадров для распознавания/превью, можно вынести в camera.Option
        int frameSize = width * height * 3; // bgr24 = 3 байта на пиксель


        var psi = new ProcessStartInfo
        {
            FileName = "ffmpeg",
            Arguments =
                rtspOpt +
                $"-re -i \"{camera.StreamUrl}\" " +
                "-an -sn -dn " +
                "-f rawvideo -pix_fmt bgr24 " +
                $"-vf fps={fps},scale={width}:{height} " +
                "-",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        Process? process = null;

        try
        {
            process = Process.Start(psi);
            if (process == null)
            {
                _logger.LogError($"Не удалось запустить ffmpeg для камеры {camera.Name}");
                return;
            }

            // читаем stderr отдельно, иначе при заполнении буфера канала ffmpeg зависнет
            // _ = Task.Run(async () =>
            // {
            //     try
            //     {
            //         string? line;
            //         while ((line = await process.StandardError.ReadLineAsync()) != null)
            //             _logger.LogWarning($"[ffmpeg:{camera.Name}] {line}");
            //     }
            //     catch { /* процесс завершился — это нормально */ }
            // });

            var stream = process.StandardOutput.BaseStream;
            var buffer = new byte[frameSize];

            while (!token.IsCancellationRequested)
            {
                int read = 0;
                while (read < frameSize)
                {
                    int r = await stream.ReadAsync(buffer, read, frameSize - read, token);
                    if (r == 0) break;
                    read += r;
                }

                if (read == 0)
                {
                    _logger.LogWarning($"[{camera.Name}] FFmpeg stream broken");
                    break;
                }
                if (read < frameSize)
                {
                    // неполный кадр в конце (например, поток только что обрвался) — пропускаем
                    _logger.LogWarning($"[{camera.Name}] Получен неполный кадр: {read}/{frameSize} байт");
                    continue;
                }

                using var frame = Mat.FromPixelData(height, width, MatType.CV_8UC3, buffer);

                //Тут обработка frame сторонней библиотекой поиска номера авто

                if (camera.Option.UseArea)
                {
                    var border = new Rect(camera.Option.AreaX, camera.Option.AreaY,
                                        camera.Option.AreaWidth, camera.Option.AreaHeight);
                    Cv2.Rectangle(frame, border, Scalar.Blue, 1);
                }

                string fpsText = $"FPS: {fps}";

                var font = HersheyFonts.HersheySimplex;
                double fontScale = 0.7;
                int thickness = 2;
                int margin = 10;

                var textSize = Cv2.GetTextSize(fpsText, font, fontScale, thickness, out int baseline);

                int x = frame.Width - textSize.Width - margin;
                int y = margin + textSize.Height;

                Cv2.PutText(frame, fpsText, new Point(x, y), font, fontScale, Scalar.White, thickness);

                Cv2.ImEncode(".jpg", frame, out var outBytes, new[] { (int)ImwriteFlags.JpegQuality, 80 });
                _frameBuffer.SetFrame(camera.Id, outBytes);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogDebug($"Обработка камеры {camera.Name} остановлена");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Ошибка видеопотока камеры {camera.Name}");
        }
        finally
        {
            if (process != null && !process.HasExited)
            {
                try { process.Kill(entireProcessTree: true); }
                catch { /* процесс уже мог завершиться сам */ }
            }
            process?.Dispose();
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
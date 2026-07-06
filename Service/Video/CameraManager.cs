using Microsoft.EntityFrameworkCore;
using OpenCvSharp;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.InteropServices;

public class CameraManager
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly FrameBuffer _frameBuffer;
   

    // private readonly Dictionary<int, Task> _cameraTasks = new();
    private readonly ConcurrentDictionary<int, CameraWorker> _workers = new();
    private CancellationTokenSource? _cts;
    private readonly ILogger<CameraManager> _logger;
    private readonly IServiceProvider _serviceProvider;
    public CameraManager(IServiceScopeFactory scopeFactory, ILogger<CameraManager> logger, FrameBuffer buffer, IServiceProvider serviceProvider)
    {
        _scopeFactory = scopeFactory;
        _frameBuffer = buffer;
        _logger = logger;
        _serviceProvider = serviceProvider;
       
    }

    private readonly SemaphoreSlim _frameSignal = new(0, 1);
    private Mat? _latestFrame;
    private readonly object _frameLock = new(); 
    
    private async Task ProcessCameraInternal(Camera camera, CancellationToken token)
    {
        _logger.LogDebug("Запуск обработки видео камеры {Name}", camera.Name);
        _logger.LogDebug("Адрес видеопотока: {camera.StreamUrl}", camera.StreamUrl);

        var rtspOpt = camera.StreamUrl.StartsWith("rtsp://", StringComparison.OrdinalIgnoreCase)
            ? "-rtsp_transport tcp "
            : "";

        int width = camera.Width != 0 ? camera.Width : 1280;
        int height = camera.Height != 0 ? camera.Height : 720;
        int fps = camera.Fps != 0 ? camera.Fps : 5;
        int frameSize = width * height * 3;

        // локальные для этой камеры — не общие на весь CameraManager
        var frameSignal = new SemaphoreSlim(0, 1);
        Mat? latestFrame = null;
        var frameLock = new object();

        var psi = new ProcessStartInfo
        {
            FileName = "ffmpeg",
            Arguments =
                rtspOpt +
                "-fflags +nobuffer+discardcorrupt " +
                "-flags low_delay " +
                "-analyzeduration 1000000 " +
                "-probesize 1000000 " +
                $"-i \"{camera.StreamUrl}\" " +
                "-an -sn -dn " +
                $"-vf fps={camera.Fps} " +
                "-pix_fmt bgr24 " +
                "-f rawvideo -",
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

            _ = Task.Run(async () =>
            {
                try
                {
                    // string? line;
                    // while ((line = await process.StandardError.ReadLineAsync()) != null)
                    //     _logger.LogWarning($"[ffmpeg:{camera.Name}] {line}");
                }
                catch { }
            });

            var stream = process.StandardOutput.BaseStream;
            var buffer = new byte[frameSize];

            using var cameraRecognize = new CameraRecognize(camera, _logger);

            var plateAnalyse = _serviceProvider.GetRequiredService<PlateAnalyse>();
            plateAnalyse.Init(camera);

            // --- Задача обработки кадров (в отдельном потоке от чтения) ---
            var processingTask = Task.Run(async () =>
            {
                var sw = Stopwatch.StartNew();
                int processedCount = 0;

                while (!token.IsCancellationRequested)
                {
                    await frameSignal.WaitAsync(token);

                    Mat? frame;
                    lock (frameLock)
                    {
                        frame = latestFrame;
                        latestFrame = null;
                    }

                    if (frame == null) continue;

                    using (frame)
                    {
                        if (!camera.Simulate)
                        {
                            List<PlateResult> platesResult = cameraRecognize.RecognizePlate(frame);

                            foreach (var plate in platesResult)
                            {
                                Cv2.Rectangle(frame, plate.RectPlate, Scalar.Yellow, 2);
                                plateAnalyse.Detect(plate);
                            }
                        }
                        plateAnalyse.Lost();

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

                        Cv2.ImEncode(".jpg", frame, out var outBytes, JpegParams);
                        _frameBuffer.SetFrame(camera.Id, outBytes);
                    }

                    processedCount++;
                    if (sw.ElapsedMilliseconds >= 1000)
                    {
                        double actualFps = processedCount / (sw.ElapsedMilliseconds / 1000.0);
                        _logger.LogDebug("[{Name}] Обработка: {Fps:F1} FPS", camera.Name, actualFps);
                        processedCount = 0;
                        sw.Restart();
                    }
                }
            }, token);

            // --- Цикл чтения кадров из ffmpeg (быстрый, без тяжёлой обработки) ---
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
                    _logger.LogWarning($"[{camera.Name}] Получен неполный кадр: {read}/{frameSize} байт");
                    continue;
                }

                var frameData = (byte[])buffer.Clone();
                var frame = new Mat(height, width, MatType.CV_8UC3);
                Marshal.Copy(frameData, 0, frame.Data, frameData.Length);

                lock (frameLock)
                {
                    latestFrame?.Dispose();
                    latestFrame = frame;
                }

                if (frameSignal.CurrentCount == 0)
                    frameSignal.Release();
            }

            // ждём завершения задачи обработки при остановке камеры
            try { await processingTask; } catch (OperationCanceledException) { }
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
                catch { }
            }
            process?.Dispose();
        }
    }
    private async Task ProcessCamera(Camera camera, CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            try
            {
                await ProcessCameraInternal(camera, token);
                _logger.LogWarning("Камера {Name} остановилась, повторное подключение через 5 секунд", camera.Name);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,"Ошибка камеры {Name}, повторное подключение через 5 секунд",camera.Name);
            }

            try
            {
                await Task.Delay(TimeSpan.FromSeconds(2), token);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }


    public async Task StartAsync(CancellationToken token)
    {
        _cts = CancellationTokenSource.CreateLinkedTokenSource(token);

        var cameras = await LoadCameras();

        foreach (var camera in cameras)
        {
            StartCamera(camera);
        }

        _logger.LogInformation(
            "Запущено камер: {Count}",
            cameras.Count);
    }

   public async Task StopAsync()
    {
        if (_workers.IsEmpty)
            return;

        _logger.LogInformation("Остановка всех камер...");

        foreach (var worker in _workers.Values)
        {
            worker.Cts?.Cancel();
        }

        var tasks = _workers.Values
            .Where(x => x.Task != null)
            .Select(x => x.Task!);

        try
        {
            await Task.WhenAll(tasks);
        }
        catch (OperationCanceledException)
        {
            // Нормальная ситуация при остановке
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при остановке камер");
        }

        _workers.Clear();

        _cts?.Dispose();
        _cts = null;

        _logger.LogInformation("Все камеры остановлены");
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

    private static readonly int[] JpegParams = {
        (int)ImwriteFlags.JpegQuality,
        80
    };

    public void StartCamera(Camera camera)
    {
        if (!camera.Enable)
            return;

        if (_workers.ContainsKey(camera.Id))
            return;

        var cts = new CancellationTokenSource();

        var worker = new CameraWorker
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
            catch (OperationCanceledException)
            {
                _logger.LogInformation(
                    "Камера {Name} остановлена",
                    camera.Name);
            }
            catch (Exception ex)
            {
                worker.LastError = ex.Message;

                _logger.LogError(
                    ex,
                    "Ошибка камеры {Name}",
                    camera.Name);
            } 
            finally
            {
                _workers.TryRemove(camera.Id, out _);
            }
        });

        if (!_workers.TryAdd(camera.Id, worker))
        {
            cts.Cancel();
            return;
        }

        _logger.LogInformation(
            "Камера {Name} запущена",
            camera.Name);
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

    private ConcurrentDictionary<string, TrackActive> _activeTracks = new ConcurrentDictionary<string, TrackActive>();
    private int cnt;
    
   
}
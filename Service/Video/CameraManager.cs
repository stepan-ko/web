using Microsoft.EntityFrameworkCore;
using OpenCvSharp;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Channels; // Обязательно добавить этот namespace

public class CameraManager
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly FrameBuffer _frameBuffer;
    
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

    private static readonly int[] JpegParams = {
        (int)ImwriteFlags.JpegQuality,
        95 // Можно снизить до 70 для ускорения ImEncode
    };

    private async Task ProcessCameraInternal(Camera camera, CancellationToken token)
    {
        _logger.LogDebug("Запуск обработки видео камеры {Name}", camera.Name);

        var rtspOpt = camera.StreamUrl.StartsWith("rtsp://", StringComparison.OrdinalIgnoreCase)
            ? "-rtsp_transport tcp "
            : "";

        int width = camera.Width != 0 ? camera.Width : 1280;
        int height = camera.Height != 0 ? camera.Height : 720;
        // int fps = camera.Fps != 0 ? camera.Fps : 5; // В новом подходе мы не полагаемся на жесткий FPS из конфига для логики
        
        int frameSize = width * height * 3; // BGR24

        // --- НАСТРОЙКА КАНАЛА (Очередь кадров) ---
        // BoundedChannelOptions:
        // Capacity: макс количество кадров в очереди (буфер). 8-16 - хороший баланс.
        // FullMode: DropOldest - если очередь полна, выкидываем СТАРЫЙ кадр, чтобы новый мог войти.
        // Это критически важно: поток чтения никогда не ждет!
        var channelOptions = new BoundedChannelOptions(16)
        {
            FullMode = BoundedChannelFullMode.DropOldest
        };
        
        var frameChannel = Channel.CreateBounded<Mat>(channelOptions);

        var psi = new ProcessStartInfo
        {
            FileName = "ffmpeg",
            Arguments =
                rtspOpt +
                "-fflags discardcorrupt " +
                "-flags low_delay " +
                "-analyzeduration 1000000 " +
                "-probesize 1000000 " +
                $"-i \"{camera.StreamUrl}\" " +
                "-an -sn -dn " +
                // Убрал фильтр -vf fps={camera.Fps}. Пусть ffmpeg отдает кадры как есть.
                // Пропуск кадров будет контролироваться C# логикой (каналом).
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

            // Игнорируем stderr для краткости, но в продакшене лучше логировать ошибки ffmpeg асинхронно
            _ = Task.Run(async () =>
            {
                string? line;
                while ((line = await process?.StandardError.ReadLineAsync()) != null && !token.IsCancellationRequested)
                {
                    // Можно раскомментировать, если нужны детальные логи ffmpeg
                    // _logger.LogWarning($"[ffmpeg:{camera.Name}] {line}");
                }
            }, token);

            var stream = process.StandardOutput.BaseStream;
            var buffer = new byte[frameSize];

            using var cameraRecognize = camera.Simulate ? null : new CameraRecognize(camera, _logger);
            var plateAnalyse = _serviceProvider.GetRequiredService<PlateAnalyse>();
            plateAnalyse.Init(camera);
            
            // Подготовка текста FPS (вычисляем один раз)
            var font = HersheyFonts.HersheySimplex;
            double fontScale = 0.7;
            int thickness = 2;
            int margin = 50;
            string fpsText = $"FPS: {camera.Fps}"; 
            var textSize = Cv2.GetTextSize(fpsText, font, fontScale, thickness, out int baseline);
            int x = width - textSize.Width - margin;
            int y = margin + textSize.Height;

            // --- ЗАДАЧА ОБРАБОТКИ КАДРОВ (Фоновый воркер) ---
            var processingTask = Task.Run(async () =>
            {
                var sw = Stopwatch.StartNew();
                int processedCount = 0;
                double actualFps = 0.0;
                // Читаем из канала. Если канал закрыт или токен отменен - цикл завершится
                await foreach (var frame in frameChannel.Reader.ReadAllAsync(token))
                {
                    if (!camera.Simulate)
                    {
                        using (frame) // Гарантированный Dispose() даже при ошибке
                        {
                            // 1. Распознавание (тяжелая операция)
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
                            if (!camera.Simulate)
                            {
                                // 2. Отрисовка зоны
                                if (camera.Option.UseArea)
                                {
                                    var border = new Rect(camera.Option.AreaX, camera.Option.AreaY,
                                                        camera.Option.AreaWidth, camera.Option.AreaHeight);
                                    Cv2.Rectangle(frame, border, Scalar.Blue, 1);
                                }

                                // 3. Отрисовка FPS
                                Cv2.PutText(frame,  $"FPS: {Math.Round(actualFps)}", new Point(x, y), font, fontScale, Scalar.White, thickness);
                                
                                // 4. Кодирование и сохранение (тоже тяжелая операция)
                                Cv2.ImEncode(".jpg", frame, out var outBytes, JpegParams);
                                _frameBuffer.SetFrame(camera.Id, outBytes);
                            }
                        }
                    }

                    processedCount++;
                    if (sw.ElapsedMilliseconds >= 1000)
                    {
                        actualFps = processedCount / (sw.ElapsedMilliseconds / 1000.0);
                        _logger.LogDebug("[{Name}] Обработка: {Fps:F1} FPS", camera.Name, actualFps);
                        processedCount = 0;
                        sw.Restart();
                    }
                }
            }, token);

            // --- ЦИКЛ ЧТЕНИЯ КАДРОВ (Максимально быстрый, без блокировок) ---
            while (!token.IsCancellationRequested)
            {
                int read = 0;
                // Читаем ровно столько, сколько нужно для одного кадра
                while (read < frameSize)
                {
                    int r = await stream.ReadAsync(buffer, read, frameSize - read, token);
                    if (r == 0) break; // Конец потока
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

                // Создаем Mat из буфера
                var frame = new Mat(height, width, MatType.CV_8UC3);
                Marshal.Copy(buffer, 0, frame.Data, buffer.Length);

                // Пытаемся записать в канал
                // TryWrite возвращает false, если канал полон. 
                // В этом случае мы НЕ ждем, а сразу уничтожаем кадр и идем дальше.
                if (!frameChannel.Writer.TryWrite(frame))
                {
                    // Канал переполнен (система не успевает обрабатывать).
                    // Мы жертвуем этим кадром, чтобы поток чтения не встал.
                    frame.Dispose(); 
                    // Опционально: можно залогировать факт пропуска, но не часто, чтобы не спамить лог
                    // _logger.LogTrace("Пропущен кадр из-за перегрузки обработчика");
                    continue; 
                }
                // Если TryWrite вернул true, кадр передан в обработку. Dispose() сделает воркер.
            }

            // Сигнализируем воркеру, что новых кадров не будет
            frameChannel.Writer.Complete();

            // Ждем завершения обработки оставшихся кадров в очереди
            try 
            { 
                await processingTask; 
            } 
            catch (OperationCanceledException) 
            { 
                // Нормальная ситуация при остановке
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
                catch { }
            }
            process?.Dispose();
        }
    }

    // Остальные методы (ProcessCamera, StartAsync, StopAsync и т.д.) остаются без изменений,
    // так как они только управляют жизненным циклом ProcessCameraInternal.
    
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
                await Task.Delay(TimeSpan.FromSeconds(5), token);
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
        _logger.LogInformation("Запущено камер: {Count}", cameras.Count);
    }

    public async Task StopAsync()
    {
        if (_workers.IsEmpty) return;
        _logger.LogInformation("Остановка всех камер...");
        foreach (var worker in _workers.Values) worker.Cts?.Cancel();
        
        var tasks = _workers.Values
            .Where(x => x.Task != null)
            .Select(x => x.Task!);

        try { await Task.WhenAll(tasks); }
        catch (OperationCanceledException) {}
        catch (Exception ex) { _logger.LogError(ex, "Ошибка при остановке камер"); }

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

    public void StartCamera(Camera camera)
    {
        if (!camera.Enable) return;
        if (_workers.ContainsKey(camera.Id)) return;

        var cts = new CancellationTokenSource();
        var worker = new CameraWorker { Camera = camera, Cts = cts };

        worker.Task = Task.Run(async () =>
        {
            try { await ProcessCamera(camera, cts.Token); }
            catch (OperationCanceledException) { _logger.LogInformation("Камера {Name} остановлена", camera.Name); }
            catch (Exception ex) 
            { 
                worker.LastError = ex.Message;
                _logger.LogError(ex, "Ошибка камеры {Name}", camera.Name); 
            } 
            finally { _workers.TryRemove(camera.Id, out _); }
        });

        if (!_workers.TryAdd(camera.Id, worker))
        {
            cts.Cancel();
            return;
        }
        _logger.LogInformation("Камера {Name} запущена", camera.Name);
    }

    public async Task StopCamera(int cameraId)
    {
        if (!_workers.TryGetValue(cameraId, out var worker)) return;
        worker.Cts?.Cancel();
        if (worker.Task != null)
        {
            try { await worker.Task; } catch { }
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
        return _workers.TryGetValue(cameraId, out var worker) && worker.IsRunning;
    }
}

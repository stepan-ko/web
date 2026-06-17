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

        var psi = new ProcessStartInfo
        {
            FileName = "ffmpeg",
            Arguments =
                rtspOpt +
                $"-re -i \"{camera.StreamUrl}\" " +
                "-an -sn -dn " +
                "-f image2pipe -vcodec mjpeg -q:v 4 " +
                "-vf scale=1280:720 " +
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
            _ = Task.Run(async () =>
            {
                try
                {
                    string? line;
                    while ((line = await process.StandardError.ReadLineAsync()) != null)
                        _logger.LogWarning($"[ffmpeg:{camera.Name}] {line}");
                }
                catch { /* процесс завершился — это нормально */ }
            });

            var stream = process.StandardOutput.BaseStream;
            using var ms = new MemoryStream();
            var chunk = new byte[8192];

            while (!token.IsCancellationRequested)
            {
                int n = await stream.ReadAsync(chunk, 0, chunk.Length, token);
                if (n == 0)
                {
                    _logger.LogWarning($"[{camera.Name}] FFmpeg stream broken");
                    break;
                }
                ms.Write(chunk, 0, n);

                var data = ms.GetBuffer();
                int len = (int)ms.Length;

                int start = FindMarker(data, len, 0xFF, 0xD8, 0);
                int end = start >= 0 ? FindMarker(data, len, 0xFF, 0xD9, start + 2) : -1;

                if (start < 0 || end <= start)
                    continue;

                int frameLen = end + 2 - start;
                var jpegBytes = new byte[frameLen];
                Array.Copy(data, start, jpegBytes, 0, frameLen);

                using var frame = Cv2.ImDecode(jpegBytes, ImreadModes.Color);

                if (camera.Option.UseArea)
                {
                    var border = new Rect(camera.Option.AreaX, camera.Option.AreaY,
                                        camera.Option.AreaWidth, camera.Option.AreaHeight);
                    Cv2.Rectangle(frame, border, Scalar.Blue, 1);
                }

                Cv2.ImEncode(".jpg", frame, out var outBytes, new[] { (int)ImwriteFlags.JpegQuality, 80 });
                _frameBuffer.SetFrame(camera.Id, outBytes);

                // остаток после EOI переносим в начало буфера — там может быть начало следующего кадра
                int restLen = len - (end + 2);
                var rest = new byte[restLen];
                Array.Copy(data, end + 2, rest, 0, restLen);
                ms.SetLength(0);
                ms.Write(rest, 0, restLen);
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

    static int FindMarker(byte[] buf, int len, byte b1, byte b2, int from)
    {
        for (int i = from; i < len - 1; i++)
            if (buf[i] == b1 && buf[i + 1] == b2) return i;
        return -1;
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
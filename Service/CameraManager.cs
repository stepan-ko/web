using Microsoft.EntityFrameworkCore;
using OpenCvSharp;

public class CameraManager
{
    private readonly IServiceScopeFactory _scopeFactory;

    private readonly Dictionary<int, Task> _cameraTasks = new();
    private CancellationTokenSource? _cts;
    private readonly ILogger<CameraManager> _logger;
    public CameraManager(IServiceScopeFactory scopeFactory, ILogger<CameraManager> logger)
    {
        _scopeFactory = scopeFactory;
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


         // 1. открыть поток
            using var capture = new VideoCapture();
            capture.Open(camera.StreamUrl);
            if (!capture.IsOpened())
            {
                var msg = $"Видеопоток {camera.Id}: '{camera.Name}' не может быть открыт";
                _logger.LogError(msg);                
                throw new Exception(msg);
            }
            else {
               
                 _logger.LogInformation($"Видеопоток {camera.Id}: '{camera.Name}'открыт");  
            }


        while (!token.IsCancellationRequested)
        {
            // 2. получить кадр
            using var frame = new Mat();
            capture.Read(frame);
            if (token.IsCancellationRequested) break;
                        
            if (frame.Empty())
            {
                _logger.LogInformation($"Видеопоток #{camera.Id}: '{camera.Name}' завершился");
                break;
            }

            if (token.IsCancellationRequested) break;

            // Рисуем рамку Arae
            if (camera.Option.UseArea)
            {
               Rect border = new Rect(camera.Option.AreaX, camera.Option.AreaY, camera.Option.AreaWidth, camera.Option.AreaHeight);
               Cv2.Rectangle(frame, border, Scalar.Blue, 1);
            }


            // 3. распознать номер
             

            // 4. передать видео на страницу
           

            await Task.Delay(100, token);
        }
    }


}
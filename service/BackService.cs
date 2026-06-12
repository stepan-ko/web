using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Configuration.VideoConfig;

namespace Server.Service
{
   
public class BackService : BackgroundService
{
    private readonly ILogger<BackService> _logger;
    private readonly VideoConfig _settings;
   
    public BackService(ILogger<BackService> logger, IOptions<VideoConfig> option)
    {
        _logger = logger;
        _settings = options.Value;

        _logger.LogDebug($" BackService is initial");       
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogDebug($"BackService is starting.");        
        
        
        stoppingToken.Register(() => _logger.LogDebug($"BackService background task is stopping."));

        while (!stoppingToken.IsCancellationRequested)
        {
             _logger.LogDebug($"BackService doing background work.");
           
            try
            {
                // Ваша фоновая логика  
               

                 await Task.Delay(1000, stoppingToken); // каждые 1 секунд
            }
            catch (OperationCanceledException)
            {
                // Обработка отмены
                break;
            }
            catch (Exception ex)
            {                
                 _logger.LogDebug($"Ошибка: {ex.Message}");
                await Task.Delay(30000, stoppingToken); // пауза на 30 сек после ошибки
            }
        }
        _logger.LogDebug($"BackService background task is stopping.");
    }
}


}

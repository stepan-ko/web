using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Server.Service
{
   
public class BackService : BackgroundService
{
    private readonly ILogger<BackService> _logger;
    int counter;
    
    public BackService(ILogger<BackService> logger)
    {
        _logger = logger;
        _logger.LogDebug($" BackService is initial.");
        // Constructor's parameters validations...
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
               _logger.LogDebug($"counter = {counter++}");

                 await Task.Delay(10000, stoppingToken); // каждые 10 секунд
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

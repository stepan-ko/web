using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Server.Service
{
   
public class BackService : BackgroundService
{
    private readonly ILogger<BackService> _logger;
    private readonly CameraManager _cameraManager;
    
    public BackService(ILogger<BackService> logger, CameraManager cameraManager)
    {
        _logger = logger;
        _cameraManager = cameraManager;

        _logger.LogDebug($" BackService is initial.");
        // Constructor's parameters validations...
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("BackService starting...");

        stoppingToken.Register(() =>
            _logger.LogInformation("BackService stopping..."));

        try
        {        
            await _cameraManager.StartAsync(stoppingToken);
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fatal error in BackService");
        }

        _logger.LogInformation("BackService stopped.");
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await _cameraManager.StopAsync();
    }


}
}

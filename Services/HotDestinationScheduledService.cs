namespace qisu_server.Services;

public class HotDestinationScheduledService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<HotDestinationScheduledService> _logger;
    private readonly TimeSpan _updateInterval = TimeSpan.FromHours(2);

    public HotDestinationScheduledService(
        IServiceScopeFactory scopeFactory,
        ILogger<HotDestinationScheduledService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("热门目的地定时更新服务已启动（间隔2小时，数据来源：高德POI API）");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                    var updateService = scope.ServiceProvider.GetRequiredService<HotDestinationUpdateService>();
                    await updateService.UpdateHotDestinationsAsync();
                }

                await Task.Delay(_updateInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("热门目的地定时更新服务正在停止");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "热门目的地定时更新任务执行出错");
                await Task.Delay(TimeSpan.FromMinutes(30), stoppingToken);
            }
        }

        _logger.LogInformation("热门目的地定时更新服务已停止");
    }
}

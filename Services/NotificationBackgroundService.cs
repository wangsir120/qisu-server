using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace qisu_server.Services;

public class NotificationBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<NotificationBackgroundService> _logger;

    public NotificationBackgroundService(IServiceProvider serviceProvider, ILogger<NotificationBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("通知后台服务已启动");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var notificationService = scope.ServiceProvider.GetRequiredService<NotificationService>();

                var now = DateTime.Now;
                
                if (now.Hour == 9 && now.Minute == 0)
                {
                    _logger.LogInformation("执行每日通知检查任务");
                    await notificationService.CheckPendingApplicationsAsync();
                    await notificationService.CheckUnreadMessagesAsync();
                }

                if (now.Hour == 18 && now.Minute == 0)
                {
                    _logger.LogInformation("执行每日报告任务");
                    await notificationService.SendDailyReportAsync();
                }

                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "通知后台服务执行出错");
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }

        _logger.LogInformation("通知后台服务已停止");
    }
}

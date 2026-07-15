using Microsoft.EntityFrameworkCore;
using qisu_server.Controllers;
using qisu_server.Data;
using qisu_server.Models;

namespace qisu_server.Services;

public class OrderTimeoutScheduledService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OrderTimeoutScheduledService> _logger;

    private readonly TimeSpan _normalInterval = TimeSpan.FromSeconds(30);
    private readonly TimeSpan _fastInterval = TimeSpan.FromSeconds(10);

    private static readonly string[] SafeToCancelStatuses = { "pending" };

    public OrderTimeoutScheduledService(
        IServiceScopeFactory scopeFactory,
        ILogger<OrderTimeoutScheduledService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "订单超时自动取消服务已启动（正常间隔：{NormalInterval}秒，快速间隔：{FastInterval}秒）",
            _normalInterval.TotalSeconds,
            _fastInterval.TotalSeconds);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    var notificationService = scope.ServiceProvider.GetRequiredService<NotificationService>();
                    var hasExpiredOrders = await CheckAndCancelExpiredOrdersAsync(context, notificationService);

                    var interval = hasExpiredOrders ? _fastInterval : _normalInterval;
                    await Task.Delay(interval, stoppingToken);
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("订单超时自动取消服务正在停止");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "订单超时检查任务执行出错");
                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            }
        }

        _logger.LogInformation("订单超时自动取消服务已停止");
    }

    private async Task<bool> CheckAndCancelExpiredOrdersAsync(AppDbContext context, NotificationService notificationService)
    {
        var now = DateTime.Now;

        var expiredOrders = await context.Orders
            .Where(o => o.Status == "pending" &&
                        o.PayDeadline != null &&
                        o.PayDeadline < now)
            .ToListAsync();

        if (expiredOrders.Count == 0)
        {
            return false;
        }

        _logger.LogInformation(
            "发现 {Count} 个超时未支付订单（当前时间：{Now}），开始自动取消...",
            expiredOrders.Count,
            now.ToString("yyyy-MM-dd HH:mm:ss"));

        int successCount = 0;
        int skippedCount = 0;

        foreach (var order in expiredOrders)
        {
            try
            {
                if (!IsSafeToCancel(order))
                {
                    skippedCount++;
                    _logger.LogWarning(
                        "跳过不安全的订单：OrderNo={OrderNo}, OrderId={Id}, 当前状态={Status}, 原因=状态不允许自动取消",
                        order.OrderNo, order.Id, order.Status);
                    continue;
                }

                var previousStatus = order.Status;
                var overdueSeconds = (now - order.PayDeadline.Value).TotalSeconds;

                order.Status = "cancelled";
                order.CancelReason = $"支付超时，系统自动取消（已超时 {Math.Round(overdueSeconds, 0)} 秒）";
                order.CancelTime = now;
                order.UpdatedAt = now;

                _logger.LogInformation(
                    "自动取消超时订单：OrderNo={OrderNo}, OrderId={Id}, 原状态={PreviousStatus}→cancelled, 创建时间={CreatedAt}, 截止时间={PayDeadline}, 超时时长={OverdueSeconds}秒",
                    order.OrderNo, order.Id, previousStatus, order.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"),
                    order.PayDeadline?.ToString("yyyy-MM-dd HH:mm:ss"), Math.Round(overdueSeconds, 0));

                successCount++;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "处理超时订单失败：OrderId={OrderId}", order.Id);
            }
        }

        await context.SaveChangesAsync();

        _logger.LogInformation(
            "订单超时处理完成 - 成功: {SuccessCount}, 跳过: {SkippedCount}, 总计: {Total}",
            successCount, skippedCount, expiredOrders.Count);

        foreach (var order in expiredOrders.Where(o => o.Status == "cancelled"))
        {
            _logger.LogInformation("开始发送通知：OrderNo={OrderNo}, UserId={UserId}", order.OrderNo, order.UserId);

            try
            {
                var propertyTitle = order.Property?.Title ?? "房源";

                _logger.LogInformation("正在发送SSE实时推送：UserId={UserId}, OrderNo={OrderNo}", order.UserId, order.OrderNo);

                await SseController.NotifyUserAsync(
                    order.UserId,
                    "order_timeout",
                    new
                    {
                        orderId = order.Id,
                        orderNo = order.OrderNo,
                        propertyTitle = propertyTitle,
                        cancelReason = order.CancelReason,
                        cancelledAt = order.CancelTime?.ToString("yyyy-MM-dd HH:mm:ss"),
                        message = $"您的订单 {order.OrderNo} 已因支付超时被系统自动取消"
                    });

                _logger.LogInformation("SSE实时推送完成：OrderNo={OrderNo}", order.OrderNo);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "发送SSE通知失败：OrderId={OrderId}", order.Id);
            }

            try
            {
                _logger.LogInformation("正在创建数据库通知记录：UserId={UserId}, OrderNo={OrderNo}", order.UserId, order.OrderNo);

                await notificationService.NotifyUserAsync(
                    order.UserId,
                    $"订单 {order.OrderNo} 已超时自动取消",
                    $"您预订的「{order.Property?.Title ?? "房源"}」因超过5分钟未完成支付，已被系统自动取消。如有疑问请联系客服。",
                    "warning"
                );

                _logger.LogInformation("数据库通知创建完成：OrderNo={OrderNo}", order.OrderNo);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "创建数据库通知失败：OrderId={OrderId}", order.Id);
            }

            _logger.LogInformation("所有通知发送完成：OrderNo={OrderNo}", order.OrderNo);
        }

        return successCount > 0;
    }

    private bool IsSafeToCancel(Order order)
    {
        if (!SafeToCancelStatuses.Contains(order.Status?.ToLower()))
        {
            return false;
        }

        if (!order.PayDeadline.HasValue)
        {
            _logger.LogWarning("订单没有截止时间，跳过自动取消: OrderId={Id}", order.Id);
            return false;
        }

        if (order.CancelTime.HasValue || !string.IsNullOrEmpty(order.CancelReason))
        {
            _logger.LogWarning("订单已经被取消过，跳过: OrderId={Id}, CancelTime={CancelTime}",
                order.Id, order.CancelTime?.ToString("yyyy-MM-dd HH:mm:ss"));
            return false;
        }

        return true;
    }
}

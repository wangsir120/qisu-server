using qisu_server.Data;
using qisu_server.Models;
using Microsoft.EntityFrameworkCore;

namespace qisu_server.Services;

public class NotificationService
{
    private readonly AppDbContext _context;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(AppDbContext context, ILogger<NotificationService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task CreateNotificationAsync(string title, string? content, string type = "info", long? targetUserId = null, string? targetRole = null)
    {
        try
        {
            var notification = new SystemNotification
            {
                Title = title,
                Content = content,
                Type = type,
                TargetUserId = targetUserId,
                TargetRole = targetRole,
                IsRead = false,
                CreatedAt = DateTime.Now
            };

            _context.SystemNotifications.Add(notification);
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("创建通知成功: {Title}", title);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "创建通知失败: {Title}", title);
        }
    }

    public async Task NotifyAllAdminsAsync(string title, string? content, string type = "info")
    {
        await CreateNotificationAsync(title, content, type, null, "admin");
    }

    public async Task NotifyAllLandlordsAsync(string title, string? content, string type = "info")
    {
        await CreateNotificationAsync(title, content, type, null, "landlord");
    }

    public async Task NotifyUserAsync(long userId, string title, string? content, string type = "info")
    {
        await CreateNotificationAsync(title, content, type, userId, null);
    }

    public async Task CheckPendingApplicationsAsync()
    {
        try
        {
            var pendingCount = await _context.HostApplications
                .Where(h => h.Status == "pending")
                .CountAsync();

            if (pendingCount > 0)
            {
                var existingNotification = await _context.SystemNotifications
                    .Where(n => n.Title.Contains("待审核申请") && n.CreatedAt.Date == DateTime.Today)
                    .FirstOrDefaultAsync();

                if (existingNotification == null)
                {
                    await NotifyAllAdminsAsync(
                        $"您有{pendingCount}条待审核申请",
                        $"当前共有{pendingCount}条房东入驻申请等待审核，请及时处理。",
                        "warning"
                    );
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "检查待审核申请失败");
        }
    }

    public async Task CheckUnreadMessagesAsync()
    {
        try
        {
            var unreadCount = await _context.ChatConversations
                .SumAsync(c => c.UnreadCount);

            if (unreadCount > 0)
            {
                var existingNotification = await _context.SystemNotifications
                    .Where(n => n.Title.Contains("未读消息") && n.CreatedAt.Date == DateTime.Today)
                    .FirstOrDefaultAsync();

                if (existingNotification == null)
                {
                    await NotifyAllAdminsAsync(
                        $"您有{unreadCount}条未读消息",
                        $"当前共有{unreadCount}条客服消息未读，请及时回复用户。",
                        "info"
                    );
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "检查未读消息失败");
        }
    }

    public async Task SendDailyReportAsync()
    {
        try
        {
            var today = DateTime.Today;
            var todayEnd = today.AddDays(1);

            var newUsers = await _context.Users
                .Where(u => u.CreatedAt >= today && u.CreatedAt < todayEnd)
                .CountAsync();

            var newApplications = await _context.HostApplications
                .Where(h => h.CreatedAt >= today && h.CreatedAt < todayEnd)
                .CountAsync();

            await NotifyAllAdminsAsync(
                "每日数据报告",
                $"今日新增用户：{newUsers}人，新增入驻申请：{newApplications}条。",
                "success"
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "发送每日报告失败");
        }
    }
}

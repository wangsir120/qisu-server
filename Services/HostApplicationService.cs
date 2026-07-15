using Microsoft.EntityFrameworkCore;
using qisu_server.Data;
using qisu_server.Models;
using qisu_server.Models.DTOs;

namespace qisu_server.Services;

public interface IHostApplicationService
{
    Task<ApiResponse<HostApplicationStatsDto>> GetStatsAsync();
    Task<ApiResponse<PagedResult<HostApplicationListDto>>> GetListAsync(HostApplicationQueryRequest request);
    Task<ApiResponse<HostApplicationDetailDto>> GetByIdAsync(long id);
    Task<ApiResponse<bool>> AuditAsync(long id, HostApplicationAuditRequest request, long auditorId, string? auditorName = null);
}

public class HostApplicationService : IHostApplicationService
{
    private readonly AppDbContext _context;
    private readonly IOperationLogService _logService;
    private readonly ILogger<HostApplicationService> _logger;

    public HostApplicationService(AppDbContext context, IOperationLogService logService, ILogger<HostApplicationService> logger)
    {
        _context = context;
        _logService = logService;
        _logger = logger;
    }

    public async Task<ApiResponse<HostApplicationStatsDto>> GetStatsAsync()
    {
        var total = await _context.HostApplications.CountAsync();
        var pending = await _context.HostApplications.CountAsync(a => a.Status == "pending");
        var approved = await _context.HostApplications.CountAsync(a => a.Status == "approved");
        var rejected = await _context.HostApplications.CountAsync(a => a.Status == "rejected");

        var stats = new HostApplicationStatsDto
        {
            Total = total,
            Pending = pending,
            Approved = approved,
            Rejected = rejected
        };

        return ApiResponse<HostApplicationStatsDto>.Ok(stats);
    }

    public async Task<ApiResponse<PagedResult<HostApplicationListDto>>> GetListAsync(HostApplicationQueryRequest request)
    {
        var query = _context.HostApplications.AsQueryable();

        if (!string.IsNullOrEmpty(request.Status))
        {
            query = query.Where(a => a.Status == request.Status);
        }

        if (!string.IsNullOrEmpty(request.Keyword))
        {
            query = query.Where(a => 
                (a.Name != null && a.Name.Contains(request.Keyword)) ||
                (a.Phone != null && a.Phone.Contains(request.Keyword)) ||
                (a.IdCard != null && a.IdCard.Contains(request.Keyword)));
        }

        var total = await query.CountAsync();

        var applications = await query
            .OrderByDescending(a => a.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(a => new HostApplicationListDto
            {
                Id = a.Id,
                UserId = a.UserId,
                Name = a.Name,
                Phone = a.Phone,
                IdCard = a.IdCard,
                Status = a.Status,
                AuditRemark = a.AuditRemark,
                AuditedAt = a.AuditedAt,
                CreatedAt = a.CreatedAt
            })
            .ToListAsync();

        var result = new PagedResult<HostApplicationListDto>
        {
            Items = applications,
            Total = total,
            Page = request.Page,
            PageSize = request.PageSize
        };

        return ApiResponse<PagedResult<HostApplicationListDto>>.Ok(result);
    }

    public async Task<ApiResponse<HostApplicationDetailDto>> GetByIdAsync(long id)
    {
        var application = await _context.HostApplications.FindAsync(id);
        if (application == null)
        {
            return ApiResponse<HostApplicationDetailDto>.Fail("申请记录不存在");
        }

        string? auditorName = null;
        if (application.AuditorId.HasValue)
        {
            var auditor = await _context.Admins.FindAsync(application.AuditorId.Value);
            auditorName = auditor?.Name ?? auditor?.Username;
        }

        var detail = new HostApplicationDetailDto
        {
            Id = application.Id,
            UserId = application.UserId,
            Name = application.Name,
            Phone = application.Phone,
            IdCard = application.IdCard,
            Email = application.Email,
            Province = application.Province,
            City = application.City,
            District = application.District,
            Address = application.Address,
            PropertyType = application.PropertyType,
            RoomCount = application.RoomCount,
            BedCount = application.BedCount,
            GuestCount = application.GuestCount,
            PropertyTitle = application.PropertyTitle,
            PropertyDesc = application.PropertyDesc,
            Amenities = application.Amenities,
            Images = application.Images,
            Status = application.Status,
            AuditRemark = application.AuditRemark,
            AuditorId = application.AuditorId,
            AuditorName = auditorName,
            AuditedAt = application.AuditedAt,
            CreatedAt = application.CreatedAt
        };

        return ApiResponse<HostApplicationDetailDto>.Ok(detail);
    }

    public async Task<ApiResponse<bool>> AuditAsync(long id, HostApplicationAuditRequest request, long auditorId, string? auditorName = null)
    {
        var application = await _context.HostApplications.FindAsync(id);
        if (application == null)
        {
            return ApiResponse<bool>.Fail("申请记录不存在");
        }

        if (application.Status != "pending")
        {
            return ApiResponse<bool>.Fail("该申请已审核，无法重复审核");
        }

        var statusText = request.Status == "approved" ? "通过" : "拒绝";
        var description = $"审核房东申请(ID:{id}), 申请人:{application.Name}, 结果:{statusText}";
        if (!string.IsNullOrEmpty(request.AuditRemark))
        {
            description += $", 备注:{request.AuditRemark}";
        }

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            application.Status = request.Status;
            application.AuditRemark = request.AuditRemark;
            application.AuditorId = auditorId;
            application.AuditedAt = DateTime.Now;
            application.UpdatedAt = DateTime.Now;

            if (request.Status == "approved")
            {
                var user = await _context.Users.FindAsync(application.UserId);

                var host = new Models.Host
                {
                    UserId = application.UserId,
                    Name = application.Name,
                    Phone = application.Phone,
                    Avatar = (user != null && !string.IsNullOrEmpty(user.Avatar)) ? user.Avatar : null,
                    Verified = true,
                    Status = 1,
                    Rating = 0,
                    TotalListings = 0,
                    TotalReviews = 0,
                    ResponseRate = 0,
                    IsSuperhost = false,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };
                _context.Hosts.Add(host);

                if (application.UserId.HasValue)
                {
                    if (user != null)
                    {
                        user.IsVerified = true;
                        if (!string.IsNullOrEmpty(application.IdCard))
                        {
                            user.IdCard = application.IdCard;
                        }
                        user.UpdatedAt = DateTime.Now;
                    }

                    var message = new Models.Message
                    {
                        UserId = application.UserId.Value,
                        Title = "房东申请审核通过",
                        Content = "恭喜！您的房东申请已通过审核，现在可以登录后台系统发布房源了。",
                        Type = "system",
                        IsRead = false,
                        CreatedAt = DateTime.Now
                    };
                    _context.Messages.Add(message);
                    _logger.LogInformation("插入消息: UserId={UserId}, Title={Title}", application.UserId.Value, message.Title);
                }
            }
            else if (request.Status == "rejected")
            {
                if (application.UserId.HasValue)
                {
                    var message = new Models.Message
                    {
                        UserId = application.UserId.Value,
                        Title = "房东申请审核未通过",
                        Content = $"很抱歉，您的房东申请未通过审核。原因：{request.AuditRemark ?? "无"}",
                        Type = "system",
                        IsRead = false,
                        CreatedAt = DateTime.Now
                    };
                    _context.Messages.Add(message);
                }
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("消息已保存到数据库");
            await transaction.CommitAsync();

            await _logService.LogAuditAsync(auditorId, auditorName, "房东申请", id, "审核", description, "", true);

            if (application.UserId.HasValue)
            {
                var notificationTitle = request.Status == "approved" 
                    ? "房东申请审核通过" 
                    : "房东申请审核未通过";
                var notificationContent = request.Status == "approved"
                    ? "恭喜！您的房东申请已通过审核，现在可以登录后台系统发布房源了。"
                    : $"很抱歉，您的房东申请未通过审核。原因：{request.AuditRemark ?? "无"}";

                await Controllers.SseController.NotifyUserAsync(
                    application.UserId.Value,
                    "message",
                    new { title = notificationTitle, content = notificationContent, type = "system" }
                );
            }

            return ApiResponse<bool>.Ok(true, "审核完成");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "审核失败");
            await _logService.LogAuditAsync(auditorId, auditorName, "房东申请", id, "审核", description, "", false, ex.Message);
            return ApiResponse<bool>.Fail("审核失败：" + ex.Message);
        }
    }
}

using Microsoft.EntityFrameworkCore;
using qisu_server.Data;
using qisu_server.Models;
using qisu_server.Models.DTOs;
using System.Text.Json;

namespace qisu_server.Services;

public interface IHostApplyService
{
    Task<ApiResponse<bool>> ApplyAsync(long userId, HostApplyRequest request);
    Task<ApiResponse<HostApplyStatusDto>> GetStatusAsync(long userId);
}

public class HostApplyService : IHostApplyService
{
    private readonly AppDbContext _context;
    private readonly IIdCardService _idCardService;
    private readonly ILogger<HostApplyService> _logger;

    public HostApplyService(AppDbContext context, IIdCardService idCardService, ILogger<HostApplyService> logger)
    {
        _context = context;
        _idCardService = idCardService;
        _logger = logger;
    }

    public async Task<ApiResponse<bool>> ApplyAsync(long userId, HostApplyRequest request)
    {
        var existingApply = await _context.HostApplications
            .FirstOrDefaultAsync(a => a.UserId == userId && a.Status == "pending");
        
        if (existingApply != null)
        {
            return ApiResponse<bool>.Fail("您已有待审核的申请，请等待审核结果");
        }

        var approvedApply = await _context.HostApplications
            .FirstOrDefaultAsync(a => a.UserId == userId && a.Status == "approved");
        
        if (approvedApply != null)
        {
            return ApiResponse<bool>.Fail("您已是房东，无需重复申请");
        }

        var verifyResult = await _idCardService.VerifyAsync(request.RealName, request.IdCard);
        if (!verifyResult.Success || !verifyResult.Data?.IsMatch == true)
        {
            return ApiResponse<bool>.Fail("身份证信息验证失败，请确认姓名和身份证号是否正确");
        }

        var application = new HostApplication
        {
            UserId = userId,
            Name = request.RealName,
            Phone = request.Phone,
            Email = request.Email,
            IdCard = request.IdCard,
            Province = request.Province,
            City = request.City,
            District = request.District,
            Address = request.Address,
            PropertyType = request.PropertyType,
            RoomCount = request.RoomCount,
            BedCount = request.BedCount,
            GuestCount = request.GuestCount,
            PropertyTitle = request.PropertyTitle,
            PropertyDesc = request.PropertyDesc,
            Amenities = request.Amenities != null ? JsonSerializer.Serialize(request.Amenities) : null,
            Images = request.Images != null ? JsonSerializer.Serialize(request.Images) : null,
            Status = "pending",
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        };

        _context.HostApplications.Add(application);
        await _context.SaveChangesAsync();

        return ApiResponse<bool>.Ok(true, "申请提交成功，我们将在3个工作日内审核");
    }

    public async Task<ApiResponse<HostApplyStatusDto>> GetStatusAsync(long userId)
    {
        var application = await _context.HostApplications
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.CreatedAt)
            .FirstOrDefaultAsync();

        if (application == null)
        {
            return ApiResponse<HostApplyStatusDto>.Ok(new HostApplyStatusDto
            {
                HasApplied = false,
                Status = null
            });
        }

        return ApiResponse<HostApplyStatusDto>.Ok(new HostApplyStatusDto
        {
            HasApplied = true,
            Status = application.Status,
            AuditRemark = application.AuditRemark,
            CreatedAt = application.CreatedAt,
            AuditedAt = application.AuditedAt,
            PropertyTitle = application.PropertyTitle,
            PropertyType = application.PropertyType,
            Province = application.Province,
            City = application.City,
            District = application.District,
            Address = application.Address
        });
    }
}

public class HostApplyStatusDto
{
    public bool HasApplied { get; set; }
    public string? Status { get; set; }
    public string? AuditRemark { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? AuditedAt { get; set; }
    public string? PropertyTitle { get; set; }
    public string? PropertyType { get; set; }
    public string? Province { get; set; }
    public string? City { get; set; }
    public string? District { get; set; }
    public string? Address { get; set; }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using qisu_server.Data;
using qisu_server.Models;
using qisu_server.Models.DTOs;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace qisu_server.Controllers;

[ApiController]
[Route("api/finance")]
[Authorize]
public class FinanceController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<FinanceController> _logger;

    public FinanceController(AppDbContext context, ILogger<FinanceController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// 获取收支明细列表
    /// </summary>
    [HttpGet("bills")]
    public async Task<ApiResponse<object>> GetBillList([FromQuery] BillQueryDto query)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return ApiResponse<object>.Fail("用户未登录");

            var hostId = await GetHostId(userId.Value);
            if (hostId == null)
                return ApiResponse<object>.Fail("非房东用户");

            var billsQuery = _context.Bills.Where(b => b.HostId == hostId.Value);

            if (!string.IsNullOrEmpty(query.Type))
                billsQuery = billsQuery.Where(b => b.Type == query.Type);

            if (!string.IsNullOrEmpty(query.Category))
                billsQuery = billsQuery.Where(b => b.Category == query.Category);

            if (!string.IsNullOrEmpty(query.StartDate) && DateTime.TryParse(query.StartDate, out var startDate))
                billsQuery = billsQuery.Where(b => b.CreatedAt >= startDate);

            if (!string.IsNullOrEmpty(query.EndDate) && DateTime.TryParse(query.EndDate, out var endDate))
                billsQuery = billsQuery.Where(b => b.CreatedAt <= endDate.AddDays(1));

            var total = await billsQuery.CountAsync();

            var list = await billsQuery
                .OrderByDescending(b => b.CreatedAt)
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .Select(b => new
                {
                    b.Id,
                    b.Type,
                    b.Category,
                    b.Amount,
                    b.OrderNo,
                    b.GuestName,
                    b.PayMethod,
                    b.Status,
                    b.Remark,
                    CreateTime = b.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss")
                })
                .ToListAsync();

            return ApiResponse<object>.Ok(new
            {
                list,
                total,
                page = query.Page,
                pageSize = query.PageSize
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取收支明细失败");
            return ApiResponse<object>.Fail("获取收支明细失败");
        }
    }

    /// <summary>
    /// 获取收支统计
    /// </summary>
    [HttpGet("bills/stats")]
    public async Task<ApiResponse<object>> GetBillStats([FromQuery] BillQueryDto query)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return ApiResponse<object>.Fail("用户未登录");

            var hostId = await GetHostId(userId.Value);
            if (hostId == null)
                return ApiResponse<object>.Fail("非房东用户");

            var billsQuery = _context.Bills.Where(b => b.HostId == hostId.Value);

            if (!string.IsNullOrEmpty(query.Type))
                billsQuery = billsQuery.Where(b => b.Type == query.Type);

            if (!string.IsNullOrEmpty(query.Category))
                billsQuery = billsQuery.Where(b => b.Category == query.Category);

            if (!string.IsNullOrEmpty(query.StartDate) && DateTime.TryParse(query.StartDate, out var startDate))
                billsQuery = billsQuery.Where(b => b.CreatedAt >= startDate);

            if (!string.IsNullOrEmpty(query.EndDate) && DateTime.TryParse(query.EndDate, out var endDate))
                billsQuery = billsQuery.Where(b => b.CreatedAt <= endDate.AddDays(1));

            var totalIncome = await billsQuery.Where(b => b.Type == "income").SumAsync(b => b.Amount);
            var totalExpense = await billsQuery.Where(b => b.Type == "expense").SumAsync(b => b.Amount);
            var pendingAmount = await billsQuery.Where(b => b.Status == "pending").SumAsync(b => b.Amount);

            return ApiResponse<object>.Ok(new
            {
                totalIncome,
                totalExpense,
                pendingAmount
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取收支统计失败");
            return ApiResponse<object>.Fail("获取收支统计失败");
        }
    }

    /// <summary>
    /// 新增收支记录
    /// </summary>
    [HttpPost("bills")]
    public async Task<ApiResponse<object>> AddBill([FromBody] BillCreateDto dto)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return ApiResponse<object>.Fail("用户未登录");

            var hostId = await GetHostId(userId.Value);
            if (hostId == null)
                return ApiResponse<object>.Fail("非房东用户");

            var bill = new Bill
            {
                HostId = hostId.Value,
                Type = dto.Type,
                Category = dto.Category,
                Amount = dto.Amount,
                PayMethod = dto.PayMethod,
                Status = "completed",
                Remark = dto.Remark,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            _context.Bills.Add(bill);
            await _context.SaveChangesAsync();

            return ApiResponse<object>.Ok(new { bill.Id }, "记录添加成功");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "新增收支记录失败");
            return ApiResponse<object>.Fail("新增收支记录失败");
        }
    }

    /// <summary>
    /// 导出收支明细
    /// </summary>
    [HttpGet("bills/export")]
    public async Task<IActionResult> ExportBills([FromQuery] BillQueryDto query)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized();

            var hostId = await GetHostId(userId.Value);
            if (hostId == null)
                return Forbid();

            var billsQuery = _context.Bills.Where(b => b.HostId == hostId.Value);

            if (!string.IsNullOrEmpty(query.Type))
                billsQuery = billsQuery.Where(b => b.Type == query.Type);

            if (!string.IsNullOrEmpty(query.Category))
                billsQuery = billsQuery.Where(b => b.Category == query.Category);

            if (!string.IsNullOrEmpty(query.StartDate) && DateTime.TryParse(query.StartDate, out var startDate))
                billsQuery = billsQuery.Where(b => b.CreatedAt >= startDate);

            if (!string.IsNullOrEmpty(query.EndDate) && DateTime.TryParse(query.EndDate, out var endDate))
                billsQuery = billsQuery.Where(b => b.CreatedAt <= endDate.AddDays(1));

            var bills = await billsQuery.OrderByDescending(b => b.CreatedAt).ToListAsync();

            var csv = new System.Text.StringBuilder();
            csv.AppendLine("交易时间,类型,收支项目,金额,关联订单,关联住客,支付方式,状态,备注");
            foreach (var b in bills)
            {
                var typeText = b.Type == "income" ? "收入" : "支出";
                var statusText = b.Status switch
                {
                    "completed" => "已完成",
                    "pending" => "待结算",
                    "failed" => "失败",
                    _ => b.Status
                };
                csv.AppendLine($"{b.CreatedAt:yyyy-MM-dd HH:mm:ss},{typeText},{b.Category},{b.Amount},{b.OrderNo ?? ""},{b.GuestName ?? ""},{b.PayMethod ?? ""},{statusText},{b.Remark ?? ""}");
            }

            var bytes = System.Text.Encoding.UTF8.GetBytes(csv.ToString());
            return File(bytes, "text/csv", $"收支明细_{DateTime.Now:yyyyMMddHHmmss}.csv");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "导出收支明细失败");
            return StatusCode(500, "导出失败");
        }
    }

    #region Private Methods

    private long? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim))
            userIdClaim = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        if (string.IsNullOrEmpty(userIdClaim))
            userIdClaim = User.FindFirst("sub")?.Value;
        return string.IsNullOrEmpty(userIdClaim) ? null : long.TryParse(userIdClaim, out var id) ? id : null;
    }

    private async Task<long?> GetHostId(long userId)
    {
        var host = await _context.Hosts.FirstOrDefaultAsync(h => h.UserId == userId);
        return host?.Id;
    }

    #endregion
}

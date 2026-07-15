using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using qisu_server.Data;
using qisu_server.Models;
using qisu_server.Models.DTOs;

namespace qisu_server.Controllers;

/// <summary>
/// 操作日志管理控制器
/// </summary>
[ApiController]
[Route("api/admin/logs")]
[Authorize]
public class OperationLogsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<OperationLogsController> _logger;

    public OperationLogsController(AppDbContext context, ILogger<OperationLogsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// 获取操作日志列表
    /// </summary>
    /// <param name="page">页码，默认1</param>
    /// <param name="pageSize">每页数量，默认10</param>
    /// <param name="type">类型筛选：login-登录，logout-退出，register-注册，security-安全，audit-审核，user_manage-用户管理，system-系统</param>
    /// <param name="operatorName">操作人名称，支持模糊搜索</param>
    /// <param name="startTime">开始时间</param>
    /// <param name="endTime">结束时间</param>
    /// <param name="status">状态筛选：success-成功，fail-失败</param>
    /// <returns>分页的日志列表</returns>
    [HttpGet]
    public async Task<ApiResponse<PagedResult<OperationLogDto>>> GetList(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? type = null,
        [FromQuery] string? operatorName = null,
        [FromQuery] DateTime? startTime = null,
        [FromQuery] DateTime? endTime = null,
        [FromQuery] string? status = null)
    {
        var query = _context.OperationLogs.AsQueryable();

        if (!string.IsNullOrEmpty(type))
        {
            query = query.Where(l => l.Type == type);
        }

        if (!string.IsNullOrEmpty(operatorName))
        {
            query = query.Where(l => l.OperatorName != null && l.OperatorName.Contains(operatorName));
        }

        if (!string.IsNullOrEmpty(status))
        {
            query = query.Where(l => l.Status == status);
        }

        if (startTime.HasValue)
        {
            query = query.Where(l => l.CreatedAt >= startTime.Value);
        }

        if (endTime.HasValue)
        {
            query = query.Where(l => l.CreatedAt <= endTime.Value);
        }

        query = query.OrderByDescending(l => l.CreatedAt);

        var total = await query.CountAsync();

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(l => new OperationLogDto
            {
                Id = l.Id,
                Type = l.Type,
                OperatorId = l.OperatorId,
                OperatorName = l.OperatorName,
                Description = l.Description,
                IpAddress = l.IpAddress,
                Location = l.Location,
                Browser = l.Browser,
                Os = l.Os,
                RequestUrl = l.RequestUrl,
                RequestMethod = l.RequestMethod,
                RequestParams = l.RequestParams,
                ResponseCode = l.ResponseCode,
                Status = l.Status,
                ErrorMessage = l.ErrorMessage,
                Duration = l.Duration,
                CreatedAt = l.CreatedAt
            })
            .ToListAsync();

        var result = new PagedResult<OperationLogDto>
        {
            Items = items,
            Total = total,
            Page = page,
            PageSize = pageSize
        };

        return ApiResponse<PagedResult<OperationLogDto>>.Ok(result);
    }

    /// <summary>
    /// 获取操作日志详情
    /// </summary>
    /// <param name="id">日志ID</param>
    /// <returns>日志详情信息</returns>
    [HttpGet("{id}")]
    public async Task<ApiResponse<OperationLogDto>> GetById(long id)
    {
        var log = await _context.OperationLogs.FindAsync(id);
        if (log == null)
        {
            return ApiResponse<OperationLogDto>.Fail("日志不存在");
        }

        var dto = new OperationLogDto
        {
            Id = log.Id,
            Type = log.Type,
            OperatorId = log.OperatorId,
            OperatorName = log.OperatorName,
            Description = log.Description,
            IpAddress = log.IpAddress,
            Location = log.Location,
            Browser = log.Browser,
            Os = log.Os,
            RequestUrl = log.RequestUrl,
            RequestMethod = log.RequestMethod,
            RequestParams = log.RequestParams,
            ResponseCode = log.ResponseCode,
            Status = log.Status,
            ErrorMessage = log.ErrorMessage,
            Duration = log.Duration,
            CreatedAt = log.CreatedAt
        };

        return ApiResponse<OperationLogDto>.Ok(dto);
    }

    /// <summary>
    /// 导出操作日志
    /// </summary>
    /// <param name="type">类型筛选</param>
    /// <param name="operatorName">操作人名称，支持模糊搜索</param>
    /// <param name="startTime">开始时间</param>
    /// <param name="endTime">结束时间</param>
    /// <returns>CSV文件下载</returns>
    /// <remarks>
    /// 最多导出10000条记录
    /// </remarks>
    [HttpGet("export")]
    public async Task<IActionResult> Export(
        [FromQuery] string? type = null,
        [FromQuery] string? operatorName = null,
        [FromQuery] DateTime? startTime = null,
        [FromQuery] DateTime? endTime = null)
    {
        var query = _context.OperationLogs.AsQueryable();

        if (!string.IsNullOrEmpty(type))
        {
            query = query.Where(l => l.Type == type);
        }

        if (!string.IsNullOrEmpty(operatorName))
        {
            query = query.Where(l => l.OperatorName != null && l.OperatorName.Contains(operatorName));
        }

        if (startTime.HasValue)
        {
            query = query.Where(l => l.CreatedAt >= startTime.Value);
        }

        if (endTime.HasValue)
        {
            query = query.Where(l => l.CreatedAt <= endTime.Value);
        }

        var logs = await query
            .OrderByDescending(l => l.CreatedAt)
            .Take(10000)
            .ToListAsync();

        var csv = new System.Text.StringBuilder();
        csv.AppendLine("ID,操作类型,操作人,操作描述,IP地址,登录地址,浏览器,操作系统,状态,操作时间");

        foreach (var log in logs)
        {
            csv.AppendLine($"{log.Id},{log.Type},{log.OperatorName},{log.Description},{log.IpAddress},{log.Location},{log.Browser},{log.Os},{log.Status},{log.CreatedAt:yyyy-MM-dd HH:mm:ss}");
        }

        var bytes = System.Text.Encoding.UTF8.GetBytes(csv.ToString());
        return File(bytes, "text/csv", $"operation_logs_{DateTime.Now:yyyyMMddHHmmss}.csv");
    }
}

/// <summary>
/// 操作日志数据传输对象
/// </summary>
public class OperationLogDto
{
    /// <summary>
    /// 日志ID
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 操作类型
    /// </summary>
    public string Type { get; set; } = "system";

    /// <summary>
    /// 操作人用户ID
    /// </summary>
    public long? OperatorId { get; set; }

    /// <summary>
    /// 操作人用户名
    /// </summary>
    public string? OperatorName { get; set; }

    /// <summary>
    /// 操作描述
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 操作者IP地址
    /// </summary>
    public string? IpAddress { get; set; }

    /// <summary>
    /// IP地址对应的地理位置
    /// </summary>
    public string? Location { get; set; }

    /// <summary>
    /// 浏览器信息
    /// </summary>
    public string? Browser { get; set; }

    /// <summary>
    /// 操作系统信息
    /// </summary>
    public string? Os { get; set; }

    /// <summary>
    /// 请求URL路径
    /// </summary>
    public string? RequestUrl { get; set; }

    /// <summary>
    /// 请求HTTP方法
    /// </summary>
    public string? RequestMethod { get; set; }

    /// <summary>
    /// 请求参数
    /// </summary>
    public string? RequestParams { get; set; }

    /// <summary>
    /// HTTP响应状态码
    /// </summary>
    public int? ResponseCode { get; set; }

    /// <summary>
    /// 操作状态：success-成功、fail-失败
    /// </summary>
    public string Status { get; set; } = "success";

    /// <summary>
    /// 错误信息
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// 请求耗时（毫秒）
    /// </summary>
    public int? Duration { get; set; }

    /// <summary>
    /// 日志创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using qisu_server.Data;
using qisu_server.Models.DTOs;

namespace qisu_server.Controllers;

/// <summary>
/// 公开系统配置接口控制器（无需登录即可访问）
/// </summary>
[ApiController]
[Route("api/system-configs")]
public class PublicSystemConfigsController : ControllerBase
{
    private readonly AppDbContext _context;

    public PublicSystemConfigsController(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// 获取公开的系统配置信息
    /// </summary>
    /// <returns>系统配置字典，包含站点名称、标语、图标、联系方式等</returns>
    [HttpGet]
    public async Task<ApiResponse<Dictionary<string, string>>> GetPublicConfigs()
    {
        var publicKeys = new[] { "site_name", "site_slogan", "site_icon", "contact_phone", "contact_email", "address" };
        
        var configs = await _context.SystemConfigs
            .AsNoTracking()
            .Where(c => publicKeys.Contains(c.ConfigKey))
            .ToListAsync();
            
        var result = configs.ToDictionary(c => c.ConfigKey, c => c.ConfigValue ?? "");
        return ApiResponse<Dictionary<string, string>>.Ok(result);
    }
}

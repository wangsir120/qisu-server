using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using qisu_server.Data;
using qisu_server.Models;
using qisu_server.Models.DTOs;

namespace qisu_server.Controllers;

/// <summary>
/// 系统配置管理控制器（管理员后台使用）
/// </summary>
[ApiController]
[Route("api/admin/system-configs")]
[Authorize]
public class SystemConfigsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<SystemConfigsController> _logger;

    public SystemConfigsController(AppDbContext context, ILogger<SystemConfigsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// 获取所有系统配置项
    /// </summary>
    /// <returns>所有系统配置的键值对字典</returns>
    [HttpGet]
    public async Task<ApiResponse<Dictionary<string, string>>> GetAll()
    {
        var configs = await _context.SystemConfigs
            .AsNoTracking()
            .ToListAsync();
        var result = configs.ToDictionary(c => c.ConfigKey, c => c.ConfigValue ?? "");
        return ApiResponse<Dictionary<string, string>>.Ok(result);
    }

    /// <summary>
    /// 根据配置键获取单个配置项
    /// </summary>
    /// <param name="key">配置键</param>
    /// <returns>配置项详情</returns>
    [HttpGet("{key}")]
    public async Task<ApiResponse<SystemConfigDto>> GetByKey(string key)
    {
        var config = await _context.SystemConfigs.FirstOrDefaultAsync(c => c.ConfigKey == key);
        if (config == null)
        {
            return ApiResponse<SystemConfigDto>.Fail("配置项不存在");
        }

        var dto = new SystemConfigDto
        {
            Key = config.ConfigKey,
            Value = config.ConfigValue ?? "",
            Description = config.Description
        };

        return ApiResponse<SystemConfigDto>.Ok(dto);
    }

    /// <summary>
    /// 批量更新系统配置
    /// </summary>
    /// <param name="configs">配置键值对字典</param>
    /// <returns>更新结果</returns>
    [HttpPut]
    public async Task<ApiResponse<bool>> UpdateConfigs([FromBody] Dictionary<string, string> configs)
    {
        foreach (var kvp in configs)
        {
            var config = await _context.SystemConfigs.FirstOrDefaultAsync(c => c.ConfigKey == kvp.Key);
            if (config != null)
            {
                config.ConfigValue = kvp.Value;
                config.UpdatedAt = DateTime.Now;
            }
            else
            {
                _context.SystemConfigs.Add(new SystemConfig
                {
                    ConfigKey = kvp.Key,
                    ConfigValue = kvp.Value,
                    CreatedAt = DateTime.Now
                });
            }
        }

        await _context.SaveChangesAsync();
        return ApiResponse<bool>.Ok(true, "配置更新成功");
    }

    /// <summary>
    /// 根据配置键更新单个配置项
    /// </summary>
    /// <param name="key">配置键</param>
    /// <param name="request">更新请求，包含配置值和描述</param>
    /// <returns>更新结果</returns>
    [HttpPut("{key}")]
    public async Task<ApiResponse<bool>> UpdateByKey(string key, [FromBody] UpdateConfigRequest request)
    {
        var config = await _context.SystemConfigs.FirstOrDefaultAsync(c => c.ConfigKey == key);
        if (config == null)
        {
            config = new SystemConfig
            {
                ConfigKey = key,
                ConfigValue = request.Value,
                Description = request.Description,
                CreatedAt = DateTime.Now
            };
            _context.SystemConfigs.Add(config);
        }
        else
        {
            config.ConfigValue = request.Value;
            if (request.Description != null)
            {
                config.Description = request.Description;
            }
            config.UpdatedAt = DateTime.Now;
        }

        await _context.SaveChangesAsync();
        return ApiResponse<bool>.Ok(true, "配置更新成功");
    }
}

public class SystemConfigDto
{
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class UpdateConfigRequest
{
    public string Value { get; set; } = string.Empty;
    public string? Description { get; set; }
}

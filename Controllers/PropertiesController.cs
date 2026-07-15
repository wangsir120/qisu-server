using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using qisu_server.Models.DTOs;
using qisu_server.Services;

namespace qisu_server.Controllers;

[ApiController]
[Route("api/host/properties")]
[Authorize]
public class PropertiesController : ControllerBase
{
    private readonly IPropertyService _propertyService;
    private readonly ILogger<PropertiesController> _logger;

    public PropertiesController(IPropertyService propertyService, ILogger<PropertiesController> logger)
    {
        _propertyService = propertyService;
        _logger = logger;
    }

    [HttpGet("list")]
    public async Task<ApiResponse<PagedResult<PropertyListDto>>> GetList([FromQuery] PropertyQueryRequest request)
    {
        var hostId = await GetCurrentHostId();
        if (hostId == null)
        {
            return ApiResponse<PagedResult<PropertyListDto>>.Fail("房东信息不存在");
        }
        return await _propertyService.GetListAsync(hostId.Value, request);
    }

    [HttpGet("{id}")]
    public async Task<ApiResponse<PropertyListDto>> GetById(long id)
    {
        var hostId = await GetCurrentHostId();
        if (hostId == null)
        {
            return ApiResponse<PropertyListDto>.Fail("房东信息不存在");
        }
        return await _propertyService.GetByIdAsync(hostId.Value, id);
    }

    [HttpPost]
    public async Task<ApiResponse<PropertyListDto>> Create([FromBody] PropertyCreateRequest request)
    {
        var hostId = await GetCurrentHostId();
        if (hostId == null)
        {
            return ApiResponse<PropertyListDto>.Fail("房东信息不存在");
        }
        return await _propertyService.CreateAsync(hostId.Value, request);
    }

    [HttpPut("{id}")]
    public async Task<ApiResponse<PropertyListDto>> Update(long id, [FromBody] PropertyUpdateRequest request)
    {
        var hostId = await GetCurrentHostId();
        if (hostId == null)
        {
            return ApiResponse<PropertyListDto>.Fail("房东信息不存在");
        }
        return await _propertyService.UpdateAsync(hostId.Value, id, request);
    }

    [HttpDelete("{id}")]
    public async Task<ApiResponse<bool>> Delete(long id)
    {
        var hostId = await GetCurrentHostId();
        if (hostId == null)
        {
            return ApiResponse<bool>.Fail("房东信息不存在");
        }
        return await _propertyService.DeleteAsync(hostId.Value, id);
    }

    [HttpPost("batch-delete")]
    public async Task<ApiResponse<bool>> BatchDelete([FromBody] BatchDeleteRequest request)
    {
        var hostId = await GetCurrentHostId();
        if (hostId == null)
        {
            return ApiResponse<bool>.Fail("房东信息不存在");
        }
        return await _propertyService.BatchDeleteAsync(hostId.Value, request.Ids);
    }

    private async Task<long?> GetCurrentHostId()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim))
        {
            userIdClaim = User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value;
        }
        if (string.IsNullOrEmpty(userIdClaim))
        {
            userIdClaim = User.FindFirst("sub")?.Value;
        }
        if (string.IsNullOrEmpty(userIdClaim) || !long.TryParse(userIdClaim, out var userId))
        {
            return null;
        }

        var dataContext = HttpContext.RequestServices.GetRequiredService<qisu_server.Data.AppDbContext>();
        var host = await dataContext.Hosts.FirstOrDefaultAsync(h => h.UserId == userId);
        return host?.Id;
    }
}

public class BatchDeleteRequest
{
    public List<long> Ids { get; set; } = new();
}

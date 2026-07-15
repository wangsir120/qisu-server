using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using qisu_server.Models.DTOs;
using qisu_server.Services;

namespace qisu_server.Controllers;

[ApiController]
[Route("api/host/rooms")]
[Authorize]
public class RoomsController : ControllerBase
{
    private readonly IRoomService _roomService;
    private readonly ILogger<RoomsController> _logger;

    public RoomsController(IRoomService roomService, ILogger<RoomsController> logger)
    {
        _roomService = roomService;
        _logger = logger;
    }

    [HttpGet("list")]
    public async Task<ApiResponse<PagedResult<RoomListDto>>> GetList([FromQuery] RoomQueryRequest request)
    {
        var hostId = await GetCurrentHostId();
        if (hostId == null)
        {
            return ApiResponse<PagedResult<RoomListDto>>.Fail("房东信息不存在");
        }
        return await _roomService.GetListAsync(hostId.Value, request);
    }

    [HttpGet("{id}")]
    public async Task<ApiResponse<RoomListDto>> GetById(long id)
    {
        var hostId = await GetCurrentHostId();
        if (hostId == null)
        {
            return ApiResponse<RoomListDto>.Fail("房东信息不存在");
        }
        return await _roomService.GetByIdAsync(hostId.Value, id);
    }

    [HttpPost]
    public async Task<ApiResponse<RoomListDto>> Create([FromBody] RoomCreateRequest request)
    {
        var hostId = await GetCurrentHostId();
        if (hostId == null)
        {
            return ApiResponse<RoomListDto>.Fail("房东信息不存在");
        }
        return await _roomService.CreateAsync(hostId.Value, request);
    }

    [HttpPut("{id}")]
    public async Task<ApiResponse<RoomListDto>> Update(long id, [FromBody] RoomUpdateRequest request)
    {
        var hostId = await GetCurrentHostId();
        if (hostId == null)
        {
            return ApiResponse<RoomListDto>.Fail("房东信息不存在");
        }
        return await _roomService.UpdateAsync(hostId.Value, id, request);
    }

    [HttpDelete("{id}")]
    public async Task<ApiResponse<bool>> Delete(long id)
    {
        var hostId = await GetCurrentHostId();
        if (hostId == null)
        {
            return ApiResponse<bool>.Fail("房东信息不存在");
        }
        return await _roomService.DeleteAsync(hostId.Value, id);
    }

    [HttpPost("batch-delete")]
    public async Task<ApiResponse<bool>> BatchDelete([FromBody] BatchDeleteRequest request)
    {
        var hostId = await GetCurrentHostId();
        if (hostId == null)
        {
            return ApiResponse<bool>.Fail("房东信息不存在");
        }
        return await _roomService.BatchDeleteAsync(hostId.Value, request.Ids);
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

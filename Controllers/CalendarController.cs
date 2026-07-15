using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using qisu_server.Data;
using qisu_server.Models.DTOs;

namespace qisu_server.Controllers;

[ApiController]
[Route("api/host/calendar")]
[Authorize]
public class CalendarController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<CalendarController> _logger;

    public CalendarController(AppDbContext context, ILogger<CalendarController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ApiResponse<CalendarDataDto>> GetCalendarData([FromQuery] CalendarQueryRequest request)
    {
        var hostId = await GetCurrentHostId();
        if (hostId == null)
        {
            return ApiResponse<CalendarDataDto>.Fail("房东信息不存在");
        }

        var startDate = string.IsNullOrEmpty(request.StartDate)
            ? DateTime.Today.AddDays(-7)
            : DateTime.Parse(request.StartDate);
        var endDate = string.IsNullOrEmpty(request.EndDate)
            ? DateTime.Today.AddDays(14)
            : DateTime.Parse(request.EndDate);

        if (endDate < startDate)
        {
            return ApiResponse<CalendarDataDto>.Fail("结束日期不能早于开始日期");
        }

        var propertyIds = await _context.Properties
            .Where(p => p.HostId == hostId.Value)
            .Select(p => p.Id)
            .ToListAsync();

        var roomQuery = _context.Rooms.Where(r => propertyIds.Contains(r.PropertyId));
        if (request.PropertyId.HasValue)
        {
            roomQuery = roomQuery.Where(r => r.PropertyId == request.PropertyId.Value);
        }

        var rooms = await roomQuery
            .OrderBy(r => r.Floor)
            .ThenBy(r => r.Name)
            .Select(r => new CalendarRoomDto
            {
                Id = r.Id,
                Name = r.Name,
                RoomType = r.RoomType,
                Floor = r.Floor,
                PricePerNight = r.PricePerNight,
                Status = r.Status,
                PropertyId = r.PropertyId
            })
            .ToListAsync();

        var roomIds = rooms.Select(r => r.Id).ToList();

        // 查询日期范围内与房间关联的订单
        var orders = await _context.Orders
            .Where(o =>
                (roomIds.Contains(o.RoomId.Value) || propertyIds.Contains(o.PropertyId))
                && o.CheckInDate <= endDate
                && o.CheckOutDate > startDate
                && o.Status != "cancelled"
                && o.Status != "refunded"
            )
            .Select(o => new
            {
                o.Id,
                o.OrderNo,
                o.PropertyId,
                o.RoomId,
                o.CheckInDate,
                o.CheckOutDate,
                o.Status,
                o.PricePerNight,
                o.GuestName,
                o.GuestPhone
            })
            .ToListAsync();

        var statuses = new List<CalendarDayStatusDto>();

        foreach (var room in rooms)
        {
            var roomPrice = room.PricePerNight;

            // 查找该房间关联的订单（优先匹配 RoomId，其次匹配 PropertyId）
            var roomOrders = orders.Where(o =>
                o.RoomId == room.Id ||
                (o.RoomId == null && o.PropertyId == room.PropertyId)
            ).ToList();

            for (var date = startDate; date <= endDate; date = date.AddDays(1))
            {
                var dateOnly = date.Date;
                var matchingOrder = roomOrders.FirstOrDefault(o =>
                    dateOnly >= o.CheckInDate && dateOnly < o.CheckOutDate
                );

                if (matchingOrder != null)
                {
                    string status;
                    if (matchingOrder.Status == "staying")
                        status = "occupied";
                    else if (matchingOrder.Status == "paid" || matchingOrder.Status == "confirmed")
                        status = "confirmed";
                    else if (matchingOrder.Status == "pending")
                        status = "confirmed";
                    else
                        status = "occupied";

                    statuses.Add(new CalendarDayStatusDto
                    {
                        RoomId = room.Id,
                        Date = dateOnly.ToString("yyyy-MM-dd"),
                        Status = status,
                        Price = matchingOrder.PricePerNight,
                        GuestName = matchingOrder.GuestName,
                        GuestPhone = matchingOrder.GuestPhone,
                        OrderNo = matchingOrder.OrderNo,
                        OrderId = matchingOrder.Id
                    });
                }
                else
                {
                    // 检查房间是否处于维护状态
                    var roomStatus = room.Status == 3 ? "maintenance" : "available";
                    statuses.Add(new CalendarDayStatusDto
                    {
                        RoomId = room.Id,
                        Date = dateOnly.ToString("yyyy-MM-dd"),
                        Status = roomStatus,
                        Price = roomPrice
                    });
                }
            }
        }

        return ApiResponse<CalendarDataDto>.Ok(new CalendarDataDto
        {
            Rooms = rooms,
            Statuses = statuses
        });
    }

    private async Task<long?> GetCurrentHostId()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim))
            userIdClaim = User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value;
        if (string.IsNullOrEmpty(userIdClaim))
            userIdClaim = User.FindFirst("sub")?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !long.TryParse(userIdClaim, out var userId))
            return null;

        var host = await _context.Hosts.FirstOrDefaultAsync(h => h.UserId == userId);
        return host?.Id;
    }
}

namespace qisu_server.Models.DTOs;

public class CalendarQueryRequest
{
    public long? PropertyId { get; set; }
    public string? StartDate { get; set; }
    public string? EndDate { get; set; }
}

public class CalendarRoomDto
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? RoomType { get; set; }
    public int Floor { get; set; }
    public decimal PricePerNight { get; set; }
    public byte Status { get; set; }
    public long PropertyId { get; set; }
}

public class CalendarDayStatusDto
{
    public long RoomId { get; set; }
    public string Date { get; set; } = string.Empty;
    public string Status { get; set; } = "available";
    public decimal Price { get; set; }
    public string? GuestName { get; set; }
    public string? GuestPhone { get; set; }
    public string? OrderNo { get; set; }
    public long? OrderId { get; set; }
}

public class CalendarDataDto
{
    public List<CalendarRoomDto> Rooms { get; set; } = new();
    public List<CalendarDayStatusDto> Statuses { get; set; } = new();
}

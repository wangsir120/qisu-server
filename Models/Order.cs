namespace qisu_server.Models;

public class Order
{
    public long Id { get; set; }
    public string OrderNo { get; set; } = string.Empty;
    public long UserId { get; set; }
    public long PropertyId { get; set; }
    public long? RoomId { get; set; }
    public long HostId { get; set; }
    public DateTime CheckInDate { get; set; }
    public DateTime CheckOutDate { get; set; }
    public int Nights { get; set; }
    public int GuestCount { get; set; }
    public string? GuestName { get; set; }
    public string? GuestPhone { get; set; }
    public string? GuestIdCard { get; set; }
    public decimal PricePerNight { get; set; }
    public decimal Subtotal { get; set; }
    public decimal CleaningFee { get; set; }
    public decimal ServiceFee { get; set; }
    public decimal TotalPrice { get; set; }
    public string Status { get; set; } = "pending";
    public string? PaymentMethod { get; set; }
    public DateTime? PaymentTime { get; set; }
    public DateTime? PayDeadline { get; set; }
    public string? CancelReason { get; set; }
    public DateTime? CancelTime { get; set; }
    public decimal? RefundAmount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public User? User { get; set; }
    public Property? Property { get; set; }
    public Host? Host { get; set; }
    public Room? Room { get; set; }
}

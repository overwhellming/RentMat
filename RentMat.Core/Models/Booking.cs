using RentMat.Core.Enums;

namespace RentMat.Core.Models;

public class Booking
{
    public int Id { get; init; }
    public DateTimeOffset StartDate { get; set; }
    public DateTimeOffset EndDate { get; set; }
    public decimal TotalPrice { get; set; }

    public BookingStatus Status { get; set; }

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public int DeviceId { get; set; }
    public Device Device { get; set; } = null!;
}
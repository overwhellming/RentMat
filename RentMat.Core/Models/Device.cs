using RentMat.Core.Enums;

namespace RentMat.Core.Models;

public class Device
{
    public int Id { get; init; }
    public required string Name { get; set; }
    public decimal HourRentPrice { get; set; }
    
    public int CategoryId { get; set; }
    public DeviceCategory Category { get; set; } = null!; 
    
    public DeviceStatus Status { get; set; }
}
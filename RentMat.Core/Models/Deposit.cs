namespace RentMat.Core.Models;

public class Deposit
{
    public int Id { get; init; }
    public decimal Amount { get; set; }
    public DateTimeOffset CreatedAt { get; init; }
    
    public int UserId { get; set; }
    public User User { get; set; } = null!;
}
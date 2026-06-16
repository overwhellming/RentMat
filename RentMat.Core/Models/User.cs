namespace RentMat.Core.Models;

public class User
{
    public int Id { get; init; }
    public required string Login { get; set; } 
    public required string Email { get; set; }
    public required string HashedPassword { get; set; }
    public decimal Balance { get; set; } 
    public DateTime CreatedAt { get; init; }
}
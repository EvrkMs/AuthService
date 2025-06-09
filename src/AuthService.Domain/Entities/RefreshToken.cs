
namespace AuthService.Domain.Entities;

public class RefreshToken
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string TokenHash { get; set; }
    public required string Salt { get; set; }
    public required string DeviceId { get; set; }
    public string? DeviceInfo { get; set; }
    public DateTime ExpiresAt { get; set; }
    public bool Revoked { get; set; }
    public Guid UserId { get; set; }
    public User? User { get; set; }
}

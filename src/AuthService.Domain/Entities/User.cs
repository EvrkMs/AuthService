
namespace AuthService.Domain.Entities;

public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Phone { get; set; }
    public required string PasswordHash { get; set; }
    public List<string> Roles { get; set; } = new();
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}

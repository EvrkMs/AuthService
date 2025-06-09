using System.Text.Json.Serialization;

namespace AuthService.Application.DTOs;

public record LoginRequest
{
    public required string Phone { get; init; }
    public required string Password { get; init; }
    [JsonIgnore]
    public string? DeviceInfo { get; set; }
}

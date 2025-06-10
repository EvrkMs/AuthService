namespace AuthService.Application.DTOs;

public record SessionDto(Guid Id,string DeviceId,string? DeviceInfo,DateTime ExpiresAt);

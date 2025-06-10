namespace AuthService.Application.DTOs;

public record ChangePasswordRequest(string OldPassword,string NewPassword);

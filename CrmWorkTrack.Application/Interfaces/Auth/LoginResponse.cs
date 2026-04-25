namespace CrmWorkTrack.Application.Interfaces.Auth.DTOs;

public record LoginResponse(
    string AccessToken,
    DateTime ExpiresAt,
    string RefreshToken,
    string UserId,
    string UserName,
    List<string> Roles,
    List<string> Perms
);
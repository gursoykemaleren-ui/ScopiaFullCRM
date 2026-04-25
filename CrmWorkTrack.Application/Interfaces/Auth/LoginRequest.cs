namespace CrmWorkTrack.Application.Interfaces.Auth.DTOs;

public record LoginRequest(
    string UserName,
    string Password
);

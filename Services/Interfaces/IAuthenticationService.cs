using PhotoScavengerHunt.Features.Users;

namespace PhotoScavengerHunt.Services.Interfaces;

public interface IAuthenticationService
{
    Task<(bool Success, string Message, object? Data)> RegisterAsync(RegisterRequest request);
    Task<(bool Success, string Message, object? Data)> LoginAsync(LoginRequest request);
}


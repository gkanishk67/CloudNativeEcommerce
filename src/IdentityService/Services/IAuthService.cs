using IdentityService.DTOs;

namespace IdentityService.Services
{
    public interface IAuthService
    {
        Task RegisterAsync(RegisterRequest request);

        Task<string?> LoginAsync(LoginRequest request);
    }
}
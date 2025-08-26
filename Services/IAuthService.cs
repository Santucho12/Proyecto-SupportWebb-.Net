using SupportWeb.Models.DTOs;

namespace SupportWeb.Services
{
    public interface IAuthService
    {
        Task<bool> LoginAsync(string email, string password);
        Task LogoutAsync();
        void Logout(); // Método síncrono
        Task<bool> RegisterAsync(RegisterRequestDto request);
        Task<UsuarioDto?> GetCurrentUserAsync();
        Task<string?> GetCurrentUserRoleAsync();
        Task<Guid?> GetCurrentUserIdAsync();
        bool IsAuthenticated();
        string? GetStoredToken();
        string? GetToken();
    }
}

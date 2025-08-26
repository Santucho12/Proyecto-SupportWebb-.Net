using SupportWeb.Models.DTOs;

namespace SupportWeb.Services
{
    public interface IApiService
    {
        // Auth
        Task<LoginResponseDto?> LoginAsync(LoginRequestDto request);
        Task<bool> RegisterAsync(RegisterRequestDto request);

        // Reclamos
        Task<List<ReclamoDto>> GetReclamosAsync();
        Task<ReclamoDto?> GetReclamoByIdAsync(Guid id);
        Task<ReclamoDto?> CreateReclamoAsync(CreateReclamoDto request);
        Task<bool> UpdateReclamoAsync(Guid id, ReclamoDto reclamo);
        Task<bool> UpdateReclamoEstadoAsync(Guid id, string estado);
        Task<bool> DeleteReclamoAsync(Guid id);

        // Respuestas
        Task<List<RespuestaDto>> GetRespuestasByReclamoAsync(Guid reclamoId);
        Task<RespuestaDto?> CreateRespuestaAsync(CreateRespuestaDto request);
        Task<List<NotificacionDto>> GetNotificacionesNoVistasAsync(Guid usuarioId);
        Task<bool> MarcarRespuestaComoVistaAsync(Guid respuestaId);

        // Usuarios
        Task<List<UsuarioDto>> GetUsuariosAsync();
        Task<UsuarioDto?> GetUsuarioByIdAsync(Guid id);
        Task<UsuarioDto?> UpdateUsuarioAsync(Guid id, UpdateUsuarioDto request);
        Task<bool> UpdateUsuarioAsync(UpdateUsuarioDto request);
        Task<bool> DeleteUsuarioAsync(Guid id);

        // Dashboard
        Task<DashboardStatsDto> GetDashboardStatsAsync();

        // Notificaciones
        Task<List<NotificacionDto>> GetNotificacionesAsync(Guid usuarioId);
        Task<bool> MarcarNotificacionLeidaAsync(Guid notificacionId);
    Task<bool> EliminarNotificacionAsync(Guid notificacionId);
        // Notificaciones para soporte
        Task<List<NotificacionDto>> GetNotificacionesSoporteAsync();

    // Configuración
    void SetAuthToken(string token);
    void ClearAuthToken();

    // Cambio de contraseña
    Task<bool> ChangePasswordAsync(Guid usuarioId, string currentPassword, string newPassword);
        // 2FA
        Task<bool> EnviarCodigo2FAEmailAsync(string email, string code);
        Task<bool> Activar2FAUsuarioAsync(Guid usuarioId);
    }
}

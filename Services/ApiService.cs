using SupportWeb.Models.DTOs;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace SupportWeb.Services
{
    public class ApiService : IApiService
    {
        // ...existing code...

        public async Task<List<NotificacionDto>> GetNotificacionesSoporteAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/notificaciones/soporte");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<List<NotificacionDto>>(content, _jsonOptions) ?? new List<NotificacionDto>();
                }
                _logger.LogWarning("Error getting notificaciones soporte. Status: {StatusCode}", response.StatusCode);
                return new List<NotificacionDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting notificaciones soporte");
                return new List<NotificacionDto>();
            }
        }

        public async Task<bool> ChangePasswordAsync(Guid usuarioId, string currentPassword, string newPassword)
        {
            try
            {
                var json = JsonSerializer.Serialize(new {
                    usuarioId = usuarioId,
                    currentPassword = currentPassword,
                    newPassword = newPassword
                }, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("/api/usuarios/cambiar-contrasena", content);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cambiando la contraseña del usuario {Id}", usuarioId);
                return false;
            }
        }
        public async Task<bool> DeleteUsuarioAsync(Guid id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"/api/usuarios/{id}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting usuario {Id}", id);
                return false;
            }
        }
        private readonly HttpClient _httpClient;
        private readonly ILogger<ApiService> _logger;
        private readonly JsonSerializerOptions _jsonOptions;

        public ApiService(HttpClient httpClient, ILogger<ApiService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true
            };
        }

        public void SetAuthToken(string token)
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        }

        public void ClearAuthToken()
        {
            _httpClient.DefaultRequestHeaders.Authorization = null;
        }

        // Auth Methods
        public async Task<LoginResponseDto?> LoginAsync(LoginRequestDto request)
        {
            try
            {
                var json = JsonSerializer.Serialize(request, _jsonOptions);
                _logger.LogError("=== JSON ENVIADO A LA API ===");
                _logger.LogError("JSON: {Json}", json);
                _logger.LogError("URL: /api/auth/login");
                _logger.LogError("=============================");

                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("/api/auth/login", content);

                _logger.LogError("=== RESPUESTA DE LA API ===");
                _logger.LogError("Status Code: {StatusCode}", response.StatusCode);
                var responseBody = await response.Content.ReadAsStringAsync();
                _logger.LogError("Response Body: {ResponseBody}", responseBody);
                _logger.LogError("===========================");

                if (response.IsSuccessStatusCode)
                {
                    return JsonSerializer.Deserialize<LoginResponseDto>(responseBody, _jsonOptions);
                }

                _logger.LogWarning("Login failed: {StatusCode}", response.StatusCode);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login");
                return null;
            }
        }

        public async Task<bool> RegisterAsync(RegisterRequestDto request)
        {
            try
            {
                var json = JsonSerializer.Serialize(request, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("/api/auth/register", content);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration");
                return false;
            }
        }

        // 2FA: Enviar código por email
        public async Task<bool> EnviarCodigo2FAEmailAsync(string email, string code)
        {
            try
            {
                var json = JsonSerializer.Serialize(new { email = email, code = code }, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                // Simulación: endpoint ficticio, reemplaza por tu lógica real
                var response = await _httpClient.PostAsync("/api/usuarios/enviar-codigo-2fa", content);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error enviando código 2FA por email a {Email}", email);
                return false;
            }
        }

        // 2FA: Activar 2FA para usuario
        public async Task<bool> Activar2FAUsuarioAsync(Guid usuarioId)
        {
            try
            {
                var json = JsonSerializer.Serialize(new { usuarioId = usuarioId }, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                // Simulación: endpoint ficticio, reemplaza por tu lógica real
                var response = await _httpClient.PostAsync("/api/usuarios/activar-2fa", content);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error activando 2FA para usuario {Id}", usuarioId);
                return false;
            }
        }

        // Reclamos Methods
        public async Task<List<ReclamoDto>> GetReclamosAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("/api/reclamos");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<List<ReclamoDto>>(content, _jsonOptions) ?? new List<ReclamoDto>();
                }

                return new List<ReclamoDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting reclamos");
                return new List<ReclamoDto>();
            }
        }

        public async Task<ReclamoDto?> GetReclamoByIdAsync(Guid id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/reclamos/{id}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<ReclamoDto>(content, _jsonOptions);
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting reclamo {Id}", id);
                return null;
            }
        }

        public async Task<ReclamoDto?> CreateReclamoAsync(CreateReclamoDto request)
        {
            try
            {
                // Solo enviar los campos relevantes como JSON, ignorando el archivo
                var json = JsonSerializer.Serialize(new {
                    titulo = request.Titulo,
                    descripcion = request.Descripcion,
                    prioridad = request.Prioridad,
                    usuarioId = request.UsuarioId
                }, _jsonOptions);

                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("/api/reclamos", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<ReclamoDto>(responseContent, _jsonOptions);
                }

                _logger.LogWarning("Error creating reclamo. Status: {StatusCode}, Content: {Content}",
                    response.StatusCode, await response.Content.ReadAsStringAsync());
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating reclamo");
                return null;
            }
        }

        public async Task<bool> UpdateReclamoAsync(Guid id, ReclamoDto reclamo)
        {
            try
            {
                var json = JsonSerializer.Serialize(reclamo, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync($"/api/reclamos/{id}", content);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating reclamo {Id}", id);
                return false;
            }
        }

        public async Task<bool> UpdateReclamoEstadoAsync(Guid id, string estado)
        {
            try
            {
                var estadoDto = new { Estado = estado };
                var json = JsonSerializer.Serialize(estadoDto, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PatchAsync($"/api/reclamos/{id}/estado", content);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating reclamo estado {Id}", id);
                return false;
            }
        }

        public async Task<bool> DeleteReclamoAsync(Guid id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"/api/reclamos/{id}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting reclamo {Id}", id);
                return false;
            }
        }

        // Respuestas Methods
        public async Task<List<RespuestaDto>> GetRespuestasByReclamoAsync(Guid reclamoId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/respuestas?reclamoId={reclamoId}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<List<RespuestaDto>>(content, _jsonOptions) ?? new List<RespuestaDto>();
                }

                return new List<RespuestaDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting respuestas for reclamo {ReclamoId}", reclamoId);
                return new List<RespuestaDto>();
            }
        }

        public async Task<RespuestaDto?> CreateRespuestaAsync(CreateRespuestaDto request)
        {
            try
            {
                var json = JsonSerializer.Serialize(request, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("/api/respuestas", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<RespuestaDto>(responseContent, _jsonOptions);
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating respuesta");
                return null;
            }
        }

        public async Task<List<NotificacionDto>> GetNotificacionesNoVistasAsync(Guid usuarioId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/respuestas/notificaciones?usuarioId={usuarioId}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<List<NotificacionDto>>(content, _jsonOptions) ?? new List<NotificacionDto>();
                }

                return new List<NotificacionDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting notificaciones for user {UserId}", usuarioId);
                return new List<NotificacionDto>();
            }
        }

        public async Task<bool> MarcarRespuestaComoVistaAsync(Guid respuestaId)
        {
            try
            {
                var response = await _httpClient.PatchAsync($"/api/respuestas/{respuestaId}/visto", null);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking respuesta as seen {RespuestaId}", respuestaId);
                return false;
            }
        }

        // Usuarios Methods
        public async Task<List<UsuarioDto>> GetUsuariosAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("/api/usuarios");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<List<UsuarioDto>>(content, _jsonOptions) ?? new List<UsuarioDto>();
                }

                return new List<UsuarioDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting usuarios");
                return new List<UsuarioDto>();
            }
        }

        public async Task<UsuarioDto?> GetUsuarioByIdAsync(Guid id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/usuarios/{id}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<UsuarioDto>(content, _jsonOptions);
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting usuario {Id}", id);
                return null;
            }
        }

        public async Task<UsuarioDto?> UpdateUsuarioAsync(Guid id, UpdateUsuarioDto request)
        {
            try
            {
                var json = JsonSerializer.Serialize(request, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync($"/api/usuarios/{id}", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<UsuarioDto>(responseContent, _jsonOptions);
                }

                _logger.LogWarning("Error updating usuario {Id}: {StatusCode} - {ReasonPhrase}",
                    id, response.StatusCode, response.ReasonPhrase);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating usuario {Id}", id);
                return null;
            }
        }

        // Dashboard Methods
        public async Task<DashboardStatsDto> GetDashboardStatsAsync()
        {
            try
            {
                var reclamos = await GetReclamosAsync();

                var stats = new DashboardStatsDto
                {
                    TotalReclamos = reclamos.Count,
                    ReclamosNuevos = reclamos.Count(r => r.Estado == "Nuevo"),
                    ReclamosEnProceso = reclamos.Count(r => r.Estado == "EnProceso"),
                    ReclamosRespondidos = reclamos.Count(r => r.Estado == "Respondido"),
                    ReclamosCerrados = reclamos.Count(r => r.Estado == "Cerrado"),
                    ReclamosRecientes = reclamos.OrderByDescending(r => r.FechaCreacion).Take(5).ToList()
                };

                // Calcular tiempo promedio de resolución (simplificado)
                var reclamosCerrados = reclamos.Where(r => r.Estado == "Cerrado").ToList();
                if (reclamosCerrados.Any())
                {
                    var zonaArgentina = TimeZoneInfo.FindSystemTimeZoneById("Argentina Standard Time");
                    var nowLocal = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, zonaArgentina);
                    stats.TiempoPromedioResolucion = reclamosCerrados.Average(r => (nowLocal - r.FechaCreacion).TotalDays);
                }

                return stats;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting dashboard stats");
                return new DashboardStatsDto();
            }
        }

        public async Task<List<NotificacionDto>> GetNotificacionesAsync(Guid usuarioId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/notificaciones/usuario/{usuarioId}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<List<NotificacionDto>>(content, _jsonOptions) ?? new List<NotificacionDto>();
                }

                _logger.LogWarning("Error getting notificaciones. Status: {StatusCode}", response.StatusCode);
                return new List<NotificacionDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting notificaciones");
                return new List<NotificacionDto>();
            }
        }

        public async Task<bool> MarcarNotificacionLeidaAsync(Guid notificacionId)
        {
            try
            {
                var response = await _httpClient.PatchAsync($"/api/respuestas/{notificacionId}/visto", null);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking notification as read");
                return false;
            }
        }

        // Método para actualizar perfil del usuario actual
        public async Task<bool> UpdateUsuarioAsync(UpdateUsuarioDto request)
        {
            try
            {
                var json = JsonSerializer.Serialize(request, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync($"/api/usuarios/{request.Id}", content);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating usuario");
                return false;
            }
        }

        public async Task<bool> EliminarNotificacionAsync(Guid notificacionId)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"/api/respuestas/{notificacionId}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting notification {Id}", notificacionId);
                return false;
            }
        }
    }
}

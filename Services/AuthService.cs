using SupportWeb.Models.DTOs;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;

namespace SupportWeb.Services
{
    public class AuthService : IAuthService
    {
        private readonly IApiService _apiService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<AuthService> _logger;
        private UsuarioDto? _currentUser;

        public AuthService(IApiService apiService, IHttpContextAccessor httpContextAccessor, ILogger<AuthService> logger)
        {
            _apiService = apiService;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public async Task<bool> LoginAsync(string email, string password)
        {
            try
            {
                var loginRequest = new LoginRequestDto
                {
                    CorreoElectronico = email,
                    Contrasena = password
                };

                var loginResponse = await _apiService.LoginAsync(loginRequest);

                if (loginResponse != null && !string.IsNullOrEmpty(loginResponse.Token))
                {
                    // Almacenar token en sesión
                    _httpContextAccessor.HttpContext?.Session.SetString("AuthToken", loginResponse.Token);

                    // Almacenar usuario en sesión (serialización robusta)
                    if (loginResponse.Usuario != null)
                    {
                        var userJson = JsonSerializer.Serialize(loginResponse.Usuario, typeof(UsuarioDto), new JsonSerializerOptions { PropertyNamingPolicy = null });
                        _httpContextAccessor.HttpContext?.Session.SetString("CurrentUser", userJson);
                    }

                    // Configurar token en ApiService
                    _apiService.SetAuthToken(loginResponse.Token);

                    _currentUser = loginResponse.Usuario;

                    _logger.LogInformation("User {Email} logged in successfully", email);
                    return true;
                }

                _logger.LogWarning("Login failed for user {Email}", email);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for user {Email}", email);
                return false;
            }
        }

        public async Task LogoutAsync()
        {
            try
            {
                // Limpiar sesión
                _httpContextAccessor.HttpContext?.Session.Remove("AuthToken");
                _httpContextAccessor.HttpContext?.Session.Remove("CurrentUser");

                // Limpiar token en ApiService
                _apiService.ClearAuthToken();

                _currentUser = null;

                _logger.LogInformation("User logged out successfully");
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout");
            }
        }

        // Método síncrono para logout rápido
        public void Logout()
        {
            try
            {
                _logger.LogInformation("=== LOGOUT SÍNCRONO EJECUTADO ===");

                // Limpiar toda la sesión
                _httpContextAccessor.HttpContext?.Session.Clear();

                // Limpiar token en ApiService
                _apiService.ClearAuthToken();

                _currentUser = null;

                _logger.LogInformation("Sesión completamente limpiada");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout");
            }
        }

        public async Task<bool> RegisterAsync(RegisterRequestDto request)
        {
            try
            {
                var result = await _apiService.RegisterAsync(request);

                if (result)
                {
                    _logger.LogInformation("User {Email} registered successfully", request.CorreoElectronico);
                }
                else
                {
                    _logger.LogWarning("Registration failed for user {Email}", request.CorreoElectronico);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration for user {Email}", request.CorreoElectronico);
                return false;
            }
        }

        public async Task<UsuarioDto?> GetCurrentUserAsync()
        {
            try
            {
                _logger.LogInformation("=== OBTENIENDO USUARIO ACTUAL ===");

                if (_currentUser != null)
                {
                    _logger.LogInformation("Usuario encontrado en cache: {Email}", _currentUser.Email);
                    return _currentUser;
                }

                var userJson = _httpContextAccessor.HttpContext?.Session.GetString("CurrentUser");
                _logger.LogInformation("Usuario en sesión: {UserJson}", userJson != null ? "Encontrado" : "No encontrado");

                if (!string.IsNullOrEmpty(userJson))
                {
                    try
                    {
                        _currentUser = JsonSerializer.Deserialize<UsuarioDto>(userJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        _logger.LogInformation("Usuario deserializado desde sesión: {Email}", _currentUser?.Email);
                        return _currentUser;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error deserializando usuario desde sesión. JSON: {UserJson}", userJson);
                    }
                }

                // Si no hay usuario en sesión pero hay token, intentar decodificar del token
                var token = GetStoredToken();
                _logger.LogInformation("Token encontrado para decodificar: {TokenExists}", !string.IsNullOrEmpty(token));

                if (!string.IsNullOrEmpty(token))
                {
                    var handler = new JwtSecurityTokenHandler();
                    var jsonToken = handler.ReadJwtToken(token);

                    var userId = jsonToken.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;
                    var userName = jsonToken.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Name)?.Value;
                    var userEmail = jsonToken.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Email)?.Value;
                    var userRole = jsonToken.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Role)?.Value;

                    _logger.LogInformation("Claims extraídos - ID: {UserId}, Name: {UserName}, Email: {UserEmail}, Role: {UserRole}",
                        userId, userName, userEmail, userRole);

                    if (!string.IsNullOrEmpty(userId) && Guid.TryParse(userId, out var userGuid))
                    {
                        _currentUser = new UsuarioDto
                        {
                            Id = userGuid,
                            Nombre = userName ?? "",
                            Email = userEmail ?? "",
                            Rol = userRole ?? "Usuario"
                        };

                        // Guardar en sesión para próximas consultas
                        var newUserJson = JsonSerializer.Serialize(_currentUser);
                        _httpContextAccessor.HttpContext?.Session.SetString("CurrentUser", newUserJson);

                        _logger.LogInformation("Usuario creado desde token: {Email}, ID: {Id}", _currentUser.Email, _currentUser.Id);
                        return _currentUser;
                    }
                    else
                    {
                        _logger.LogWarning("No se pudo parsear el ID del usuario desde el token: {UserId}", userId);
                    }
                }

                _logger.LogWarning("No se pudo obtener usuario - ni en cache, ni en sesión, ni en token");
                await Task.CompletedTask;
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current user");
                return null;
            }
        }

        public async Task<string?> GetCurrentUserRoleAsync()
        {
            try
            {
                // Intentar primero desde el usuario cacheado
                var user = await GetCurrentUserAsync();
                if (user != null && !string.IsNullOrEmpty(user.Rol))
                {
                    return user.Rol;
                }

                // Si no hay usuario cacheado, leer directamente del token
                var token = GetStoredToken();
                if (!string.IsNullOrEmpty(token))
                {
                    var handler = new JwtSecurityTokenHandler();
                    var jsonToken = handler.ReadJwtToken(token);

                    // Buscar el claim de rol usando diferentes formatos posibles
                    var roleClaim = jsonToken.Claims.FirstOrDefault(x =>
                        x.Type == ClaimTypes.Role ||
                        x.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role" ||
                        x.Type == "role");

                    var role = roleClaim?.Value;
                    _logger.LogInformation("Rol extraído del token: {Role}", role ?? "No encontrado");
                    return role;
                }

                _logger.LogWarning("No se pudo obtener el rol del usuario");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo rol del usuario");
                return null;
            }
        }

        public async Task<Guid?> GetCurrentUserIdAsync()
        {
            var user = await GetCurrentUserAsync();
            return user?.Id;
        }

        public bool IsAuthenticated()
        {
            // MODO DEBUG: Siempre requerir login fresco
            _logger.LogInformation("=== VERIFICACIÓN DE AUTENTICACIÓN ===");

            var token = GetStoredToken();
            _logger.LogInformation("Token encontrado: {HasToken}", !string.IsNullOrEmpty(token));

            if (string.IsNullOrEmpty(token))
            {
                _logger.LogInformation("No hay token - NO AUTENTICADO");
                return false;
            }

            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jsonToken = handler.ReadJwtToken(token);

                // Verificar si el token no ha expirado
                var isValid = jsonToken.ValidTo > DateTime.UtcNow;
                _logger.LogInformation("Token válido: {IsValid}, Expira: {Expiry}", isValid, jsonToken.ValidTo);

                return isValid;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validando token - NO AUTENTICADO");
                return false;
            }
        }

        public string? GetStoredToken()
        {
            return _httpContextAccessor.HttpContext?.Session.GetString("AuthToken");
        }

        public string? GetToken()
        {
            return GetStoredToken();
        }
    }
}

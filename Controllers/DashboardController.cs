using Microsoft.AspNetCore.Mvc;
using SupportWeb.Models.ViewModels;
using SupportWeb.Models.ViewModels.Usuario;
using SupportWeb.Models.DTOs;
using SupportWeb.Services;

namespace SupportWeb.Controllers
{
    public class DashboardController : Controller
    {
        private readonly IApiService _apiService;
        private readonly IAuthService _authService;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(IApiService apiService, IAuthService authService, ILogger<DashboardController> logger)
        {
            _apiService = apiService;
            _authService = authService;
            _logger = logger;
        }

        // Dashboard genérico que redirige según el rol
        public async Task<IActionResult> Index()
        {
            _logger.LogInformation("=== DASHBOARD CARGADO ===");

            try
            {
                // Verificar que el usuario esté autenticado
                var isAuthenticated = _authService.IsAuthenticated();
                if (!isAuthenticated)
                {
                    _logger.LogWarning("Usuario no autenticado intentando acceder al dashboard");
                    return RedirectToAction("Login", "Auth");
                }

                // Obtener el rol del usuario
                var userRole = await _authService.GetCurrentUserRoleAsync();
                _logger.LogInformation("Usuario con rol {Rol} accediendo al dashboard", userRole ?? "No disponible");

                // Redirigir según el rol
                switch (userRole)
                {
                    case "Admin":
                        return RedirectToAction("Dashboard", "Admin");
                    case "Soporte":
                        return RedirectToAction("Dashboard", "Soporte");
                    case "Usuario":
                        return RedirectToAction("Dashboard", "Usuario");
                    default:
                        _logger.LogWarning("Rol de usuario no reconocido: {Rol}", userRole);
                        return RedirectToAction("Login", "Auth");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en el dashboard principal");
                TempData["ErrorMessage"] = "Error al cargar el dashboard.";
                return RedirectToAction("Login", "Auth");
            }
        }

        // Métodos API para estadísticas compartidas
        [HttpGet]
        public async Task<IActionResult> GetNotificationCount()
        {
            try
            {
                var isAuthenticated = _authService.IsAuthenticated();
                    // ...existing code...
                    await Task.CompletedTask;
                {
                    return Json(new { count = 0 });
                }

                // Simular notificaciones hasta que tengamos el endpoint real
                var count = Random.Shared.Next(0, 10);
                return Json(new { count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo contador de notificaciones");
                return Json(new { count = 0 });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetStats()
        {
            try
            {
                var isAuthenticated = _authService.IsAuthenticated();
                    // ...existing code...
                    await Task.CompletedTask;
                {
                    return Json(new { error = "No autenticado" });
                }

                var token = _authService.GetStoredToken();
                if (!string.IsNullOrEmpty(token))
                {
                    _apiService.SetAuthToken(token);
                }

                var reclamos = await _apiService.GetReclamosAsync();

                var stats = new
                {
                    total = reclamos?.Count ?? 0,
                    nuevos = reclamos?.Count(r => r.Estado == "Nuevo") ?? 0,
                    proceso = reclamos?.Count(r => r.Estado == "En Proceso") ?? 0,
                    resueltos = reclamos?.Count(r => r.Estado == "Cerrado") ?? 0,
                    prioridad = new
                    {
                        alta = reclamos?.Count(r => r.Prioridad == "Alta") ?? 0,
                        media = reclamos?.Count(r => r.Prioridad == "Media") ?? 0,
                        baja = reclamos?.Count(r => r.Prioridad == "Baja") ?? 0
                    }
                };

                return Json(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo estadísticas");
                return Json(new { error = "Error interno" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetNotifications()
        {
            try
            {
                var isAuthenticated = _authService.IsAuthenticated();
                    // ...existing code...
                    await Task.CompletedTask;
                {
                    return Json(new List<object>());
                }

                // Simular notificaciones hasta tener endpoint real
                var zonaArgentina = TimeZoneInfo.FindSystemTimeZoneById("Argentina Standard Time");
                var nowLocal = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, zonaArgentina);
                var notifications = new[]
                {
                    new { id = Guid.NewGuid(), mensaje = "Nuevo reclamo recibido", fecha = nowLocal.AddMinutes(-5), leida = false },
                    new { id = Guid.NewGuid(), mensaje = "Reclamo actualizado", fecha = nowLocal.AddHours(-1), leida = false },
                    new { id = Guid.NewGuid(), mensaje = "Respuesta pendiente", fecha = nowLocal.AddHours(-2), leida = true }
                };
                // Asegurarse que ninguna fecha sea mayor que nowLocal
                notifications = notifications.Select(n => new { n.id, n.mensaje, fecha = n.fecha > nowLocal ? nowLocal : n.fecha, n.leida }).ToArray();

                return Json(notifications);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo notificaciones");
                return Json(new List<object>());
            }
        }

        [HttpPost]
        public async Task<IActionResult> MarkNotificationAsRead(Guid notificationId)
        {
            try
            {
                var isAuthenticated = _authService.IsAuthenticated();
                    // ...existing code...
                    await Task.CompletedTask;
                {
                    return Json(new { success = false, message = "No autenticado" });
                }

                // Aquí iría la lógica real para marcar como leída
                _logger.LogInformation("Marcando notificación {Id} como leída", notificationId);

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marcando notificación como leída");
                return Json(new { success = false, message = "Error interno" });
            }
        }

        // Métodos genéricos que pueden ser usados por cualquier rol

        [HttpPost]
        public async Task<IActionResult> ActualizarPerfil(UpdateUsuarioDto model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View("MiPerfil", model);
                }

                var token = _authService.GetStoredToken();
                if (!string.IsNullOrEmpty(token))
                {
                    _apiService.SetAuthToken(token);
                }

                var result = await _apiService.UpdateUsuarioAsync(model.Id, model);
                if (result != null)
                {
                    TempData["SuccessMessage"] = "Perfil actualizado correctamente";
                }
                else
                {
                    TempData["ErrorMessage"] = "Error actualizando el perfil";
                }

                return RedirectToAction("MiPerfil");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error actualizando perfil");
                TempData["ErrorMessage"] = "Error actualizando perfil de usuario";
                return RedirectToAction("MiPerfil");
            }
        }
    }
}

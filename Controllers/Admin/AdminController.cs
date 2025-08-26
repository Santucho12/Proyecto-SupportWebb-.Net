using Microsoft.AspNetCore.Mvc;
using SupportWeb.Services;
using SupportWeb.Models.DTOs;
using SupportWeb.Models.ViewModels.Admin;
using SupportWeb.Models.ViewModels;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace SupportWeb.Controllers.Admin
{
    [Route("Admin")]
    public class AdminController : Controller
    {
                // ...existing code...

                [HttpPost("CambiarContrasena")]
                [ValidateAntiForgeryToken]
                public async Task<IActionResult> CambiarContrasena([FromBody] ChangePasswordRequest model)
                {
                    if (!_authService.IsAuthenticated())
                        return Json(new { success = false, message = "Usuario no autenticado" });

                    var userRole = await _authService.GetCurrentUserRoleAsync();
                    if (userRole != "Admin")
                        return Json(new { success = false, message = "Sin permisos" });

                    var usuario = await _authService.GetCurrentUserAsync();
                    if (usuario == null)
                        return Json(new { success = false, message = "Usuario no encontrado" });

                    // Aquí deberías validar la contraseña actual y cambiarla usando tu servicio
                    var result = await _apiService.ChangePasswordAsync(usuario.Id, model.currentPassword, model.newPassword);
                    if (result)
                        return Json(new { success = true });
                    else
                        return Json(new { success = false, message = "La contraseña actual es incorrecta o hubo un error." });
                }

                public class ChangePasswordRequest
                {
                    public string currentPassword { get; set; }
                    public string newPassword { get; set; }
                }

                [HttpPost("GuardarConfiguracion")]
                [ValidateAntiForgeryToken]
                public async Task<IActionResult> GuardarConfiguracion([FromBody] ConfiguracionRequest model)
                {
                    if (!_authService.IsAuthenticated())
                        return Json(new { success = false, message = "Usuario no autenticado" });

                    var userRole = await _authService.GetCurrentUserRoleAsync();
                    if (userRole != "Admin")
                        return Json(new { success = false, message = "Sin permisos" });

                    // Aquí podrías guardar la configuración en la base de datos o en el perfil del usuario
                    // Ejemplo: await _apiService.SaveAdminConfigAsync(usuarioId, model);
                    // Por ahora, simula éxito
                    await Task.Delay(200); // Simulación
                    return Json(new { success = true });
                }

                public class ConfiguracionRequest
                {
                    public bool emailNotifications { get; set; }
                    public bool smsNotifications { get; set; }
                    public bool newTicketNotifications { get; set; }
                    public bool responseNotifications { get; set; }
                    public bool darkMode { get; set; }
                    public string language { get; set; }
                    public bool twoFactorAuth { get; set; }
                }
                [HttpPost("EliminarUsuario")]
                public async Task<IActionResult> EliminarUsuario(Guid id)
                {
                    try
                    {
                        if (!_authService.IsAuthenticated())
                            return RedirectToAction("Login", "Auth");

                        var userRole = await _authService.GetCurrentUserRoleAsync();
                        if (userRole != "Admin")
                            return RedirectToAction("Login", "Auth");

                        var token = _authService.GetToken();
                        if (!string.IsNullOrEmpty(token))
                            _apiService.SetAuthToken(token);

                        var result = await _apiService.DeleteUsuarioAsync(id);
                        if (result)
                        {
                            TempData["SuccessMessage"] = "Usuario eliminado correctamente.";
                        }
                        else
                        {
                            TempData["ErrorMessage"] = "No se pudo eliminar el usuario.";
                        }
                        return RedirectToAction("GestionUsuarios");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error al eliminar el usuario");
                        TempData["ErrorMessage"] = "Error al eliminar el usuario.";
                        return RedirectToAction("GestionUsuarios");
                    }
                }
                    // Devuelve la clase CSS para el estado del reclamo
                    private string GetEstadoBadgeClass(string estado)
                    {
                        return estado switch
                        {
                            "Nuevo" => "bg-primary text-white",
                            "EnProceso" => "bg-warning text-dark",
                            "Respondido" => "bg-info text-white",
                            "Cerrado" => "bg-secondary text-white",
                            _ => "bg-light text-dark"
                        };
                    }

                    // Devuelve la clase CSS para la prioridad del reclamo
                    private string GetPrioridadBadgeClass(string prioridad)
                    {
                        return prioridad switch
                        {
                            "Baja" => "bg-info text-white",
                            "Media" => "bg-success text-white",
                            "Alta" => "bg-danger text-white",
                            "Urgente" => "bg-dark text-white",
                            _ => "bg-light text-dark"
                        };
                    }
                    private readonly IAuthService _authService;
                    private readonly IApiService _apiService;
                    private readonly ILogger<AdminController> _logger;

                public AdminController(IAuthService authService, IApiService apiService, ILogger<AdminController> logger)
                {
                    _authService = authService;
                    _apiService = apiService;
                    _logger = logger;
                }

                [HttpGet]
                public async Task<IActionResult> FiltrarReclamos(string busqueda, string estado, string prioridad)
                {
                    if (!_authService.IsAuthenticated())
                        return Content("");

                    var token = _authService.GetToken();
                    if (!string.IsNullOrEmpty(token))
                        _apiService.SetAuthToken(token);

                    var reclamos = await _apiService.GetReclamosAsync();

                    if (!string.IsNullOrWhiteSpace(busqueda))
                    {
                        reclamos = reclamos.Where(r =>
                            (r.Titulo?.ToLower().Contains(busqueda.ToLower()) ?? false) ||
                            (r.UsuarioNombre?.ToLower().Contains(busqueda.ToLower()) ?? false) ||
                            (r.Estado?.ToLower().Contains(busqueda.ToLower()) ?? false)
                        ).ToList();
                    }
                    if (!string.IsNullOrWhiteSpace(estado))
                        reclamos = reclamos.Where(r => r.Estado == estado).ToList();
                    if (!string.IsNullOrWhiteSpace(prioridad))
                        reclamos = reclamos.Where(r => r.Prioridad == prioridad).ToList();

                    int idx = 1;
                    var html = new System.Text.StringBuilder();
                    foreach (var reclamo in reclamos)
                    {
                        html.AppendLine($"<tr>");
                        html.AppendLine($"<td>{idx++}</td>");
                        html.AppendLine($"<td>{reclamo.Titulo}</td>");
                        html.AppendLine($"<td>{reclamo.UsuarioNombre}</td>");
                        html.AppendLine($"<td><span class='badge {GetEstadoBadgeClass(reclamo.Estado)}'>{reclamo.Estado}</span></td>");
                        html.AppendLine($"<td><span class='badge {GetPrioridadBadgeClass(reclamo.Prioridad)}'>{reclamo.Prioridad}</span></td>");
                        html.AppendLine($"<td>{reclamo.FechaCreacion:dd/MM/yyyy HH:mm}</td>");
                        html.AppendLine($"<td><a href='/Admin/DetalleReclamo/{reclamo.Id}' class='btn btn-sm btn-outline-primary'><i class='bi bi-eye'></i> Ver</a></td>");
                        html.AppendLine($"</tr>");
                    }
                    if (reclamos.Count == 0)
                    {
                        html.AppendLine("<tr><td colspan='7' class='text-center text-muted'>No se encontraron reclamos.</td></tr>");
                    }
                    return Content(html.ToString(), "text/html");
                }

                [HttpPost("ActualizarEstadoReclamo")]
                public async Task<IActionResult> ActualizarEstadoReclamo(Guid reclamoId, string nuevoEstado)
                {
                    try
                    {
                        if (!_authService.IsAuthenticated())
                            return RedirectToAction("Login", "Auth");

                        var userRole = await _authService.GetCurrentUserRoleAsync();
                        if (userRole != "Admin")
                            return RedirectToAction("Login", "Auth");

                        var token = _authService.GetToken();
                        if (!string.IsNullOrEmpty(token))
                            _apiService.SetAuthToken(token);

                        var result = await _apiService.UpdateReclamoEstadoAsync(reclamoId, nuevoEstado);
                        if (result)
                        {
                            TempData["SuccessMessage"] = "Estado actualizado correctamente.";
                        }
                        else
                        {
                            TempData["ErrorMessage"] = "No se pudo actualizar el estado.";
                        }
                        return RedirectToAction("DetalleReclamo", new { id = reclamoId });
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error al actualizar el estado del reclamo");
                        TempData["ErrorMessage"] = "Error al actualizar el estado.";
                        return RedirectToAction("DetalleReclamo", new { id = reclamoId });
                    }
                }

                [HttpPost("NuevaRespuesta")]
                public async Task<IActionResult> NuevaRespuesta(Guid reclamoId, string contenido)
                {
                    try
                    {
                        if (!_authService.IsAuthenticated())
                            return RedirectToAction("Login", "Auth");

                        var userRole = await _authService.GetCurrentUserRoleAsync();
                        if (userRole != "Admin")
                            return RedirectToAction("Login", "Auth");

                        var token = _authService.GetToken();
                        if (!string.IsNullOrEmpty(token))
                            _apiService.SetAuthToken(token);

                        // Crear DTO de respuesta y enviarla a la API
                        var usuario = await _authService.GetCurrentUserAsync();
                        var nuevaRespuesta = new CreateRespuestaDto
                        {
                            ReclamoId = reclamoId,
                            Contenido = contenido,
                            Mensaje = contenido,
                            UsuarioId = usuario?.Id ?? Guid.Empty
                        };

                        await _apiService.CreateRespuestaAsync(nuevaRespuesta);

                        TempData["SuccessMessage"] = "Respuesta enviada correctamente.";
                        return RedirectToAction("DetalleReclamo", new { id = reclamoId });
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error al enviar la respuesta");
                        TempData["ErrorMessage"] = "Error al enviar la respuesta.";
                        return RedirectToAction("DetalleReclamo", new { id = reclamoId });
                    }
                }

                [HttpGet("MiPerfil")]
                public async Task<IActionResult> MiPerfil()
                {
                    if (!_authService.IsAuthenticated())
                        return RedirectToAction("Login", "Auth");

                    var userRole = await _authService.GetCurrentUserRoleAsync();
                    if (userRole != "Admin")
                        return RedirectToAction("Login", "Auth");

                    var currentUser = await _authService.GetCurrentUserAsync();
                    if (currentUser == null)
                    {
                        TempData["ErrorMessage"] = "No se pudo obtener la información del usuario.";
                        return RedirectToAction("Login", "Auth");
                    }

                    ViewBag.CurrentUserRole = "Admin";
                    return View("~/Views/roles/Admin/MiPerfil.cshtml", currentUser);
                }

                [HttpGet("Configuracion")]
                public IActionResult Configuracion()
                {
                    ViewBag.CurrentUserRole = "Admin";
                    return View("~/Views/roles/Admin/Configuracion.cshtml");
                }

                [HttpGet("Notificaciones")]
                public IActionResult Notificaciones()
                {
                    ViewBag.CurrentUserRole = "Admin";
                    // Simulación de notificaciones para Admin
                    var notificaciones = new List<dynamic>
                    {
                        new {
                            Titulo = "Nuevo reclamo recibido",
                            Mensaje = "Se ha recibido un nuevo reclamo en el sistema.",
                            FechaCreacion = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Argentina Standard Time")).AddMinutes(-30),
                            Tipo = "reclamo",
                            Leida = false
                        },
                        new {
                            Titulo = "Respuesta enviada",
                            Mensaje = "Se ha enviado una respuesta a un reclamo.",
                            FechaCreacion = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Argentina Standard Time")).AddHours(-1),
                            Tipo = "respuesta",
                            Leida = true
                        },
                        new {
                            Titulo = "Estado actualizado",
                            Mensaje = "El estado de un reclamo ha cambiado a 'En Proceso'.",
                            FechaCreacion = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Argentina Standard Time")).AddHours(-2),
                            Tipo = "estado",
                            Leida = false
                        }
                    };
                    ViewBag.Notificaciones = notificaciones;
                    return View("~/Views/roles/Admin/Notificaciones.cshtml");
                }

                [HttpGet("Dashboard")]
                public async Task<IActionResult> Dashboard()
                {
                    try
                    {
                        if (!_authService.IsAuthenticated())
                            return RedirectToAction("Login", "Auth");

                        var userRole = await _authService.GetCurrentUserRoleAsync();
                        if (userRole != "Admin")
                            return RedirectToAction("Login", "Auth");

                        var currentUser = await _authService.GetCurrentUserAsync();
                        if (currentUser == null)
                        {
                            TempData["ErrorMessage"] = "No se pudo obtener la información del usuario.";
                            return RedirectToAction("Login", "Auth");
                        }

                        var token = _authService.GetToken();
                        if (!string.IsNullOrEmpty(token))
                            _apiService.SetAuthToken(token);

                        var todosLosReclamos = await _apiService.GetReclamosAsync();
                        var todosLosUsuarios = await _apiService.GetUsuariosAsync();

                        var estadisticasPorDia = todosLosReclamos
                            .GroupBy(r => r.FechaCreacion.ToString("yyyy-MM-dd"))
                            .OrderBy(g => g.Key)
                            .Select(g => new EstadisticaDiaria {
                                Fecha = g.Key,
                                Reclamos = g.Count(),
                                Usuarios = 0 // Si quieres mostrar usuarios por día, agrega lógica aquí
                            }).ToList();

                        var viewModel = new AdminDashboardViewModel
                        {
                            Usuario = currentUser,
                            TotalReclamos = todosLosReclamos.Count,
                            TotalUsuarios = todosLosUsuarios.Count,
                            UsuariosActivos = todosLosUsuarios.Count, // Por ahora todos son activos
                            ReclamosNuevos = todosLosReclamos.Count(r => r.Estado == "Nuevo"),
                            ReclamosEnProceso = todosLosReclamos.Count(r => r.Estado == "EnProceso" || r.Estado == "Respondido"),
                            ReclamosResueltos = todosLosReclamos.Count(r => r.Estado == "Cerrado"),
                            ReclamosRecientes = todosLosReclamos.OrderByDescending(r => r.FechaCreacion).Take(10).ToList(),
                            UsuariosRecientes = todosLosUsuarios.OrderByDescending(u => u.Id).Take(5).ToList(),
                            EstadisticasPorRol = todosLosUsuarios.GroupBy(u => u.Rol).ToDictionary(g => g.Key, g => g.Count()),
                            ReclamosPorPrioridad = todosLosReclamos.GroupBy(r => r.Prioridad).ToDictionary(g => g.Key, g => g.Count()),
                            ReclamosPorEstado = todosLosReclamos.GroupBy(r => r.Estado).ToDictionary(g => g.Key, g => g.Count()),
                            EstadisticasPorDia = estadisticasPorDia
                        };

                        ViewBag.CurrentUserRole = userRole;
                        return View("~/Views/roles/Admin/Dashboard.cshtml", viewModel);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error al cargar el dashboard de administrador");
                        TempData["ErrorMessage"] = "Error al cargar el dashboard.";
                        return RedirectToAction("Login", "Auth");
                    }
                }

                [HttpGet("GestionUsuarios")]
                public async Task<IActionResult> GestionUsuarios()
                {
                    try
                    {
                        if (!_authService.IsAuthenticated())
                            return RedirectToAction("Login", "Auth");

                        var userRole = await _authService.GetCurrentUserRoleAsync();
                        if (userRole != "Admin")
                            return RedirectToAction("Login", "Auth");

                        var token = _authService.GetToken();
                        if (!string.IsNullOrEmpty(token))
                            _apiService.SetAuthToken(token);

                        var usuarios = await _apiService.GetUsuariosAsync();

                        var viewModel = new GestionUsuariosViewModel
                        {
                            Usuarios = usuarios,
                            TotalUsuarios = usuarios.Count,
                            UsuariosPorRol = usuarios.GroupBy(u => u.Rol).ToDictionary(g => g.Key, g => g.Count())
                        };

                        ViewBag.CurrentUserRole = userRole;
                        return View("~/Views/roles/Admin/GestionUsuarios.cshtml", viewModel);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error al cargar la gestión de usuarios");
                        TempData["ErrorMessage"] = "Error al cargar los usuarios.";
                        return RedirectToAction("Login", "Auth");
                    }
                }

                [HttpGet("GestionReclamos")]
                public async Task<IActionResult> GestionReclamos()
                {
                    try
                    {
                        if (!_authService.IsAuthenticated())
                            return RedirectToAction("Login", "Auth");

                        var userRole = await _authService.GetCurrentUserRoleAsync();
                        if (userRole != "Admin")
                            return RedirectToAction("Login", "Auth");

                        var token = _authService.GetToken();
                        if (!string.IsNullOrEmpty(token))
                            _apiService.SetAuthToken(token);

                        var reclamos = await _apiService.GetReclamosAsync();
                        var usuarios = await _apiService.GetUsuariosAsync();

                        var viewModel = new AdminGestionReclamosViewModel
                        {
                            Reclamos = reclamos,
                            TotalReclamos = reclamos.Count,
                            ReclamosNuevos = reclamos.Count(r => r.Estado == "Nuevo"),
                            ReclamosEnProceso = reclamos.Count(r => r.Estado == "EnProceso"),
                            ReclamosResueltos = reclamos.Count(r => r.Estado == "Cerrado"),
                            UsuariosDict = usuarios.ToDictionary(u => u.Id, u => u.Nombre),
                            ReclamosPorMes = CalcularReclamosPorMes(reclamos),
                            PromedioTiempoResolucion = CalcularPromedioResolucion(reclamos)
                        };

                        ViewBag.CurrentUserRole = userRole;
                        return View("~/Views/roles/Admin/GestionReclamos.cshtml", viewModel);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error al cargar la gestión de reclamos del admin");
                        TempData["ErrorMessage"] = "Error al cargar los reclamos.";
                        return RedirectToAction("Login", "Auth");
                    }
                }

                [HttpGet("DetalleReclamo/{id}")]
                public async Task<IActionResult> DetalleReclamo(Guid id)
                {
                    try
                    {
                        if (!_authService.IsAuthenticated())
                            return RedirectToAction("Login", "Auth");

                        var userRole = await _authService.GetCurrentUserRoleAsync();
                        if (userRole != "Admin")
                            return RedirectToAction("Login", "Auth");

                        var token = _authService.GetToken();
                        if (!string.IsNullOrEmpty(token))
                            _apiService.SetAuthToken(token);

                        var reclamo = await _apiService.GetReclamoByIdAsync(id);
                        if (reclamo == null)
                        {
                            TempData["ErrorMessage"] = "No se encontró el reclamo.";
                            return RedirectToAction("GestionReclamos");
                        }

                        // Simulación de estados disponibles y permisos de edición
                        var estadosDisponibles = new List<string> { "Nuevo", "EnProceso", "Respondido", "Cerrado" };
                        bool puedeEditarEstado = true;

                        // Construir ViewModel fuertemente tipado para la vista
                        var viewModel = new SupportWeb.Models.ViewModels.ReclamoDetailsViewModel
                        {
                            Reclamo = reclamo,
                            Respuestas = reclamo.Respuestas ?? new List<RespuestaDto>(),
                            EstadosDisponibles = estadosDisponibles,
                            PuedeEditarEstado = puedeEditarEstado,
                            PuedeResponder = true // o lógica según permisos
                        };

                        ViewBag.CurrentUserRole = userRole;
                        return View("~/Views/roles/Admin/DetalleReclamo.cshtml", viewModel);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error al cargar el detalle del reclamo");
                        TempData["ErrorMessage"] = "Error al cargar el detalle del reclamo.";
                        return RedirectToAction("GestionReclamos");
                    }
                }

                [HttpGet("Reportes")]
                public IActionResult Reportes()
                {
                    // Datos ficticios para pruebas
                    var usuarios = new List<UsuarioDto>
                    {
                        new UsuarioDto { Id = Guid.NewGuid(), Nombre = "Juan Pérez", Rol = "Usuario" },
                        new UsuarioDto { Id = Guid.NewGuid(), Nombre = "Ana Gómez", Rol = "Usuario" },
                        new UsuarioDto { Id = Guid.NewGuid(), Nombre = "Soporte 1", Rol = "Soporte" },
                        new UsuarioDto { Id = Guid.NewGuid(), Nombre = "Soporte 2", Rol = "Soporte" },
                        new UsuarioDto { Id = Guid.NewGuid(), Nombre = "Admin", Rol = "Admin" }
                    };

                    var tiposReclamo = new List<string> { "Técnico", "Administrativo", "Satisfacción" };
                    var areas = new List<string> { "IT", "Recursos Humanos", "Atención al Cliente" };

                    var viewModel = new ReportesViewModel
                    {
                        TotalReclamos = 25,
                        TotalUsuarios = usuarios.Count,
                        ReclamosPorEstado = new Dictionary<string, int> {
                            { "Nuevo", 5 }, { "EnProceso", 8 }, { "Respondido", 7 }, { "Cerrado", 5 }
                        },
                        ReclamosPorPrioridad = new Dictionary<string, int> {
                            { "Alta", 10 }, { "Media", 9 }, { "Baja", 6 }
                        },
                        UsuariosPorRol = new Dictionary<string, int> {
                            { "Usuario", 2 }, { "Soporte", 2 }, { "Admin", 1 }
                        },
                        ReclamosPorMes = new Dictionary<string, int> {
                            { "2025-07", 8 }, { "2025-08", 17 }
                        },
                        TiempoPromedioResolucion = 4.9,
                        UsuarioConMasReclamos = "Juan Pérez",
                        PrioridadMasFrecuente = "Alta",
                        Usuarios = usuarios,
                        TiposReclamo = tiposReclamo,
                        Areas = areas,
                        ReclamosPorUsuario = new Dictionary<string, int> {
                            { "Juan Pérez", 11 }, { "Ana Gómez", 7 }
                        },
                        ReclamosPorArea = new Dictionary<string, int> {
                            { "IT", 12 }, { "Recursos Humanos", 6 }, { "Atención al Cliente", 7 }
                        },
                        ReclamosPorTipo = new Dictionary<string, int> {
                            { "Técnico", 13 }, { "Administrativo", 7 }, { "Satisfacción", 5 }
                        },
                        RankingUsuarios = new Dictionary<string, int> {
                            { "Juan Pérez", 11 }, { "Ana Gómez", 7 }
                        },
                        RankingSoporte = new Dictionary<string, int> {
                            { "Soporte 1", 9 }, { "Soporte 2", 8 }
                        },
                        PorcentajeResueltosEnTiempo = 80.0
                    };

                    // Datos ficticios para evolución histórica, mapa de calor y satisfacción
                    viewModel.EvolucionHistorica = new List<EvolucionHistoricaItem> {
                        new EvolucionHistoricaItem { Fecha = "2025-08-01", Cantidad = 2 },
                        new EvolucionHistoricaItem { Fecha = "2025-08-02", Cantidad = 3 },
                        new EvolucionHistoricaItem { Fecha = "2025-08-03", Cantidad = 5 }
                    };
                    viewModel.GetType().GetProperty("MapaCalor")?.SetValue(viewModel,
                        new Dictionary<string, int> {
                            { "Lunes", 4 }, { "Martes", 6 }, { "Miércoles", 8 }, { "Jueves", 3 }, { "Viernes", 4 }
                        });
                    viewModel.GetType().GetProperty("Satisfaccion")?.SetValue(viewModel,
                        new Dictionary<string, double> {
                            { "Soporte 1", 95.5 }, { "Soporte 2", 89.2 }
                        });

                    ViewBag.CurrentUserRole = "Admin";
                    return View("~/Views/roles/Admin/Reportes.cshtml", viewModel);
                }

                [HttpGet("EditarUsuario/{id}")]
                public async Task<IActionResult> EditarUsuario(Guid id)
                {
                    try
                    {
                        if (!_authService.IsAuthenticated())
                            return RedirectToAction("Login", "Auth");

                        var userRole = await _authService.GetCurrentUserRoleAsync();
                        if (userRole != "Admin")
                            return RedirectToAction("Login", "Auth");

                        var token = _authService.GetToken();
                        if (!string.IsNullOrEmpty(token))
                            _apiService.SetAuthToken(token);

                        var usuario = await _apiService.GetUsuarioByIdAsync(id);

                        if (usuario == null)
                        {
                            TempData["ErrorMessage"] = "Usuario no encontrado.";
                            return RedirectToAction("GestionUsuarios");
                        }

                        ViewBag.CurrentUserRole = userRole;
                        return View(usuario);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error al cargar el usuario para editar");
                        TempData["ErrorMessage"] = "Error al cargar el usuario.";
                        return RedirectToAction("GestionUsuarios");
                    }
                }

                [HttpPost("EditarUsuario/{id}")]
                public async Task<IActionResult> EditarUsuario(Guid id, UsuarioDto model)
                {
                    try
                    {
                        if (!_authService.IsAuthenticated())
                            return RedirectToAction("Login", "Auth");

                        var userRole = await _authService.GetCurrentUserRoleAsync();
                        if (userRole != "Admin")
                            return RedirectToAction("Login", "Auth");

                        var token = _authService.GetToken();
                        if (!string.IsNullOrEmpty(token))
                            _apiService.SetAuthToken(token);

                        if (!ModelState.IsValid)
                        {
                            ViewBag.CurrentUserRole = userRole;
                            return View(model);
                        }

                        var updateDto = new UpdateUsuarioDto
                        {
                            Id = id,
                            Nombre = model.Nombre,
                            Email = model.Email
                        };

                        var resultado = await _apiService.UpdateUsuarioAsync(id, updateDto);

                        if (resultado != null)
                        {
                            TempData["SuccessMessage"] = "Usuario actualizado exitosamente.";
                            return RedirectToAction("GestionUsuarios");
                        }
                        else
                        {
                            TempData["ErrorMessage"] = "Error al actualizar el usuario.";
                            return View(model);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error al actualizar el usuario");
                        TempData["ErrorMessage"] = "Error al actualizar el usuario.";
                        return RedirectToAction("GestionUsuarios");
                    }
                }

                [HttpPost("EliminarReclamo")]
                public async Task<IActionResult> EliminarReclamo(Guid reclamoId)
                {
                    try
                    {
                        if (!_authService.IsAuthenticated())
                            return Json(new { success = false, message = "Usuario no autenticado" });

                        var userRole = await _authService.GetCurrentUserRoleAsync();
                        if (userRole != "Admin")
                            return Json(new { success = false, message = "Sin permisos" });

                        var token = _authService.GetToken();
                        if (!string.IsNullOrEmpty(token))
                            _apiService.SetAuthToken(token);

                        var resultado = await _apiService.DeleteReclamoAsync(reclamoId);

                        if (resultado)
                        {
                            TempData["SuccessMessage"] = "Reclamo eliminado exitosamente.";
                            return RedirectToAction("GestionReclamos");
                        }
                        else
                        {
                            TempData["ErrorMessage"] = "Error al eliminar el reclamo.";
                            return RedirectToAction("DetalleReclamo", new { id = reclamoId });
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error al eliminar el reclamo {ReclamoId}", reclamoId);
                        return Json(new { success = false, message = "Error interno al eliminar el reclamo" });
                    }
                }

                private double CalcularPromedioResolucion(List<ReclamoDto> reclamos)
                {
                    var reclamosCerrados = reclamos.Where(r => r.Estado == "Cerrado").ToList();
                    if (!reclamosCerrados.Any())
                        return 0;

                    var zonaArgentina = TimeZoneInfo.FindSystemTimeZoneById("Argentina Standard Time");
                    var nowLocal = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, zonaArgentina);
                    return reclamosCerrados.Average(r => (nowLocal - r.FechaCreacion).TotalDays);
                }

                // Devuelve un diccionario con la cantidad de reclamos por mes (ej: {"2025-08": 5})
                private Dictionary<string, int> CalcularReclamosPorMes(List<ReclamoDto> reclamos)
                {
                    return reclamos
                        .GroupBy(r => r.FechaCreacion.ToString("yyyy-MM"))
                        .ToDictionary(g => g.Key, g => g.Count());
                }

                // Devuelve el nombre del usuario con más reclamos
                private string CalcularUsuarioConMasReclamos(List<ReclamoDto> reclamos, List<UsuarioDto> usuarios)
                {
                    var usuarioConMasReclamos = reclamos
                        .GroupBy(r => r.UsuarioId)
                        .OrderByDescending(g => g.Count())
                        .FirstOrDefault();

                    if (usuarioConMasReclamos == null)
                        return "N/A";

                    var usuario = usuarios.FirstOrDefault(u => u.Id == usuarioConMasReclamos.Key);
                    return usuario?.Nombre ?? "Usuario no encontrado";
                }
                // POST: /Admin/EnviarCodigo2FA
                [HttpPost("EnviarCodigo2FA")]
                public async Task<IActionResult> EnviarCodigo2FA()
                {
                    if (!_authService.IsAuthenticated())
                        return Json(new { success = false, message = "No autenticado" });

                    var currentUser = await _authService.GetCurrentUserAsync();
                    if (currentUser == null)
                        return Json(new { success = false, message = "Usuario no encontrado" });

                    var random = new Random();
                    var code = random.Next(100000, 999999).ToString();
                    HttpContext.Session.SetString($"2FA_{currentUser.Id}", code);
                    await _apiService.EnviarCodigo2FAEmailAsync(currentUser.Email, code);
                    return Json(new { success = true });
                }

                // POST: /Admin/VerificarCodigo2FA
                [HttpPost("VerificarCodigo2FA")]
                public async Task<IActionResult> VerificarCodigo2FA([FromBody] dynamic payload)
                {
                    if (!_authService.IsAuthenticated())
                        return Json(new { success = false, message = "No autenticado" });

                    var currentUser = await _authService.GetCurrentUserAsync();
                    if (currentUser == null)
                        return Json(new { success = false, message = "Usuario no encontrado" });

                    string code = payload.code;
                    var storedCode = HttpContext.Session.GetString($"2FA_{currentUser.Id}");
                    if (storedCode == code)
                    {
                        await _apiService.Activar2FAUsuarioAsync(currentUser.Id);
                        return Json(new { success = true });
                    }
                    return Json(new { success = false, message = "Código incorrecto" });
                }
            }
        }

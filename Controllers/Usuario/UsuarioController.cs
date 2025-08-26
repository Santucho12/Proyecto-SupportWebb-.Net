        using Microsoft.AspNetCore.Mvc;
        using SupportWeb.Services;
        using SupportWeb.Models.DTOs;
        using SupportWeb.Models.ViewModels;
        using SupportWeb.Models.ViewModels.Usuario;

        namespace SupportWeb.Controllers.Usuario
        {
            public class UsuarioController : Controller
            {
                // ...existing code...
                // POST: /Usuario/MarcarNotificacionLeida
                [HttpPost]
                [Route("Usuario/MarcarNotificacionLeida")]
                public async Task<IActionResult> MarcarNotificacionLeida(Guid id)
                {
                    if (!_authService.IsAuthenticated())
                        return Json(new { success = false, message = "No autenticado" });

                    var token = _authService.GetStoredToken();
                    if (!string.IsNullOrEmpty(token))
                        _apiService.SetAuthToken(token);

                    // Lógica para marcar la notificación como leída
                    var ok = await _apiService.MarcarNotificacionLeidaAsync(id);
                    return Json(new { success = ok });
                }

                // POST: /Usuario/EliminarNotificacion
                [HttpPost]
                [Route("Usuario/EliminarNotificacion")]
                public async Task<IActionResult> EliminarNotificacion(Guid id)
                {
                    if (!_authService.IsAuthenticated())
                        return Json(new { success = false, message = "No autenticado" });

                    var token = _authService.GetStoredToken();
                    if (!string.IsNullOrEmpty(token))
                        _apiService.SetAuthToken(token);

                    // Lógica para eliminar la notificación
                    var ok = await _apiService.EliminarNotificacionAsync(id);
                    return Json(new { success = ok });
                }
                private readonly IAuthService _authService;
                private readonly IApiService _apiService;
                private readonly ILogger<UsuarioController> _logger;
// POST: /Soporte/GuardarConfiguracion
    [HttpPost]
    [Route("Usuario/GuardarConfiguracion")]
        public async Task<IActionResult> GuardarConfiguracion([FromBody] dynamic config)
        {
            if (!_authService.IsAuthenticated())
                return Json(new { success = false, message = "No autenticado" });

            // Aquí podrías guardar la configuración en la base de datos o en el usuario
            // Ejemplo: simplemente loguear los valores recibidos
            _logger.LogInformation($"Configuración recibida: {config}");

            // Simulación de guardado exitoso
            return Json(new { success = true, message = "Configuración guardada correctamente" });
        }
                public UsuarioController(IAuthService authService, IApiService apiService, ILogger<UsuarioController> logger)
                {
                    _authService = authService;
                    _apiService = apiService;
                    _logger = logger;
                }

                // GET: /Usuario/Configuracion
                public IActionResult Configuracion()
                {
                    return View("~/Views/roles/Usuario/Configuracion.cshtml");
                }

                // GET: /Usuario/Dashboard
                public async Task<IActionResult> Dashboard()
                {
                        _logger.LogInformation("=== USUARIO DASHBOARD EJECUTADO ===");
                        ViewBag.CurrentUserRole = "Usuario";

                    if (!_authService.IsAuthenticated())
                    {
                        _logger.LogWarning("Usuario no autenticado intentando acceder al dashboard");
                        return RedirectToAction("Login", "Auth");
                    }

                    var userRole = await _authService.GetCurrentUserRoleAsync();
                    if (userRole != "Usuario")
                    {
                        _logger.LogWarning("Usuario con rol {Role} intentando acceder al dashboard de usuarios", userRole);
                        return RedirectToAction("Login", "Auth");
                    }

                    try
                    {
                        var token = _authService.GetStoredToken();
                        if (!string.IsNullOrEmpty(token))
                        {
                            _apiService.SetAuthToken(token);
                        }

                        var currentUser = await _authService.GetCurrentUserAsync();
                        var viewModel = new UsuarioDashboardViewModel();

                        if (currentUser != null)
                        {
                            viewModel.Usuario = currentUser;

                            var reclamos = await _apiService.GetReclamosAsync();
                            viewModel.ReclamosRecientes = reclamos?.Take(5).ToList() ?? new List<ReclamoDto>();

                            viewModel.TotalReclamos = reclamos?.Count ?? 0;
                            viewModel.ReclamosNuevos = reclamos?.Count(r => r.Estado == "Nuevo") ?? 0;
                            viewModel.ReclamosEnProceso = reclamos?.Count(r => r.Estado == "En Proceso" || r.Estado == "Respondido") ?? 0;
                            viewModel.ReclamosResueltos = reclamos?.Count(r => r.Estado == "Cerrado") ?? 0;
                            viewModel.ReclamosCerrados = viewModel.ReclamosResueltos;

                            var notificaciones = await _apiService.GetNotificacionesNoVistasAsync(currentUser.Id);
                            viewModel.NotificacionesRecientes = notificaciones.Take(5).ToList();
                            viewModel.CantidadNotificaciones = notificaciones.Count();
                        }

                        return View("~/Views/roles/Usuario/Dashboard.cshtml", viewModel);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error al cargar el dashboard del usuario");
                        TempData["ErrorMessage"] = "Error al cargar el dashboard";
                        return RedirectToAction("Login", "Auth");
                    }
                }

                // GET: /Usuario/MisReclamos
                public async Task<IActionResult> MisReclamos()
                {
                        ViewBag.CurrentUserRole = "Usuario";
                        if (!_authService.IsAuthenticated())
                        {
                            return RedirectToAction("Login", "Auth");
                        }

                        var userRole = await _authService.GetCurrentUserRoleAsync();
                        if (userRole != "Usuario")
                        {
                            return RedirectToAction("Login", "Auth");
                        }

                    try
                    {
                        var token = _authService.GetStoredToken();
                        if (!string.IsNullOrEmpty(token))
                        {
                            _apiService.SetAuthToken(token);
                        }

                        var reclamos = await _apiService.GetReclamosAsync();
                        return View("~/Views/roles/Usuario/MisReclamos.cshtml", reclamos ?? new List<ReclamoDto>());
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error al obtener reclamos del usuario");
                        TempData["ErrorMessage"] = "Error al cargar los reclamos";
                        return View("~/Views/roles/Usuario/MisReclamos.cshtml", new List<ReclamoDto>());
                    }
                }

                // GET: /Usuario/CrearReclamo
                public async Task<IActionResult> CrearReclamo()
                {
                        ViewBag.CurrentUserRole = "Usuario";
                        if (!_authService.IsAuthenticated())
                        {
                            return RedirectToAction("Login", "Auth");
                        }

                    var userRole = await _authService.GetCurrentUserRoleAsync();
                    if (userRole != "Usuario")
                    {
                        return RedirectToAction("Login", "Auth");
                    }

                    return View("~/Views/roles/Usuario/CrearReclamo.cshtml");
                }

                // POST: /Usuario/CrearReclamo
                [HttpPost]
                public async Task<IActionResult> CrearReclamo(CreateReclamoDto model)
                {
                    if (!_authService.IsAuthenticated())
                    {
                        return RedirectToAction("Login", "Auth");
                    }

                    var userRole = await _authService.GetCurrentUserRoleAsync();
                    if (userRole != "Usuario")
                    {
                        return RedirectToAction("Login", "Auth");
                    }

                    try
                    {
                        var currentUser = await _authService.GetCurrentUserAsync();
                        if (currentUser == null)
                        {
                            TempData["ErrorMessage"] = "Error al obtener información del usuario";
                            var viewModel = new CreateReclamoViewModel
                            {
                                Titulo = model.Titulo,
                                Descripcion = model.Descripcion,
                                Prioridad = model.Prioridad
                            };
                            return View("~/Views/roles/Usuario/CrearReclamo.cshtml", viewModel);
                        }

                        model.UsuarioId = currentUser.Id;

                        var token = _authService.GetStoredToken();
                        if (!string.IsNullOrEmpty(token))
                        {
                            _apiService.SetAuthToken(token);
                        }

                        var result = await _apiService.CreateReclamoAsync(model);
                        if (result != null)
                        {
                            TempData["SuccessMessage"] = "Reclamo creado exitosamente";
                            return RedirectToAction("MisReclamos");
                        }
                        else
                        {
                            TempData["ErrorMessage"] = "Error al crear el reclamo";
                            var viewModel = new CreateReclamoViewModel
                            {
                                Titulo = model.Titulo,
                                Descripcion = model.Descripcion,
                                Prioridad = model.Prioridad
                            };
                            return View("~/Views/roles/Usuario/CrearReclamo.cshtml", viewModel);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error al crear reclamo");
                        TempData["ErrorMessage"] = "Error interno del servidor";
                        var viewModel = new CreateReclamoViewModel
                        {
                            Titulo = model.Titulo,
                            Descripcion = model.Descripcion,
                            Prioridad = model.Prioridad
                        };
                        return View("~/Views/roles/Usuario/CrearReclamo.cshtml", viewModel);
                    }
                }

                // GET: /Usuario/DetalleReclamo
                public async Task<IActionResult> DetalleReclamo(Guid id)
                {
                    if (!_authService.IsAuthenticated())
                    {
                        return RedirectToAction("Login", "Auth");
                    }

                    var userRole = await _authService.GetCurrentUserRoleAsync();
                    if (userRole != "Usuario")
                    {
                        return RedirectToAction("Login", "Auth");
                    }

                    try
                    {
                        var token = _authService.GetStoredToken();
                        if (!string.IsNullOrEmpty(token))
                        {
                            _apiService.SetAuthToken(token);
                        }

                        var reclamo = await _apiService.GetReclamoByIdAsync(id);
                        if (reclamo == null)
                        {
                            TempData["ErrorMessage"] = "Reclamo no encontrado";
                            return RedirectToAction("MisReclamos");
                        }

                        var respuestas = await _apiService.GetRespuestasByReclamoAsync(id);
                        ViewBag.Respuestas = respuestas ?? new List<RespuestaDto>();

                        return View("~/Views/roles/Usuario/DetalleReclamo.cshtml", reclamo);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error al obtener detalle del reclamo {Id}", id);
                        TempData["ErrorMessage"] = "Error al cargar el detalle del reclamo";
                        return RedirectToAction("MisReclamos");
                    }
                }

                // GET: /Usuario/VerRespuesta/{id}
                public async Task<IActionResult> VerRespuesta(Guid id)
                {
                    if (!_authService.IsAuthenticated())
                    {
                        return RedirectToAction("Login", "Auth");
                    }

                    var userRole = await _authService.GetCurrentUserRoleAsync();
                    if (userRole != "Usuario")
                    {
                        return RedirectToAction("Login", "Auth");
                    }

                    try
                    {
                        var token = _authService.GetStoredToken();
                        if (!string.IsNullOrEmpty(token))
                        {
                            _apiService.SetAuthToken(token);
                        }

                        var reclamo = await _apiService.GetReclamoByIdAsync(id);
                        if (reclamo == null)
                        {
                            TempData["ErrorMessage"] = "Reclamo no encontrado";
                            return RedirectToAction("MisReclamos");
                        }

                        var respuestas = await _apiService.GetRespuestasByReclamoAsync(id);
                        var respuesta = (respuestas != null && respuestas.Count > 0)
                            ? respuestas.OrderByDescending(r => r.FechaCreacion).FirstOrDefault()
                            : null;
                        ViewBag.Respuesta = respuesta;
                        ViewBag.Reclamo = reclamo;
                        return View("~/Views/roles/Usuario/VerRespuesta.cshtml");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error al obtener la respuesta de soporte para el reclamo {Id}", id);
                        TempData["ErrorMessage"] = "Error al cargar la respuesta de soporte.";
                        return RedirectToAction("MisReclamos");
                    }
                }

                // GET: /Usuario/MiPerfil
                public async Task<IActionResult> MiPerfil()
                {
                    if (!_authService.IsAuthenticated())
                    {
                        return RedirectToAction("Login", "Auth");
                    }

                    var userRole = await _authService.GetCurrentUserRoleAsync();
                    if (userRole != "Usuario")
                    {
                        return RedirectToAction("Login", "Auth");
                    }

                    try
                    {
                        var currentUser = await _authService.GetCurrentUserAsync();
                        if (currentUser == null)
                        {
                            TempData["ErrorMessage"] = "Error al obtener información del usuario";
                            return RedirectToAction("Dashboard", "Usuario");
                        }

                        return View("~/Views/roles/Usuario/MiPerfil.cshtml", currentUser);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error al cargar el perfil del usuario");
                        TempData["ErrorMessage"] = "Error al cargar el perfil";
                        return RedirectToAction("Dashboard", "Usuario");
                    }
                }

                // POST: /Usuario/MiPerfil
                [HttpPost]
                public async Task<IActionResult> MiPerfil(UsuarioDto model)
                {
                    if (!_authService.IsAuthenticated())
                    {
                        return RedirectToAction("Login", "Auth");
                    }

                    var userRole = await _authService.GetCurrentUserRoleAsync();
                    if (userRole != "Usuario")
                    {
                        return RedirectToAction("Login", "Auth");
                    }

                    try
                    {
                        var token = _authService.GetStoredToken();
                        if (!string.IsNullOrEmpty(token))
                        {
                            _apiService.SetAuthToken(token);
                        }

                        var updateDto = new UpdateUsuarioDto
                        {
                            Id = model.Id,
                            Nombre = model.Nombre,
                            Email = model.Email
                        };

                        var result = await _apiService.UpdateUsuarioAsync(model.Id, updateDto);
                        if (result != null)
                        {
                            TempData["SuccessMessage"] = "Perfil actualizado exitosamente";
                        }
                        else
                        {
                            TempData["ErrorMessage"] = "Error al actualizar el perfil";
                        }

                        return View("~/Views/roles/Usuario/MiPerfil.cshtml", model);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error al actualizar perfil del usuario");
                        TempData["ErrorMessage"] = "Error interno del servidor";
                        return View("~/Views/roles/Usuario/MiPerfil.cshtml", model);
                    }
                }

                // GET: /Usuario/Notificaciones
                public async Task<IActionResult> Notificaciones()
                {
                    if (!_authService.IsAuthenticated())
                    {
                        return RedirectToAction("Login", "Auth");
                    }

                    var userRole = await _authService.GetCurrentUserRoleAsync();
                    if (userRole != "Usuario")
                    {
                        return RedirectToAction("Login", "Auth");
                    }

                    try
                    {
                        var currentUser = await _authService.GetCurrentUserAsync();
                        if (currentUser == null)
                        {
                            TempData["ErrorMessage"] = "Error al obtener información del usuario";
                            return RedirectToAction("Dashboard", "Usuario");
                        }

                        var token = _authService.GetStoredToken();
                        if (!string.IsNullOrEmpty(token))
                        {
                            _apiService.SetAuthToken(token);
                        }

                        var notificaciones = await _apiService.GetNotificacionesNoVistasAsync(currentUser.Id);
                        if (notificaciones == null || notificaciones.Count == 0)
                        {
                            notificaciones = new List<NotificacionDto>
                            {
                                new NotificacionDto
                                {
                                    Id = Guid.NewGuid(),
                                    Titulo = "Nuevo reclamo recibido",
                                    Mensaje = "Tienes un nuevo reclamo asignado.",
                                    Leida = false,
                                    Tipo = "warning",
                                    FechaCreacion = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Argentina Standard Time")).AddMinutes(-10),
                                    UsuarioId = currentUser.Id
                                },
                                new NotificacionDto
                                {
                                    Id = Guid.NewGuid(),
                                    Titulo = "Reclamo actualizado",
                                    Mensaje = "Uno de tus reclamos ha sido actualizado.",
                                    Leida = false,
                                    Tipo = "info",
                                    FechaCreacion = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Argentina Standard Time")).AddMinutes(-20),
                                    UsuarioId = currentUser.Id
                                },
                                new NotificacionDto
                                {
                                    Id = Guid.NewGuid(),
                                    Titulo = "Respuesta pendiente",
                                    Mensaje = "Tienes una respuesta pendiente de leer.",
                                    Leida = false,
                                    Tipo = "danger",
                                    FechaCreacion = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Argentina Standard Time")).AddMinutes(-30),
                                    UsuarioId = currentUser.Id
                                }
                            };
                        }
                        // Agregar la última respuesta y su fecha a cada notificación
                        foreach (var notificacion in notificaciones)
                        {
                            if (notificacion.ReclamoId.HasValue)
                            {
                                var respuestas = await _apiService.GetRespuestasByReclamoAsync(notificacion.ReclamoId.Value);
                                if (respuestas != null && respuestas.Count > 0)
                                {
                                    var ultima = respuestas.OrderByDescending(r => r.FechaCreacion).FirstOrDefault();
                                    notificacion.UltimaRespuesta = ultima?.Contenido;
                                    notificacion.FechaUltimaRespuesta = ultima?.FechaCreacion;
                                }
                            }
                        }
                        ViewBag.TotalNotificaciones = notificaciones.Count;
                        ViewBag.NoLeidas = notificaciones.Count(n => !n.Leida);
                        ViewBag.Leidas = notificaciones.Count(n => n.Leida);
                        ViewBag.Notificaciones = notificaciones;
                        return View("~/Views/roles/Usuario/Notificaciones.cshtml");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error al obtener notificaciones del usuario");
                        TempData["ErrorMessage"] = "Error al cargar las notificaciones";
                        return View("~/Views/roles/Usuario/Notificaciones.cshtml");
                    }
                }

                // POST: /Usuario/MarcarRespondido
                [HttpPost]
                public async Task<IActionResult> MarcarRespondido(Guid id)
                {
                    if (!_authService.IsAuthenticated())
                        return Json(new { success = false, message = "No autenticado" });

                    var token = _authService.GetStoredToken();
                    if (!string.IsNullOrEmpty(token))
                        _apiService.SetAuthToken(token);

                    var ok = await _apiService.UpdateReclamoEstadoAsync(id, "Respondido");
                    return Json(new { success = ok });
                }

                // POST: /Usuario/CerrarCaso
                [HttpPost]
                public async Task<IActionResult> CerrarCaso(Guid id)
                {
                    if (!_authService.IsAuthenticated())
                        return Json(new { success = false, message = "No autenticado" });

                    var token = _authService.GetStoredToken();
                    if (!string.IsNullOrEmpty(token))
                        _apiService.SetAuthToken(token);

                    var ok = await _apiService.UpdateReclamoEstadoAsync(id, "Cerrado");
                    return Json(new { success = ok });
                }

                // POST: /Usuario/EliminarCaso
                [HttpPost]
                public async Task<IActionResult> EliminarCaso(Guid id)
                {
                    if (!_authService.IsAuthenticated())
                        return Json(new { success = false, message = "No autenticado" });

                    var token = _authService.GetStoredToken();
                    if (!string.IsNullOrEmpty(token))
                        _apiService.SetAuthToken(token);

                    var ok = await _apiService.DeleteReclamoAsync(id);
                    return Json(new { success = ok });
                }
                // POST: /Usuario/EnviarCodigo2FA
                [HttpPost]
                public async Task<IActionResult> EnviarCodigo2FA()
                {
                    if (!_authService.IsAuthenticated())
                        return Json(new { success = false, message = "No autenticado" });

                    var currentUser = await _authService.GetCurrentUserAsync();
                    if (currentUser == null)
                        return Json(new { success = false, message = "Usuario no encontrado" });

                    // Generar código aleatorio de 6 dígitos
                    var random = new Random();
                    var code = random.Next(100000, 999999).ToString();

                    // Guardar el código en sesión (puedes usar DB en producción)
                    HttpContext.Session.SetString($"2FA_{currentUser.Id}", code);

                    // Enviar el código por email (puedes agregar SMS)
                    await _apiService.EnviarCodigo2FAEmailAsync(currentUser.Email, code);

                    return Json(new { success = true });
                }

                // POST: /Usuario/VerificarCodigo2FA
                [HttpPost]
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
                        // Aquí puedes marcar 2FA como activado en la DB
                        await _apiService.Activar2FAUsuarioAsync(currentUser.Id);
                        return Json(new { success = true });
                    }
                    return Json(new { success = false, message = "Código incorrecto" });
                }
            }
        }

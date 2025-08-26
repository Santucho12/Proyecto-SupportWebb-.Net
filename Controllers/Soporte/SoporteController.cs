        using Microsoft.AspNetCore.Mvc;
        using SupportWeb.Services;
        using SupportWeb.Models.DTOs;
        using SupportWeb.Models.ViewModels.Soporte;
        using System;
        using System.Linq;
        using System.Threading.Tasks;

        namespace SupportWeb.Controllers.Soporte
        {
            [Route("Soporte")]
            public class SoporteController : Controller
            {
                private readonly IAuthService _authService;
                private readonly IApiService _apiService;
                private readonly ILogger<SoporteController> _logger;

                [HttpPost("MarcarNotificacionVisto")]
                public IActionResult MarcarNotificacionVisto(string id)
                {
                    if (!_authService.IsAuthenticated())
                        return Json(new { success = false, message = "No autenticado" });

                    // Aquí deberías marcar la notificación como vista en la base de datos
                    // Como no hay persistencia, simplemente simula éxito
                    return Json(new { success = true, message = "Notificación marcada como vista" });
                }

                [HttpPost("EliminarNotificacion")]
                public IActionResult EliminarNotificacion(string id)
                {
                    if (!_authService.IsAuthenticated())
                        return Json(new { success = false, message = "No autenticado" });

                    // Aquí deberías eliminar la notificación en la base de datos
                    // Como no hay persistencia, simplemente simula éxito
                    return Json(new { success = true, message = "Notificación eliminada" });
                }

        // POST: /Soporte/GuardarConfiguracion
        [HttpPost("GuardarConfiguracion")]
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
                [HttpGet("ExportarReclamosCsv")]
                public async Task<IActionResult> ExportarReclamosCsv()
                {
                    if (!_authService.IsAuthenticated())
                        return RedirectToAction("Login", "Auth");

                    var userRole = await _authService.GetCurrentUserRoleAsync();
                    if (userRole != "Soporte")
                        return RedirectToAction("Login", "Auth");

                    var token = _authService.GetToken();
                    if (!string.IsNullOrEmpty(token))
                        _apiService.SetAuthToken(token);

                    var reclamos = await _apiService.GetReclamosAsync();

                    using (var memoryStream = new System.IO.MemoryStream())
                    {
                        // Escribir BOM UTF-8 para que Excel reconozca la codificación
                        var bom = new byte[] { 0xEF, 0xBB, 0xBF };
                        memoryStream.Write(bom, 0, bom.Length);

                        using (var writer = new System.IO.StreamWriter(memoryStream, System.Text.Encoding.UTF8, 1024, true))
                        using (var csv = new CsvHelper.CsvWriter(writer, System.Globalization.CultureInfo.InvariantCulture))
                        {
                            // Usar un ClassMap para forzar comillas en todos los campos
                            csv.Context.RegisterClassMap<ReclamoDtoMap>();
                            csv.WriteRecords(reclamos);
                            writer.Flush();
                        }
                        memoryStream.Position = 0;
                        return File(memoryStream.ToArray(), "text/csv", "reclamos.csv");
                    }
                }

        // TypeConverter personalizado para forzar comillas en todos los campos
        // POST: /Soporte/EnviarCodigo2FA
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

        // POST: /Soporte/VerificarCodigo2FA
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

        public class AlwaysQuoteConverter : CsvHelper.TypeConversion.StringConverter
        {
            public override string ConvertToString(object value, CsvHelper.IWriterRow row, CsvHelper.Configuration.MemberMapData memberMapData)
            {
                var str = base.ConvertToString(value, row, memberMapData);
                return $"\"{str?.Replace("\"", "\"\"")}"; // Escapa comillas internas
            }
        }

        // Mapeo personalizado usando el TypeConverter
        public sealed class ReclamoDtoMap : CsvHelper.Configuration.ClassMap<ReclamoDto>
        {
            public ReclamoDtoMap()
            {
                AutoMap(System.Globalization.CultureInfo.InvariantCulture);
                Map(m => m.Id).TypeConverter(new AlwaysQuoteConverter());
                Map(m => m.Titulo).TypeConverter(new AlwaysQuoteConverter());
                Map(m => m.Descripcion).TypeConverter(new AlwaysQuoteConverter());
                Map(m => m.Estado).TypeConverter(new AlwaysQuoteConverter());
                Map(m => m.Prioridad).TypeConverter(new AlwaysQuoteConverter());
                Map(m => m.FechaCreacion).TypeConverter(new AlwaysQuoteConverter());
                Map(m => m.UsuarioId).TypeConverter(new AlwaysQuoteConverter());
                Map(m => m.UsuarioNombre).TypeConverter(new AlwaysQuoteConverter());
            }
        }

                [HttpGet("Reportes")]
                public IActionResult Reportes()
                {
                    if (!_authService.IsAuthenticated())
                        return RedirectToAction("Login", "Auth");

                    var userRole = _authService.GetCurrentUserRoleAsync().Result;
                    if (userRole != "Soporte")
                        return RedirectToAction("Login", "Auth");

                    ViewBag.CurrentUserRole = userRole;
                    return View("~/Views/roles/Soporte/Reportes.cshtml");
                }

                [HttpGet("CasosUrgentes")]
                public async Task<IActionResult> CasosUrgentes()
                {
                    if (!_authService.IsAuthenticated())
                        return RedirectToAction("Login", "Auth");

                    var userRole = await _authService.GetCurrentUserRoleAsync();
                    if (userRole != "Soporte")
                        return RedirectToAction("Login", "Auth");

                    var token = _authService.GetToken();
                    if (!string.IsNullOrEmpty(token))
                        _apiService.SetAuthToken(token);

                    var reclamos = await _apiService.GetReclamosAsync();
                    var urgentesSinCerrados = reclamos
                        .Where(r => r.Prioridad != null && r.Prioridad.Trim().Equals("Alta", StringComparison.OrdinalIgnoreCase)
                                 && r.Estado != null && !r.Estado.Trim().Equals("Cerrado", StringComparison.OrdinalIgnoreCase))
                        .OrderByDescending(r => r.FechaCreacion)
                        .ToList();

                    var currentUser = await _authService.GetCurrentUserAsync();
                    var zonaArgentina = TimeZoneInfo.FindSystemTimeZoneById("Argentina Standard Time");
                    var nowLocal = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, zonaArgentina);
                    var viewModel = new CasosUrgentesViewModel
                    {
                        UsuarioNombre = currentUser?.Nombre ?? string.Empty,
                        CasosUrgentes = urgentesSinCerrados,
                        TotalUrgentes = urgentesSinCerrados.Count,
                        UrgentesNuevos = urgentesSinCerrados.Count(r => r.Estado == "Nuevo"),
                        UrgentesEnProceso = urgentesSinCerrados.Count(r => r.Estado == "EnProceso" || r.Estado == "En Proceso" || r.Estado == "Respondido"),
                        TiempoPromedioAtencion = urgentesSinCerrados.Any() ? urgentesSinCerrados.Average(r => (nowLocal - r.FechaCreacion).TotalHours) : 0,
                        CasosEscalados = urgentesSinCerrados.Where(r => r.Prioridad == "Alta" && r.Estado == "Escalado").ToList(),
                        // UrgentesCategoria = urgentesSinCerrados.GroupBy(r => r.Categoria ?? "Sin categoría").ToDictionary(g => g.Key, g => g.Count()),
                        CasosCriticos = urgentesSinCerrados.Where(r => r.Prioridad == "Crítica" || r.Prioridad == "Critica").ToList()
                    };

                    ViewBag.CurrentUserRole = userRole;
                    return View("~/Views/roles/Soporte/CasosUrgentes.cshtml", viewModel);
                }

                [HttpGet("SeleccionarReclamoParaResponder")]
                public async Task<IActionResult> SeleccionarReclamoParaResponder()
                {
                    if (!_authService.IsAuthenticated())
                        return RedirectToAction("Login", "Auth");

                    var userRole = await _authService.GetCurrentUserRoleAsync();
                    if (userRole != "Soporte")
                        return RedirectToAction("Login", "Auth");

                    var token = _authService.GetToken();
                    if (!string.IsNullOrEmpty(token))
                        _apiService.SetAuthToken(token);

                    var reclamos = await _apiService.GetReclamosAsync();
                    var reclamosAbiertos = reclamos.Where(r => r.Estado != "Cerrado").OrderByDescending(r => r.FechaCreacion).ToList();

                    ViewBag.CurrentUserRole = userRole;
                    return View("~/Views/roles/Soporte/SeleccionarReclamoParaResponder.cshtml", reclamosAbiertos);
                }

                public SoporteController(IAuthService authService, IApiService apiService, ILogger<SoporteController> logger)
                {
                    _authService = authService;
                    _apiService = apiService;
                    _logger = logger;
                }

                [HttpGet("Notificaciones")]
                public IActionResult Notificaciones()
                {
                    if (!_authService.IsAuthenticated())
                        return RedirectToAction("Login", "Auth");

                    var userRole = _authService.GetCurrentUserRoleAsync().Result;
                    if (userRole != "Soporte")
                        return RedirectToAction("Login", "Auth");

                    var token = _authService.GetStoredToken();
                    if (!string.IsNullOrEmpty(token))
                        _apiService.SetAuthToken(token);

                    // Obtener reclamos recientes
                    var reclamos = _apiService.GetReclamosAsync().Result;
                    // Obtener respuestas recientes
                    var respuestas = new List<RespuestaDto>();
                    if (reclamos != null && reclamos.Any())
                    {
                        foreach (var reclamo in reclamos)
                        {
                            if (reclamo.Respuestas != null)
                                respuestas.AddRange(reclamo.Respuestas);
                        }
                    }

                    // Construir notificaciones a partir de reclamos y respuestas
                    var notificaciones = new List<dynamic>();
                    // Reclamos nuevos
                    notificaciones.AddRange(reclamos.Where(r => r.Estado == "Nuevo")
                        .Select(r => new {
                            Tipo = "reclamo",
                            Titulo = "Nuevo reclamo recibido",
                            Mensaje = r.Titulo,
                            FechaCreacion = r.FechaCreacion,
                            Leida = false
                        }));
                    // Respuestas recientes
                    notificaciones.AddRange(respuestas
                        .OrderByDescending(r => r.FechaRespuesta)
                        .Take(10)
                        .Select(r => new {
                            Tipo = "respuesta",
                            Titulo = "Nueva respuesta en reclamo",
                            Mensaje = r.Contenido,
                            FechaCreacion = r.FechaRespuesta,
                            Leida = false
                        }));
                    // Cambios de estado recientes
                    notificaciones.AddRange(reclamos.Where(r => r.Estado != "Nuevo")
                        .OrderByDescending(r => r.FechaCreacion)
                        .Take(10)
                        .Select(r => new {
                            Tipo = "estado",
                            Titulo = $"Reclamo cambiado a estado {r.Estado}",
                            Mensaje = r.Titulo,
                            FechaCreacion = r.FechaCreacion,
                            Leida = false
                        }));

                    var listaNotificaciones = notificaciones.OrderByDescending(n => n.FechaCreacion).ToList();
                    ViewBag.CurrentUserRole = userRole;
                    ViewBag.Notificaciones = listaNotificaciones;
                    ViewBag.TotalNotificaciones = listaNotificaciones.Count;
                    ViewBag.Leidas = listaNotificaciones.Count(n => n.Leida == true);
                    ViewBag.NoLeidas = listaNotificaciones.Count - ViewBag.Leidas;
                    return View("~/Views/roles/Soporte/Notificaciones.cshtml");
                }

                [HttpGet("MiPerfil")]
                public async Task<IActionResult> MiPerfil()
                {
                    if (!_authService.IsAuthenticated())
                        return RedirectToAction("Login", "Auth");

                    var userRole = await _authService.GetCurrentUserRoleAsync();
                    if (userRole != "Soporte")
                        return RedirectToAction("Login", "Auth");

                    var currentUser = await _authService.GetCurrentUserAsync();
                    if (currentUser == null)
                    {
                        TempData["ErrorMessage"] = "No se pudo obtener la información del usuario.";
                        return RedirectToAction("Dashboard");
                    }

                    ViewBag.CurrentUserRole = userRole;
                    return View("~/Views/roles/Soporte/MiPerfil.cshtml", currentUser);
                }

                [HttpGet("Configuracion")]
                public IActionResult Configuracion()
                {
                    if (!_authService.IsAuthenticated())
                        return RedirectToAction("Login", "Auth");

                    var userRole = _authService.GetCurrentUserRoleAsync().Result;
                    if (userRole != "Soporte")
                        return RedirectToAction("Login", "Auth");

                    ViewBag.CurrentUserRole = userRole;
                    return View("~/Views/roles/Soporte/Configuracion.cshtml");
                }

                [HttpGet("NuevaRespuestaPorId/{reclamoId}")]
                public async Task<IActionResult> NuevaRespuestaPorId(Guid reclamoId)
                {
                    if (!_authService.IsAuthenticated())
                        return RedirectToAction("Login", "Auth");

                    var userRole = await _authService.GetCurrentUserRoleAsync();
                    if (userRole != "Soporte")
                        return RedirectToAction("Login", "Auth");

                    var currentUser = await _authService.GetCurrentUserAsync();
                    if (currentUser == null)
                    {
                        TempData["ErrorMessage"] = "No se pudo obtener la información del usuario.";
                        return RedirectToAction("Dashboard");
                    }

                    var token = _authService.GetToken();
                    if (!string.IsNullOrEmpty(token))
                        _apiService.SetAuthToken(token);

                    var reclamo = await _apiService.GetReclamoByIdAsync(reclamoId);
                    if (reclamo == null)
                    {
                        TempData["ErrorMessage"] = "No se encontró el reclamo.";
                        return RedirectToAction("GestionReclamos");
                    }

                    var viewModel = new NuevaRespuestaViewModel
                    {
                        ReclamoId = reclamoId,
                        Reclamo = reclamo,
                        ReclamosDisponibles = new System.Collections.Generic.List<ReclamoDto> { reclamo },
                        Mensaje = string.Empty,
                        UsuarioNombre = currentUser.Nombre
                    };

                    ViewBag.CurrentUserRole = userRole;
                    return View("~/Views/roles/Soporte/NuevaRespuesta.cshtml", viewModel);
                }

                [HttpGet("Dashboard")]
                public async Task<IActionResult> Dashboard()
                {
                    try
                    {
                        if (!_authService.IsAuthenticated())
                            return RedirectToAction("Login", "Auth");

                        var userRole = await _authService.GetCurrentUserRoleAsync();
                        if (userRole != "Soporte")
                            return RedirectToAction("Login", "Auth");

                        var currentUser = await _authService.GetCurrentUserAsync();
                        if (currentUser == null)
                        {
                            TempData["ErrorMessage"] = "No se pudo obtener la información del usuario.";
                            return RedirectToAction("Login", "Auth");
                        }

                        var authToken = _authService.GetToken();
                        if (!string.IsNullOrEmpty(authToken))
                            _apiService.SetAuthToken(authToken);

                        var todosLosReclamos = await _apiService.GetReclamosAsync();

                        var viewModel = new SoporteDashboardViewModel
                        {
                            Usuario = currentUser,
                            TotalReclamos = todosLosReclamos.Count,
                            ReclamosNuevos = todosLosReclamos.Count(r => r.Estado == "Nuevo"),
                            ReclamosEnProceso = todosLosReclamos.Count(r => r.Estado == "EnProceso" || r.Estado == "Respondido"),
                            ReclamosResueltos = todosLosReclamos.Count(r => r.Estado == "Cerrado"),
                            ReclamosPendientes = todosLosReclamos.Count(r => r.Estado == "Nuevo" || r.Estado == "EnProceso"),
                            ReclamosPorPrioridad = todosLosReclamos.GroupBy(r => r.Prioridad).ToDictionary(g => g.Key, g => g.Count()),
                            ReclamosRecientes = todosLosReclamos.OrderByDescending(r => r.FechaCreacion).Take(10).ToList()
                        };

                        ViewBag.CurrentUserRole = userRole;
                        return View("~/Views/roles/Soporte/Dashboard.cshtml", viewModel);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error al cargar el dashboard de soporte");
                        TempData["ErrorMessage"] = "Error al cargar el dashboard.";
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
                        if (userRole != "Soporte")
                            return RedirectToAction("Login", "Auth");

                        var currentUser = await _authService.GetCurrentUserAsync();
                        if (currentUser == null)
                        {
                            TempData["ErrorMessage"] = "No se pudo obtener la información del usuario.";
                            return RedirectToAction("Dashboard");
                        }

                        var token = _authService.GetToken();
                        if (!string.IsNullOrEmpty(token))
                            _apiService.SetAuthToken(token);

                            var todosLosReclamos = await _apiService.GetReclamosAsync();
                            var usuarios = await _apiService.GetUsuariosAsync();

                            var zonaArgentina = TimeZoneInfo.FindSystemTimeZoneById("Argentina Standard Time");
                            var hoy = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, zonaArgentina).Date;
                            var viewModel = new GestionReclamosViewModel
                            {
                                Reclamos = todosLosReclamos,
                                TodosLosReclamos = todosLosReclamos,
                                TotalReclamos = todosLosReclamos.Count,
                                ReclamosNuevos = todosLosReclamos.Where(r => r.Estado == "Nuevo").ToList(),
                                ReclamosEnProceso = todosLosReclamos.Where(r => r.Estado == "EnProceso").ToList(),
                                ReclamosRespondidos = todosLosReclamos.Where(r => r.Estado == "Respondido").ToList(),
                                ReclamosCerrados = todosLosReclamos.Where(r => r.Estado == "Cerrado").ToList(),
                                ReclamosResueltos = todosLosReclamos.Count(r => r.Estado == "Cerrado"),
                                Usuarios = usuarios,
                                ReclamosPorEstado = todosLosReclamos.GroupBy(r => r.Estado).ToDictionary(g => g.Key, g => g.Count()),
                                ReclamosPorPrioridad = todosLosReclamos.GroupBy(r => r.Prioridad).ToDictionary(g => g.Key, g => g.Count()),
                                ReclamosHoy = todosLosReclamos.Count(r => r.FechaCreacion.Date == hoy),
                                ReclamosSemana = todosLosReclamos.Count(r => r.FechaCreacion.Date >= hoy.AddDays(-7)),
                                ReclamosMes = todosLosReclamos.Count(r => r.FechaCreacion.Month == hoy.Month && r.FechaCreacion.Year == hoy.Year),
                                ReclamosPrioridadAlta = todosLosReclamos.Count(r => r.Prioridad == "Alta"),
                                ReclamosPrioridadMedia = todosLosReclamos.Count(r => r.Prioridad == "Media"),
                                ReclamosPrioridadBaja = todosLosReclamos.Count(r => r.Prioridad == "Baja"),
                                PromedioTiempoResolucion = CalcularPromedioResolucion(todosLosReclamos)
                            };

                        ViewBag.CurrentUserRole = userRole;
                        return View("~/Views/roles/Soporte/GestionReclamos.cshtml", viewModel);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error al cargar la gestión de reclamos");
                        TempData["ErrorMessage"] = "Error al cargar los reclamos.";
                        return RedirectToAction("Dashboard");
                    }
                }

                [HttpPost("NuevaRespuesta")]
                public async Task<IActionResult> NuevaRespuesta(CreateRespuestaDto model)
                {
                    try
                    {
                        if (!_authService.IsAuthenticated())
                            return RedirectToAction("Login", "Auth");

                        var userRole = await _authService.GetCurrentUserRoleAsync();
                        if (userRole != "Soporte")
                            return RedirectToAction("Login", "Auth");

                        var currentUser = await _authService.GetCurrentUserAsync();
                        if (currentUser == null)
                        {
                            TempData["ErrorMessage"] = "No se pudo obtener la información del usuario.";
                            return RedirectToAction("Dashboard");
                        }

                        if (!ModelState.IsValid)
                        {
                            var reclamos = await _apiService.GetReclamosAsync();
                            var reclamosAbiertos = reclamos.Where(r => r.Estado != "Cerrado").OrderByDescending(r => r.FechaCreacion).ToList();

                            var viewModel = new NuevaRespuestaViewModel
                            {
                                ReclamoId = model.ReclamoId,
                                ReclamosDisponibles = reclamosAbiertos,
                                Mensaje = model.Mensaje
                            };

                            ViewBag.CurrentUserRole = userRole;
                            return View(viewModel);
                        }

                        model.UsuarioId = currentUser.Id;
                        var token = _authService.GetToken();
                        if (!string.IsNullOrEmpty(token))
                            _apiService.SetAuthToken(token);

                        var resultado = await _apiService.CreateRespuestaAsync(model);

                        if (resultado != null)
                        {
                            // Actualizar estado del reclamo a 'Respondido' automáticamente
                            await _apiService.UpdateReclamoEstadoAsync(model.ReclamoId, "Respondido");
                            TempData["SuccessMessage"] = "Respuesta enviada exitosamente.";
                                // Refrescar estadísticas antes de redirigir
                                await _apiService.GetDashboardStatsAsync();
                            return RedirectToAction("GestionReclamos");
                        }
                        else
                        {
                            TempData["ErrorMessage"] = "Error al enviar la respuesta.";
                            var reclamos = await _apiService.GetReclamosAsync();
                            var reclamosAbiertos = reclamos.Where(r => r.Estado != "Cerrado").OrderByDescending(r => r.FechaCreacion).ToList();

                            var viewModel = new NuevaRespuestaViewModel
                            {
                                ReclamoId = model.ReclamoId,
                                ReclamosDisponibles = reclamosAbiertos,
                                Mensaje = model.Mensaje
                            };

                            ViewBag.CurrentUserRole = userRole;
                            return View(viewModel);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error al crear la respuesta");
                        TempData["ErrorMessage"] = "Error al enviar la respuesta.";
                        return RedirectToAction("Dashboard");
                    }
                }

                [HttpPost("ActualizarEstadoReclamo")]
                public async Task<IActionResult> ActualizarEstadoReclamo(Guid reclamoId, string nuevoEstado)
                {
                    try
                    {
                        var resultado = await _apiService.UpdateReclamoEstadoAsync(reclamoId, nuevoEstado);

                        if (resultado)
                        {
                            // Si el nuevo estado es 'Cerrado', cuenta como resuelto
                            if (nuevoEstado == "Cerrado")
                            {
                                // Aquí podrías agregar lógica extra si necesitas registrar el cierre
                            }
                            return Json(new { success = true, message = "Estado actualizado exitosamente" });
                        }
                        else
                        {
                            return Json(new { success = false, message = "Error al actualizar el estado" });
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error al actualizar el estado del reclamo {ReclamoId}", reclamoId);
                        return Json(new { success = false, message = "Error interno al actualizar el estado" });
                    }
                }

                private double CalcularPromedioResolucion(System.Collections.Generic.List<ReclamoDto> reclamos)
                {
                    var reclamosCerrados = reclamos.Where(r => r.Estado == "Cerrado").ToList();
                    if (!reclamosCerrados.Any())
                        return 0;

                    return reclamosCerrados.Average(r => (DateTime.Now - r.FechaCreacion).TotalDays);
                }

                [HttpGet("DetalleReclamo/{id}")]
                public async Task<IActionResult> DetalleReclamo(Guid id)
                {
                    if (!_authService.IsAuthenticated())
                        return RedirectToAction("Login", "Auth");

                    var userRole = await _authService.GetCurrentUserRoleAsync();
                    if (userRole != "Soporte")
                        return RedirectToAction("Login", "Auth");

                    var token = _authService.GetToken();
                    if (!string.IsNullOrEmpty(token))
                        _apiService.SetAuthToken(token);

                    var reclamo = await _apiService.GetReclamoByIdAsync(id);
                    if (reclamo == null)
                    {
                        TempData["ErrorMessage"] = "No se encontró el reclamo.";
                        return RedirectToAction("CasosUrgentes");
                    }

                    var respuestas = await _apiService.GetRespuestasByReclamoAsync(id);
                    var currentUser = await _authService.GetCurrentUserAsync();

                    var viewModel = new SupportWeb.Models.ViewModels.Soporte.DetalleReclamoViewModel
                    {
                        Reclamo = reclamo,
                        Respuestas = respuestas ?? new System.Collections.Generic.List<SupportWeb.Models.DTOs.RespuestaDto>(),
                        UsuarioNombre = currentUser?.Nombre ?? string.Empty,
                        PuedeEditarEstado = true,
                        PuedeResponder = true
                    };

                    ViewBag.CurrentUserRole = userRole;
                    return View("~/Views/roles/Soporte/DetalleReclamo.cshtml", viewModel);
                }
            }
        }

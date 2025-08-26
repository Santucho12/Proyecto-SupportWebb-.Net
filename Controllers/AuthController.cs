using Microsoft.AspNetCore.Mvc;
using SupportWeb.Models.ViewModels;
using SupportWeb.Services;

namespace SupportWeb.Controllers
{
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Login(string? returnUrl = null)
        {
            _logger.LogError("=== LOGIN GET - Verificando autenticación ===");

            // Si ya está autenticado, redirigir según el rol
            if (_authService.IsAuthenticated())
            {
                _logger.LogError("Usuario ya autenticado - verificando rol para redirección");

                var userRole = await _authService.GetCurrentUserRoleAsync();
                _logger.LogError("Rol del usuario autenticado: {Role}", userRole ?? "No disponible");

                // Debug: verificar el switch
                var normalizedRole = userRole?.ToLower();
                _logger.LogError("Rol normalizado para switch: '{NormalizedRole}'", normalizedRole ?? "null");

                // Evitar advertencia CS1998 si no hay await
                await Task.CompletedTask;

                // Redireccionar según el rol del usuario
                var redirectResult = userRole?.ToLower() switch
                {
                    "usuario" => RedirectToAction("Dashboard", "Usuario"), // Los usuarios van a su dashboard personal
                    "soporte" => RedirectToAction("Index", "Dashboard"), // Personal de soporte va al dashboard
                    "admin" => RedirectToAction("Index", "Dashboard"),   // Administradores van al dashboard
                    _ => RedirectToAction("Index", "Dashboard") // Por defecto, dashboard
                };

                _logger.LogError("Redirigiendo usuario con rol '{Role}' a: {Action}", userRole, redirectResult.GetType().Name);
                return redirectResult;
            }

            _logger.LogError("Usuario NO autenticado - mostrando login");

            var model = new LoginViewModel
            {
                ReturnUrl = returnUrl
            };

            // Verificar si hay mensaje de éxito desde registro
            if (TempData.ContainsKey("SuccessMessage"))
            {
                ViewBag.SuccessMessage = TempData["SuccessMessage"]?.ToString();
                TempData.Remove("SuccessMessage"); // Asegurar que se elimine después de usarlo
            }

            // Pre-rellenar email si viene desde registro
            if (TempData.ContainsKey("UserEmail"))
            {
                model.Email = TempData["UserEmail"]?.ToString();
                TempData.Remove("UserEmail"); // Asegurar que se elimine después de usarlo
            }

            return View(model);
        }        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            // Limpiar cualquier TempData residual de registro
            TempData.Remove("SuccessMessage");
            TempData.Remove("UserEmail");

            _logger.LogError("========================");
            _logger.LogError("LOGIN POST RECIBIDO!");
            _logger.LogError("Email recibido: {Email}", model?.Email ?? "NULL");
            _logger.LogError("Password recibido: {HasPassword}", !string.IsNullOrEmpty(model?.Password) ? "SÍ" : "NO");

            // Evitar advertencia CS1998 si no hay await
            await Task.CompletedTask;
            _logger.LogError("========================");

            // Test básico - simplemente mostrar que llegó la petición
            if (model == null)
            {
                _logger.LogError("MODEL ES NULL!");
                return View(new LoginViewModel { ErrorMessage = "ERROR: Model es null" });
            }

            if (string.IsNullOrEmpty(model.Email))
            {
                _logger.LogError("EMAIL ESTÁ VACÍO!");
                return View(new LoginViewModel { ErrorMessage = "ERROR: Email vacío" });
            }

            if (string.IsNullOrEmpty(model.Password))
            {
                _logger.LogError("PASSWORD ESTÁ VACÍO!");
                return View(new LoginViewModel { ErrorMessage = "ERROR: Password vacío" });
            }

            // Test de conexión al servicio
            try
            {
                _logger.LogError("Intentando login con AuthService...");
                var success = await _authService.LoginAsync(model.Email, model.Password);
                _logger.LogError("Resultado del login: {Success}", success);

                if (success)
                {
                    _logger.LogError("LOGIN EXITOSO - Verificando rol del usuario");

                    // Obtener el rol del usuario después del login exitoso
                    var userRole = await _authService.GetCurrentUserRoleAsync();
                    _logger.LogError("Rol del usuario: {UserRole}", userRole ?? "No disponible");

                    // Obtener el token actual
                    var token = _authService.GetToken();
                    ViewBag.Token = token;

                    // Mostrar la vista para que el script JS lo guarde en localStorage
                    return View(model);
                }
                else
                {
                    _logger.LogError("LOGIN FALLÓ - Credenciales inválidas");
                    model.ErrorMessage = "Credenciales inválidas";
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "EXCEPCIÓN durante el login");
                model.ErrorMessage = "Error del servidor: " + ex.Message;
                return View(model);
            }
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View(new RegisterViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var registerRequest = new Models.DTOs.RegisterRequestDto
                {
                    Nombre = model.Nombre,
                    CorreoElectronico = model.Email,
                    Contrasena = model.Password,
                    ConfirmPassword = model.ConfirmPassword,
                    Rol = model.Rol // Usar el rol seleccionado por el usuario
                };

                var success = await _authService.RegisterAsync(registerRequest);

                if (success)
                {
                    _logger.LogInformation("User {Email} registered successfully", model.Email);

                    // Usar TempData para mostrar el mensaje en la página de login
                    TempData["SuccessMessage"] = "¡Registro exitoso! Ya puedes iniciar sesión con tus credenciales.";
                    TempData["UserEmail"] = model.Email; // Para pre-rellenar el email en login

                    // Redireccionar al login inmediatamente
                    return RedirectToAction("Login");
                }
                else
                {
                    model.ErrorMessage = "No se pudo completar el registro. Es posible que el email ya esté en uso.";
                    ModelState.AddModelError(string.Empty, model.ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration for user {Email}", model.Email);
                model.ErrorMessage = "Ocurrió un error durante el registro. Por favor, intenta más tarde.";
                ModelState.AddModelError(string.Empty, model.ErrorMessage);
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        [HttpGet]
        public IActionResult TestLogin()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Logout()
        {
            _logger.LogInformation("=== LOGOUT GET EJECUTADO ===");
            _authService.Logout();
            return RedirectToAction("Login");
        }

        [HttpPost]
        public IActionResult Logout(string? returnUrl = null)
        {
            _logger.LogInformation("=== LOGOUT POST EJECUTADO ===");
            _authService.Logout();
            TempData["SuccessMessage"] = "Has cerrado sesión correctamente";
            return RedirectToAction("Login");
        }
    }
}

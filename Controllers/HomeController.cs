using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SupportWeb.Models;
using SupportWeb.Services;

namespace SupportWeb.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IAuthService _authService;

    public HomeController(ILogger<HomeController> logger, IAuthService authService)
    {
        _logger = logger;
        _authService = authService;
    }

    public async Task<IActionResult> Index()
    {
        _logger.LogInformation("=== HOME CONTROLLER INDEX ===");

        // Si el usuario no está autenticado, redirigir al login
        if (!_authService.IsAuthenticated())
        {
            _logger.LogInformation("Usuario no autenticado - redirigiendo al login");
            return RedirectToAction("Login", "Auth");
        }

        // Si está autenticado, redirigir según el rol
        var userRole = await _authService.GetCurrentUserRoleAsync();
        _logger.LogInformation("Usuario autenticado con rol: {Role} - redirigiendo", userRole ?? "No disponible");

        return userRole?.ToLower() switch
        {
            "usuario" => RedirectToAction("Index", "Usuario"), // Los usuarios van a su área personal
            "soporte" => RedirectToAction("Index", "Dashboard"), // Personal de soporte va al dashboard
            "admin" => RedirectToAction("Index", "Dashboard"),   // Administradores van al dashboard
            _ => RedirectToAction("Login", "Auth") // Si no tiene rol válido, al login
        };
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}

using SupportWeb.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SupportWeb.Middleware
{
    /// <summary>
    /// Middleware para autorización basada en roles en rutas específicas.
    /// </summary>
    public class RoleAuthorizationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RoleAuthorizationMiddleware> _logger;

        // Definir rutas y roles requeridos
        private readonly Dictionary<string, string[]> _roleRequirements = new()
        {
            { "/usuarios", new[] { "Admin" } },
            { "/dashboard/admin", new[] { "Admin" } },
            { "/reclamos/delete", new[] { "Admin", "Soporte" } },
            { "/reclamos/edit", new[] { "Admin", "Soporte" } }
        };

        public RoleAuthorizationMiddleware(RequestDelegate next, ILogger<RoleAuthorizationMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, IAuthService authService)
        {
            var path = context.Request.Path.Value?.ToLowerInvariant() ?? "";

            // Verificar si la ruta requiere roles específicos
            var requiredRoles = _roleRequirements
                .Where(kvp => path.StartsWith(kvp.Key))
                .SelectMany(kvp => kvp.Value)
                .ToArray();

            if (requiredRoles.Any())
            {
                var userRole = await authService.GetCurrentUserRoleAsync();

                if (string.IsNullOrEmpty(userRole) || !requiredRoles.Contains(userRole))
                {
                    _logger.LogInformation("Access denied for user with role {Role} to path {Path}", userRole, path);

                    // Si es una petición AJAX, devolver 403
                    if (context.Request.Headers.ContainsKey("X-Requested-With") &&
                        context.Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        context.Response.StatusCode = 403;
                        await context.Response.WriteAsync("Forbidden");
                        return;
                    }

                    // Para peticiones normales, redirigir a Access Denied
                    context.Response.Redirect("/auth/accessdenied");
                    return;
                }
            }

            await _next(context);
        }
    }
}

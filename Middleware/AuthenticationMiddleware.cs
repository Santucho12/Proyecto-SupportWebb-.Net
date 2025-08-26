using SupportWeb.Services;

namespace SupportWeb.Middleware
{

    /// <summary>
    /// Middleware para autenticación global de rutas protegidas.
    /// </summary>
    public class AuthenticationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<AuthenticationMiddleware> _logger;

        public AuthenticationMiddleware(RequestDelegate next, ILogger<AuthenticationMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, IAuthService authService)
        {
            var path = context.Request.Path.Value?.ToLowerInvariant() ?? "";
            _logger.LogInformation("[AuthMiddleware] Processing path: {Path}", path);

            // Rutas que no requieren autenticación
            var publicPaths = new[]
            {
                "/",
                "/auth/login",
                "/auth/register",
                "/auth/accessdenied",
                "/auth/testlogin",
                "/css/",
                "/js/",
                "/lib/",
                "/images/",
                "/favicon.ico",
                "/.well-known/"
            };

            // Verificar si es una ruta pública
            var isPublicPath = publicPaths.Any(p => path.StartsWith(p));
            _logger.LogDebug("[AuthMiddleware] Is public path: {IsPublic}", isPublicPath);

            if (isPublicPath)
            {
                _logger.LogDebug("[AuthMiddleware] Allowing public path: {Path}", path);
                await _next(context);
                return;
            }

            // Verificar autenticación
            if (!authService.IsAuthenticated())
            {
                _logger.LogInformation("Unauthorized access attempt to {Path}", path);

                // Si es una petición AJAX, devolver 401
                if (context.Request.Headers.ContainsKey("X-Requested-With") &&
                    context.Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    context.Response.StatusCode = 401;
                    await context.Response.WriteAsync("Unauthorized");
                    return;
                }

                // Para peticiones normales, redirigir al login
                var returnUrl = context.Request.Path + context.Request.QueryString;
                context.Response.Redirect($"/auth/login?returnUrl={Uri.EscapeDataString(returnUrl)}");
                return;
            }

            // Configurar token en ApiService si está disponible
            var token = authService.GetStoredToken();
            if (!string.IsNullOrEmpty(token))
            {
                var apiService = context.RequestServices.GetService<IApiService>();
                apiService?.SetAuthToken(token);
            }

            await _next(context);
        }
    }


    // RoleAuthorizationMiddleware se movió a su propio archivo para mayor claridad

    // Extension methods para registrar los middlewares
    public static class MiddlewareExtensions
    {
        public static IApplicationBuilder UseCustomAuthentication(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<AuthenticationMiddleware>();
        }

        public static IApplicationBuilder UseRoleAuthorization(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RoleAuthorizationMiddleware>();
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SupportWeb.Services;

namespace SupportWeb.Filters
{

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class AuthorizeFilter : Attribute, IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var authService = context.HttpContext.RequestServices.GetService(typeof(IAuthService)) as IAuthService;

            if (authService == null || !authService.IsAuthenticated())
            {
                // Si es una petición AJAX, devolver 401
                if (context.HttpContext.Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    context.Result = new UnauthorizedResult();
                    return;
                }

                // Redirigir al login
                var returnUrl = context.HttpContext.Request.Path + context.HttpContext.Request.QueryString;
                context.Result = new RedirectToActionResult("Login", "Auth", new { returnUrl });
                return;
            }

            await next();
        }
    }

    // RoleAuthorizeFilter se movió a su propio archivo para mayor claridad
}

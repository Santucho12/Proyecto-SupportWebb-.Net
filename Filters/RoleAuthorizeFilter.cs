using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SupportWeb.Services;
using System.Threading.Tasks;
using System.Linq;

namespace SupportWeb.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class RoleAuthorizeFilter : Attribute, IAsyncActionFilter
    {
        private readonly string[] _allowedRoles;

        public RoleAuthorizeFilter(params string[] allowedRoles)
        {
            _allowedRoles = allowedRoles;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var authService = context.HttpContext.RequestServices.GetService(typeof(IAuthService)) as IAuthService;

            if (authService == null || !authService.IsAuthenticated())
            {
                context.Result = new RedirectToActionResult("Login", "Auth", null);
                return;
            }

            var userRole = await authService.GetCurrentUserRoleAsync();

            if (userRole == null || !_allowedRoles.Contains(userRole))
            {
                context.Result = new RedirectToActionResult("AccessDenied", "Auth", null);
                return;
            }

            await next();
        }
    }
}

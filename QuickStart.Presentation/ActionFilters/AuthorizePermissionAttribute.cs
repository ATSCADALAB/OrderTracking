using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Service.Contracts;
using System.Security.Claims;

namespace QuickStart.Presentation.ActionFilters
{
    public class AuthorizePermissionAttribute : IAsyncAuthorizationFilter
    {
        private readonly string _category;
        private readonly string _permission;
        private readonly IServiceManager _serviceManager;

        public AuthorizePermissionAttribute(string category, string permission, IServiceManager serviceManager)
        {
            _category = category;
            _permission = permission;
            _serviceManager = serviceManager;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            // Lấy userId từ Claims
            var userId = context.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            // Kiểm tra quyền
            bool hasPermission = await _serviceManager.AuthorizationService.HasPermission(userId, _category, _permission);

            if (!hasPermission)
            {
                context.Result = new ForbidResult(); // 403 Forbidden
            }
        }
    }

    // Tạo attribute để sử dụng trong controller
    public class AuthorizePermission : TypeFilterAttribute
    {
        public AuthorizePermission(string category, string permission)
            : base(typeof(AuthorizePermissionAttribute))
        {
            Arguments = new object[] { category, permission };
        }
    }
}
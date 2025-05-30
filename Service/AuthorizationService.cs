using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts;
using Entities.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Repository;
using Service.Contracts;

namespace Service
{
    public class AuthorizationService : IAuthorizationServiceLocal
    {
        private readonly UserManager<User> _userManager;
        private readonly IRepositoryManager _context;
        private readonly ILoggerManager _logger;

        // Cấu hình quyền truy cập (có thể load từ database)
        private readonly Dictionary<string, List<string>> _rolePermissions = new()
        {
            { "Admin", new List<string> { "Customers.View", "Customers.Edit", "Customers.Delete" } },
            { "User", new List<string> { "Customers.View" } }
        };

        public AuthorizationService(UserManager<User> userManager, IRepositoryManager context, ILoggerManager logger)
        {
            _userManager = userManager;
            _context = context;
            _logger = logger;
        }

        public async Task<bool> HasPermission(string userId, string resource, string action)
        {
            if (string.IsNullOrEmpty(userId))
            {
                //_logger.LogWarning("User ID is null or empty.");
                return false;
            }

            // Tìm user trong Identity
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                //_logger.LogWarning($"User with ID {userId} not found.");
                return false;
            }

            // Lấy danh sách roles của user
            var roles = await _userManager.GetRolesAsync(user);
            if (!roles.Any())
            {
                //_logger.LogWarning($"No roles found for user {userId}.");
                return false;
            }

            // Lấy danh sách quyền từ database
            var rolePermissions = await _context.RolePermission.GetRolePermissionsAsync(true);
;

            // Kiểm tra xem quyền có tồn tại trong danh sách RolePermissions không
            var permissionExists = rolePermissions.Any(rp =>
                rp.Permission.Name == action && rp.Category.Name == resource);

            if (!permissionExists)
            {
                //_logger.LogWarning($"User {userId} does not have permission for {resource}.{action}");
            }

            return permissionExists;
        }
    }
}

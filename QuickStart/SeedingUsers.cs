using Entities.Identity;
using Microsoft.AspNetCore.Identity;

namespace QuickStart
{
    public static class SeedingUsers
    {
        public static async Task SeedUsers(UserManager<User> userManager)
        {
            if (await userManager.FindByNameAsync("admin") == null)
            {
                var adminUser = new User
                {
                    FirstName = "ATSCADA",
                    LastName = "LAB",
                    UserName = "atlab",
                    Email = "soft@atpro.com.vn",
                };

                await userManager.CreateAsync(adminUser, "atpro1234560");
            }

            if (await userManager.FindByNameAsync("operator") == null)
            {
                var normalUser = new User
                {
                    FirstName = "UserFirstName",
                    LastName = "UserLastName",
                    UserName = "user002",
                    Email = "user002@matech.com",
                };

                await userManager.CreateAsync(normalUser, "operator");
            }
        }
    }
}

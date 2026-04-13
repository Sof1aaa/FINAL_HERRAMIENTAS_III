using Microsoft.AspNetCore.Identity;

namespace HERRAMIENTAS.Data
{
    public static class DbInitializer
    {
        public static async Task SeedUsers(UserManager<IdentityUser> userManager)
        {
            if (userManager.FindByEmailAsync("admin@admin.com").Result == null)
            {
                var user = new IdentityUser
                {
                    UserName = "admin@admin.com",
                    Email = "admin@admin.com"
                };

                await userManager.CreateAsync(user, "Admin123!");
            }
        }
    }
}
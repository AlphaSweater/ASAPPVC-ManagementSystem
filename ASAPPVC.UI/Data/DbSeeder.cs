using ASAPPVC.App.Models;
using ASAPPVC.App.Models.Enums;
using ASAPPVC.App.Services;
using ASAPPVC.App.ViewModels.Auth;
using Microsoft.AspNetCore.Identity;

namespace ASAPPVC.App.Data
{
    public static class DbSeeder
    {
        /// <summary>
        /// Entry point to run all seeders. Creates a scope and runs each table-specific seeder.
        /// </summary>
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var services = scope.ServiceProvider;

            var roleManager = services.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
            var authService = services.GetRequiredService<IAuthService>();

            // Production Seeders
            await SeedRolesAsync(roleManager);

            // Development Seeders
            await SeedDemoUsersAsync(userManager, authService);

            // future seeders: Products, Components, Customers, Orders, etc.
        }

        /// <summary>
        /// Ensures required roles exist.
        /// Seeds roles from the RoleType enum (skips Unassigned).
        /// </summary>
        public static async Task SeedRolesAsync(RoleManager<IdentityRole<Guid>> roleManager)
        {
            if (roleManager == null)
                return;

            // Create roles from enum values
            foreach (var role in Enum.GetValues<RoleType>())
            {
                // Skip placeholder roles that shouldn't be created
                if (role == RoleType.Unassigned)
                    continue;

                var roleName = role.ToString();
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    var identityRole = new IdentityRole<Guid> { Name = roleName };
                    await roleManager.CreateAsync(identityRole);
                }
            }
        }

        /// <summary>
        /// Seeds demo users. Uses IAuthService.RegisterAsync so that both Identity user and profile are created consistently.
        /// </summary>
        public static async Task SeedDemoUsersAsync(UserManager<ApplicationUser> userManager, IAuthService authService)
        {
            if (userManager == null || authService == null)
                return;

            string adminEmail = "admin@asappvc.co.za";
            string adminPassword = "Admin123!";

            var existing = await userManager.FindByEmailAsync(adminEmail);
            if (existing != null)
                return;

            var vm = new RegisterViewModel
            {
                FirstName = "System",
                LastName = "Admin",
                Email = adminEmail,
                Password = adminPassword,
                ConfirmPassword = adminPassword,
                Role = RoleType.Admin
            };

            await authService.RegisterAsync(vm);
        }
    }
}
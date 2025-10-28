using ASAPPVC.UI.Data;
using ASAPPVC.UI.Models;
using ASAPPVC.UI.Models.Mappers;
using ASAPPVC.UI.Repositories;
using ASAPPVC.UI.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ASAPPVC.UI
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // ============================================
            // Configuration
            // ============================================
            var configuration = builder.Configuration;
            var services = builder.Services;

            // ============================================
            // Database
            // ============================================
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlite(configuration.GetConnectionString("DefaultConnection")));

            // ============================================
            // Identity
            // ============================================
            services.AddIdentity<ApplicationUser, IdentityRole<Guid>>(options =>
            {
                // Relaxed password requirements for this project
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 6;
            })
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

            // ============================================
            // Cookie settings
            // ============================================
            services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/Auth/Login";

                // Cookie security & policy
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                options.Cookie.IsEssential = true;
                options.Cookie.HttpOnly = true;
                options.Cookie.SameSite = SameSiteMode.Strict;

                // Session/timeouts
                options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
                options.SlidingExpiration = true;
                options.Cookie.MaxAge = null; // session-based cookie
            });

            // ============================================
            // MVC / Razor Pages
            // ============================================
            services.AddControllersWithViews();

            // Add in-memory caching
            services.AddMemoryCache();

            // ============================================
            // Options configuration
            // ============================================
            services.Configure<ImageServiceOptions>(opt =>
            {
                opt.MaxBytes = 5 * 1024 * 1024;
                opt.MaxWidth = 2048;
                opt.MaxHeight = 2048;
                opt.ThumbWidth = 400;
                opt.ThumbHeight = 400;
                opt.AllowedContentTypes = new[] { "image/jpeg", "image/png", "image/webp" };
                opt.ForceEncodeAs = "image/webp";
            });

            // ============================================
            // Service registrations
            // --------------------------------------------
            // - Singleton
            // - Scoped
            // ============================================
            // Singleton services
            services.AddSingleton<IImageService, ImageService>();

            // Scoped services (business logic)
            services.AddScoped<ICodeGenerationService, CodeGenerationService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IOrderService, OrderService>();
            services.AddScoped<IProductService, ProductService>();
            services.AddScoped<IComponentService, ComponentService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<ICustomerService, CustomerService>();

            // Mapper registrations
            services.AddScoped<IOrderMapper, OrderMapper>();
            services.AddScoped<IOrderProductMapper, OrderProductMapper>();
            services.AddScoped<IProductMapper, ProductMapper>();
            services.AddScoped<IProductComponentMapper, ProductComponentMapper>();
            services.AddScoped<IComponentMapper, ComponentMapper>();

            // Repository registrations
            services.AddScoped<ICodeCountersRepository, CodeCountersRepository>();
            services.AddScoped<IOrderRepository, OrderRepository>();
            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<IComponentRepository, ComponentRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<ICustomerRepository, CustomerRepository>();

            var app = builder.Build();

            // ============================================
            // Seed data
            // ============================================
            await DbSeeder.SeedAsync(app.Services);

            // ============================================
            // Middleware pipeline
            // ============================================
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            // Authentication & Authorization
            app.UseAuthentication(); // Must be before UseAuthorization
            app.UseAuthorization();

            // ============================================
            // Routing
            // ============================================
            // Area routes must come first
            app.MapControllerRoute(
                name: "areas",
                pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

            // Default route for non-area controllers
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Auth}/{action=Index}/{id?}");

            // ============================================
            // Run application
            // ============================================
            app.Run();
        }
    }
}
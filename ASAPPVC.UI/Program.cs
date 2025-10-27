using ASAPPVC.UI.Data;
using ASAPPVC.UI.Models;
using ASAPPVC.UI.Models.Mappers;
using ASAPPVC.UI.Repositories;
using ASAPPVC.UI.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

//creates database
builder.Services.AddDbContext<AppDbContext>(options =>
	options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// ASP.NET Identity (Guid keys)
builder.Services.AddIdentity<ApplicationUser, IdentityRole<Guid>>(options =>
{
	options.Password.RequireDigit = false;
	options.Password.RequireLowercase = false;
	options.Password.RequireUppercase = false;
	options.Password.RequireNonAlphanumeric = false;
	options.Password.RequiredLength = 6;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

//configure cookie settings
builder.Services.ConfigureApplicationCookie(options =>
{
	options.LoginPath = "/Auth/Login";
	options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
	options.Cookie.IsEssential = true;
	options.Cookie.HttpOnly = true;
	options.Cookie.SameSite = SameSiteMode.Strict;
	options.ExpireTimeSpan = TimeSpan.FromMinutes(30); // Session timeout
	options.SlidingExpiration = true;
	options.Cookie.MaxAge = null; // Session-based cookie
});

// Add services to the container.
builder.Services.AddControllersWithViews();

// configure options
builder.Services.Configure<ImageServiceOptions>(opt =>
{
	opt.MaxBytes = 5 * 1024 * 1024;
	opt.MaxWidth = 2048;
	opt.MaxHeight = 2048;
	opt.ThumbWidth = 400;
	opt.ThumbHeight = 400;
	opt.AllowedContentTypes = new[] { "image/jpeg", "image/png", "image/webp" };
	opt.ForceEncodeAs = "image/webp";
});

// registering services
builder.Services.AddSingleton<IImageService, ImageService>();
builder.Services.AddScoped<ICodeGenerationService, CodeGenerationService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IComponentService, ComponentService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IOrderService, OrderService>();

// registering mappers
builder.Services.AddScoped<IComponentMapper, ComponentMapper>();
builder.Services.AddScoped<IProductComponentMapper, ProductComponentMapper>();
builder.Services.AddScoped<IProductMapper, ProductMapper>();
builder.Services.AddScoped<IOrderProductMapper, OrderProductMapper>();
builder.Services.AddScoped<IOrderMapper, OrderMapper>();

//registering repositories
builder.Services.AddScoped<ICodeCountersRepository, CodeCountersRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<IComponentRepository, ComponentRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();

var app = builder.Build();

// Run seeders
await DbSeeder.SeedAsync(app.Services);

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Home/Error");
	// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
	app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
	name: "default",
	pattern: "{controller=Auth}/{action=Login}/{id?}");

app.Run();
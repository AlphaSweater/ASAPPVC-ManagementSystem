using ASAPPVC.App.Models;
using ASAPPVC.App.Models.Enums;
using ASAPPVC.App.Services;
using ASAPPVC.App.ViewModels.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ASAPPVC.App.Data
{
    public static class DbSeeder
    {
        // Expose demo data as reusable static arrays so other parts of the app (or mapping code)
        // can reference the same demo items. Arrays are used per request but can be changed to
        // lists if mutability is required.
        private static readonly ComponentFormVm[] DemoComponents = new ComponentFormVm[]
        {
            new() // 1
			{
                ComponentName   = "uPVC Profile Kit · Door Frame (2400×2100)",
                MaterialType    = Material.PVC,
                ColourOption    = Colour.White,
                UnitOfMeasure   = Unit.Piece,     // full frame kit = 1 piece
				QuantityOnHand  = 12m,
                UnitCost        = 2300.00m,
                LocationCode    = "A-1",
                LocationNote    = "Frame kits shelf",
                ReorderLevel    = 4m,
                IsActive        = true
            },
            new() // 2
			{
                ComponentName   = "uPVC Profile Kit · Window Frame (1200×900)",
                MaterialType    = Material.PVC,
                ColourOption    = Colour.White,
                UnitOfMeasure   = Unit.Piece,
                QuantityOnHand  = 25m,
                UnitCost        = 980.00m,
                LocationCode    = "A-2",
                LocationNote    = "Window kits shelf",
                ReorderLevel    = 8m,
                IsActive        = true
            },
            new() // 3
			{
                ComponentName   = "IGU Double-Glazed Unit · 24mm (1200×900)",
                MaterialType    = Material.Glass,
                ColourOption    = Colour.None,
                UnitOfMeasure   = Unit.Piece,
                QuantityOnHand  = 30m,
                UnitCost        = 1450.00m,
                LocationCode    = "B-1",
                LocationNote    = "Glass rack",
                ReorderLevel    = 10m,
                IsActive        = true
            },
            new() // 4
			{
                ComponentName   = "IGU Double-Glazed Unit · 24mm (2400×2100, 2-Panel Set)",
                MaterialType    = Material.Glass,
                ColourOption    = Colour.None,
                UnitOfMeasure   = Unit.Piece,   // set of 2 panels boxed as one piece
				QuantityOnHand  = 10m,
                UnitCost        = 5200.00m,
                LocationCode    = "B-2",
                LocationNote    = "Oversize glass rack",
                ReorderLevel    = 4m,
                IsActive        = true
            },
            new() // 5
			{
                ComponentName   = "Sliding Door Roller Set · Stainless Tandem",
                MaterialType    = Material.Steel,
                ColourOption    = Colour.Silver,
                UnitOfMeasure   = Unit.Piece,  // set = 1 piece
				QuantityOnHand  = 40m,
                UnitCost        = 480.00m,
                LocationCode    = "C-1",
                LocationNote    = "Hardware bin",
                ReorderLevel    = 12m,
                IsActive        = true
            },
            new() // 6
			{
                ComponentName   = "Multipoint Lockset · Door (uPVC)",
                MaterialType    = Material.Steel,
                ColourOption    = Colour.Silver,
                UnitOfMeasure   = Unit.Piece,
                QuantityOnHand  = 28m,
                UnitCost        = 750.00m,
                LocationCode    = "C-2",
                LocationNote    = "Locks drawer",
                ReorderLevel    = 8m,
                IsActive        = true
            },
            new() // 7
			{
                ComponentName   = "Handle Pair · Door/Window (uPVC)",
                MaterialType    = Material.Aluminium,
                ColourOption    = Colour.White,
                UnitOfMeasure   = Unit.Piece,   // pair boxed as one piece
				QuantityOnHand  = 80m,
                UnitCost        = 260.00m,
                LocationCode    = "D-1",
                LocationNote    = "Handles bin",
                ReorderLevel    = 20m,
                IsActive        = true
            },
            new() // 8
			{
                ComponentName   = "Compression Gasket Kit · 24mm IGU",
                MaterialType    = Material.Rubber,
                ColourOption    = Colour.Black,
                UnitOfMeasure   = Unit.Piece,   // kit for one unit
				QuantityOnHand  = 120m,
                UnitCost        = 180.00m,
                LocationCode    = "D-2",
                LocationNote    = "Seals drawer",
                ReorderLevel    = 40m,
                IsActive        = true
            },
            new() // 9
			{
                ComponentName   = "Friction Stay Hinge Pair · Casement",
                MaterialType    = Material.Steel,
                ColourOption    = Colour.Silver,
                UnitOfMeasure   = Unit.Piece,   // pair boxed as one piece
				QuantityOnHand  = 55m,
                UnitCost        = 320.00m,
                LocationCode    = "E-1",
                LocationNote    = "Hinges shelf",
                ReorderLevel    = 15m,
                IsActive        = true
            },
            new() // 10
			{
                ComponentName   = "Tilt & Turn Hardware Kit · uPVC",
                MaterialType    = Material.Steel,
                ColourOption    = Colour.Silver,
                UnitOfMeasure   = Unit.Piece,   // full kit = one piece
				QuantityOnHand  = 18m,
                UnitCost        = 980.00m,
                LocationCode    = "E-2",
                LocationNote    = "Hardware kits",
                ReorderLevel    = 6m,
                IsActive        = true
            }
        };

        private static readonly ProductFormVm[] DemoProducts = new ProductFormVm[]
        {
            new()
            {
				// 1) uPVC Sliding Patio Door · 2-Panel
				ProductName   = "uPVC Sliding Patio Door · 2-Panel (2400×2100)",
                Description   = "Two-panel uPVC sliding patio door with low-E double glazing, stainless tandem rollers, multi-point locking, and weather-sealed thresholds for coastal durability.",
                Image         = null,
                ExistingImageUrl = null,
                Category      = Category.None,        // set to Doors if you have it
				MaterialType  = Material.PVC,         // uPVC handled as PVC in enum
				ColourOption  = Colour.White,
                SellingPrice  = 12999.00m,
                ReorderLevel  = 3m,
                IsActive      = true,
                SelectedProductComponents = new()             // BOM to be added next
			},
            new()
            {
				// 2) uPVC Sliding Window · Double Glazed
				ProductName   = "uPVC Sliding Window · 1200×900 Double-Glazed",
                Description   = "Compact uPVC horizontal slider with 24 mm IGU double glazing, brush and bulb seals, and anodized track for smooth, quiet operation.",
                Image         = null,
                ExistingImageUrl = null,
                Category      = Category.None,        // set to Windows if you have it
				MaterialType  = Material.PVC,
                ColourOption  = Colour.White,
                SellingPrice  = 4499.00m,
                ReorderLevel  = 5m,
                IsActive      = true,
                SelectedProductComponents = new()
            },
            new()
            {
				// 3) uPVC Casement Window
				ProductName   = "uPVC Casement Window · 900×1200 Double-Glazed",
                Description   = "Outward-opening uPVC casement with friction stays, multi-point locking, low-E double glazing, and integrated drainage channels.",
                Image         = null,
                ExistingImageUrl = null,
                Category      = Category.None,
                MaterialType  = Material.PVC,
                ColourOption  = Colour.White,
                SellingPrice  = 5299.00m,
                ReorderLevel  = 4m,
                IsActive      = true,
                SelectedProductComponents = new()
            },
            new()
            {
				// 4) uPVC Front Door with Sidelight
				ProductName   = "uPVC Front Door · 6-Panel + Double-Glazed Sidelight",
                Description   = "Classic 6-panel uPVC entry door with thermally efficient double-glazed sidelight, steel reinforcement, multipoint lockset, and acoustic seals.",
                Image         = null,
                ExistingImageUrl = null,
                Category      = Category.None,
                MaterialType  = Material.PVC,
                ColourOption  = Colour.White,
                SellingPrice  = 18499.00m,
                ReorderLevel  = 2m,
                IsActive      = true,
                SelectedProductComponents = new()
            },
            new()
            {
				// 5) uPVC Tilt & Turn Window
				ProductName   = "uPVC Tilt & Turn Window · 1000×1200 Double-Glazed",
                Description   = "European-style uPVC tilt-and-turn with 24 mm IGU, micro-vent tilt safety position, perimeter locking, and compression gaskets for superior airtightness.",
                Image         = null,
                ExistingImageUrl = null,
                Category      = Category.None,
                MaterialType  = Material.PVC,
                ColourOption  = Colour.White,
                SellingPrice  = 6699.00m,
                ReorderLevel  = 3m,
                IsActive      = true,
                SelectedProductComponents = new()
            }
        };

        // Map of which demo components (by component name) should be used for each demo product.
        // The array index corresponds to the DemoProducts index.
        private static readonly (string name, decimal qty)[][] DemoProductComponentMap = new (string name, decimal qty)[][]
        {
            //0: uPVC Sliding Patio Door ·2-Panel
            new (string name, decimal qty)[]
            {
                 ("uPVC Profile Kit · Door Frame (2400×2100)",1m),
                 ("IGU Double-Glazed Unit ·24mm (2400×2100,2-Panel Set)",1m),
                 ("Sliding Door Roller Set · Stainless Tandem",1m),
                 ("Multipoint Lockset · Door (uPVC)",1m),
                 ("Handle Pair · Door/Window (uPVC)",1m),
                 ("Compression Gasket Kit ·24mm IGU",1m)
                 },
            //1: uPVC Sliding Window
            new (string name, decimal qty)[]
            {
                 ("uPVC Profile Kit · Window Frame (1200×900)",1m),
                 ("IGU Double-Glazed Unit ·24mm (1200×900)",1m),
				 // smaller or optional rollers for sliding window; set0 to skip
				 ("Sliding Door Roller Set · Stainless Tandem",0m)
                 },
            //2: uPVC Casement Window
            new (string name, decimal qty)[]
            {
                 ("uPVC Profile Kit · Window Frame (1200×900)",1m),
                 ("IGU Double-Glazed Unit ·24mm (1200×900)",1m),
                 ("Friction Stay Hinge Pair · Casement",1m),
                 ("Handle Pair · Door/Window (uPVC)",1m),
                 ("Compression Gasket Kit ·24mm IGU",1m)
                 },
            //3: uPVC Front Door
            new (string name, decimal qty)[]
            {
                 ("uPVC Profile Kit · Door Frame (2400×2100)",1m),
                 ("Multipoint Lockset · Door (uPVC)",1m),
                 ("Handle Pair · Door/Window (uPVC)",1m)
                 },
            //4: uPVC Tilt & Turn Window
            new (string name, decimal qty)[]
            {
                 ("uPVC Profile Kit · Window Frame (1200×900)",1m),
                 ("IGU Double-Glazed Unit ·24mm (1200×900)",1m),
                 ("Tilt & Turn Hardware Kit · uPVC",1m),
                 ("Compression Gasket Kit ·24mm IGU",1m)
            }
        };

        /// <summary>
        /// Entry point to run all seeders. Creates a scope and runs each table-specific seeder.
        /// </summary>
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var services = scope.ServiceProvider;

            // Ensure database is migrated before attempting to use Identity tables
            var db = services.GetRequiredService<AppDbContext>();
            await db.Database.MigrateAsync();

            var roleManager = services.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
            var authService = services.GetRequiredService<IAuthService>();

            var componentService = services.GetRequiredService<IComponentService>();
            var productService = services.GetRequiredService<IProductService>();

            // Production Seeders
            await SeedRolesAsync(roleManager);

            // Development Seeders
            await SeedDemoUsersAsync(userManager, authService);

            await SeedDemoComponentsAsync(componentService);

            // Pass componentService so we can resolve created component IDs and costs
            await SeedDemoProductsAsync(productService, componentService);
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

        public static async Task SeedDemoComponentsAsync(IComponentService componentService)
        {
            if (componentService == null)
                return;

            // Use the service to list components. If there are any, skip seeding.
            var listResult = await componentService.ListAsync();
            if (!listResult.Ok)
            {
                // If listing failed, do not attempt to seed (avoids duplicate or partial state).
                return;
            }

            if (listResult.Value is not null && listResult.Value.Count > 0)
                return;

            // Create components through the service (mapper will generate codes and handle images).
            foreach (var vm in DemoComponents)
            {
                try
                {
                    var createResult = await componentService.CreateAsync(vm);
                }
                catch
                {
                }
            }
        }

        public static async Task SeedDemoProductsAsync(IProductService productService, IComponentService componentService)
        {
            if (productService == null || componentService == null)
                return;

            // If products already exist, skip seeding
            var listResult = await productService.ListAsync();
            if (!listResult.Ok)
                return;

            if (listResult.Value is not null && listResult.Value.Count > 0)
                return;

            // Fetch created components so we can reference their IDs and costs
            var compsResult = await componentService.ListAsync();
            if (!compsResult.Ok || compsResult.Value is null)
                return;

            var availableComponents = compsResult.Value; // assumed to be IEnumerable<ComponentListVm>

            // Prepare demo products - fill details earlier. ProductComponents must be provided for creation.
            var demoProducts = DemoProducts.ToList();

            for (int i = 0; i < demoProducts.Count; i++)
            {
                var productVm = demoProducts[i];
                var mapping = i < DemoProductComponentMap.Length ? DemoProductComponentMap[i] : Array.Empty<(string, decimal)>();

                productVm.SelectedProductComponents = new List<ProductComponentVm>();

                foreach (var (name, qty) in mapping)
                {
                    if (qty <= 0)
                        continue; // skip zero/invalid quantities

                    var comp = availableComponents.FirstOrDefault(c => string.Equals(c.ComponentName, name, StringComparison.OrdinalIgnoreCase));
                    if (comp == null)
                        continue;

                    productVm.SelectedProductComponents.Add(new ProductComponentVm
                    {
                        ProductId = Guid.Empty,
                        ComponentId = comp.Id,
                        ComponentCode = comp.ComponentCode ?? string.Empty,
                        ComponentName = comp.ComponentName ?? string.Empty,
                        UnitOfMeasure = comp.UnitOfMeasure,
                        RequiredQuantity = qty,
                        UnitCost = comp.UnitCost,
                        Remove = false
                    });
                }
            }

            // Only create products that include component lines (ProductComponents.Count >0).
            foreach (var vm in demoProducts)
            {
                if (vm.SelectedProductComponents is null || vm.SelectedProductComponents.Count == 0)
                {
                    // Skip incomplete placeholder entries - user will provide data later.
                    continue;
                }

                try
                {
                    var createResult = await productService.CreateAsync(vm);
                    // ignore failures per-item; logging can be added if needed
                }
                catch
                {
                    // swallow to allow other seed items to proceed
                }
            }
        }
    }
}
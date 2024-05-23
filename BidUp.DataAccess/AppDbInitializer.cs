using BidUp.DataAccess.Entites;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BidUp.DataAccess;

public static class AppDbInitializer
{
    public static async Task SeedRoles(this RoleManager<IdentityRole<int>> roleManager)
    {
        if (!await roleManager.Roles.AnyAsync())
        {
            await roleManager.CreateAsync(new IdentityRole<int>("Admin"));
            await roleManager.CreateAsync(new IdentityRole<int>("User"));
        }
    }

    public static async Task SeedAdminAccounts(this UserManager<User> userManager)
    {
        var email = Environment.GetEnvironmentVariable("BIDUP_ADMIN_EMAIL");
        var password = Environment.GetEnvironmentVariable("BIDUP_ADMIN_PASSWORD");

        if (await userManager.FindByEmailAsync(email!) == null)
        {
            var admin = new User
            {
                FirstName = "BidUp",
                LastName = "Admin",
                UserName = email,
                Email = email
            };
            await userManager.CreateAsync(admin, password!);
            await userManager.AddToRoleAsync(admin, "Admin");
        }
    }
    public static async Task SeedCities(this AppDbContext appDbContext)
    {
        if (!await appDbContext.Cities.AnyAsync())
        {
            // List of Egyptian governorates
            var egyptianGovernorates = new List<City>{
                new() { Name = "Cairo" },
                new() { Name = "Alexandria" },
                new() { Name = "Giza" },
                new() { Name = "Qalyubia" },
                new() { Name = "Port Said" },
                new() { Name = "Suez" },
                new() { Name = "Dakahlia" },
                new() { Name = "Sharkia" },
                new() { Name = "Kafr El Sheikh" },
                new() { Name = "Gharbia" },
                new() { Name = "Monufia" },
                new() { Name = "Beheira" },
                new() { Name = "Ismailia" },
                new() { Name = "Giza" },
                new() { Name = "Beni Suef" },
                new() { Name = "Faiyum" },
                new() { Name = "Minya" },
                new() { Name = "Asyut" },
                new() { Name = "Sohag" },
                new() { Name = "Qena" },
                new() { Name = "Aswan" },
                new() { Name = "Luxor" },
                new() { Name = "Red Sea" },
                new() { Name = "New Valley" },
                new() { Name = "Matrouh" },
                new() { Name = "North Sinai" },
                new() { Name = "South Sinai" }
            };

            await appDbContext.Cities.AddRangeAsync(egyptianGovernorates);

            await appDbContext.SaveChangesAsync();
        }
    }

    public static async Task SeedCategories(this AppDbContext appDbContext)
    {
        if (!await appDbContext.Categories.AnyAsync())
        {
            var categories = new List<Category>()
            {
                new() { Name = "Vehicles", IconUrl = "https://res.cloudinary.com/dhghzuzbo/image/upload/v1716505451/vehicles_kx9kci.svg"},
                new() { Name = "Properties", IconUrl = "https://res.cloudinary.com/dhghzuzbo/image/upload/v1716505710/properties_fnpch3.svg"},
                new() { Name = "Electronics", IconUrl = "https://res.cloudinary.com/dhghzuzbo/image/upload/v1716505451/elctronics_wfiqez.svg"},
                new() { Name = "Mobile Phones & Acessories", IconUrl = "https://res.cloudinary.com/dhghzuzbo/image/upload/v1716505449/mobiles_ltcrzh.svg"},
                new() { Name = "Playstations", IconUrl = "https://res.cloudinary.com/dhghzuzbo/image/upload/v1716505448/playstations_fh3qsc.svg"},
                new() { Name = "Home Appliances", IconUrl = "https://res.cloudinary.com/dhghzuzbo/image/upload/v1716505452/home-appliances_bqgszo.svg"},
                new() { Name = "Clothing", IconUrl = "https://res.cloudinary.com/dhghzuzbo/image/upload/v1716505449/clothing_v1kwgg.svg"},
                new() { Name = "Coins", IconUrl = "https://res.cloudinary.com/dhghzuzbo/image/upload/v1716505452/coins_fl5bln.svg"},
                new() { Name = "Furniture", IconUrl = "https://res.cloudinary.com/dhghzuzbo/image/upload/v1716505448/furniture_xdlf2h.svg"},
                new() { Name = "Cameras", IconUrl = "https://res.cloudinary.com/dhghzuzbo/image/upload/v1716505446/cameras_knarg8.svg"},
                new() { Name = "Books", IconUrl = "https://res.cloudinary.com/dhghzuzbo/image/upload/v1716505448/books_ljmico.svg"},
                new() { Name = "Watches", IconUrl = "https://res.cloudinary.com/dhghzuzbo/image/upload/v1716505449/watches_z5xngg.svg"}
            };

            await appDbContext.Categories.AddRangeAsync(categories);

            await appDbContext.SaveChangesAsync();
        }
    }
}

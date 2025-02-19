using BidX.DataAccess.Entites;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BidX.DataAccess;

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
        var email = Environment.GetEnvironmentVariable("BIDX_ADMIN_EMAIL");
        var password = Environment.GetEnvironmentVariable("BIDX_ADMIN_PASSWORD");

        if (await userManager.FindByEmailAsync(email!) == null)
        {
            var admin = new User
            {
                FirstName = "BidX",
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
                new() { Name = "Vehicles", IconUrl = "https://res.cloudinary.com/dhghzuzbo/image/upload/v1734930580/2fbe62b6-8cca-4f05-a1e8-8ceea4e732c1.svg" },
                new() { Name = "Properties", IconUrl = "https://res.cloudinary.com/dhghzuzbo/image/upload/v1734931018/fee5f941-fda0-493c-b161-5c48f7d8d1d1.svg" },
                new() { Name = "Electronics", IconUrl = "https://res.cloudinary.com/dhghzuzbo/image/upload/v1734930633/2f259d39-0439-426c-95b0-6cf1d6fa2341.svg" },
                new() { Name = "Mobiles", IconUrl = "https://res.cloudinary.com/dhghzuzbo/image/upload/v1734930651/1ac20496-0ce8-4d3f-861c-7e4f9737decf.svg" },
                new() { Name = "Consoles", IconUrl = "https://res.cloudinary.com/dhghzuzbo/image/upload/v1734930680/c3f22c60-e1bf-4828-bfa1-2e4addd2846d.svg" },
                new() { Name = "Jewelries", IconUrl = "https://res.cloudinary.com/dhghzuzbo/image/upload/v1734930757/081d69c9-3176-4f17-a63a-02cd8bf5ede1.svg" },
                new() { Name = "Clothes", IconUrl = "https://res.cloudinary.com/dhghzuzbo/image/upload/v1734930815/e5078ba6-bdeb-4dc5-a5e5-3473445dd38a.svg" },
                new() { Name = "Colins", IconUrl = "https://res.cloudinary.com/dhghzuzbo/image/upload/v1734930841/6c9594a6-ae2f-4712-84d9-d7760f3cb500.svg" },
                new() { Name = "Furniture", IconUrl = "https://res.cloudinary.com/dhghzuzbo/image/upload/v1734930858/612b58cf-03ce-480f-bef8-2219fbd9d9ff.svg" },
                new() { Name = "Cameras", IconUrl = "https://res.cloudinary.com/dhghzuzbo/image/upload/v1734930869/410365ed-4497-45be-8706-ec801fc148be.svg" },
                new() { Name = "Books", IconUrl = "https://res.cloudinary.com/dhghzuzbo/image/upload/v1734930891/2aa85469-6aff-4fec-8713-7072d48ab314.svg" },
                new() { Name = "Watches", IconUrl = "https://res.cloudinary.com/dhghzuzbo/image/upload/v1734930902/2567b9d7-33c9-4cf2-95a3-da7f6364d43a.svg" }
            };
            await appDbContext.Categories.AddRangeAsync(categories);

            await appDbContext.SaveChangesAsync();
        }
    }
}

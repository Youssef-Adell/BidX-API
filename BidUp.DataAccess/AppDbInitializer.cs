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
}

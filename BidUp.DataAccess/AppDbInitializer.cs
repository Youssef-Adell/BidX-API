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
        var userName = Environment.GetEnvironmentVariable("BIDUP_ADMIN_NAME");
        var email = Environment.GetEnvironmentVariable("BIDUP_ADMIN_EMAIL");
        var password = Environment.GetEnvironmentVariable("BIDUP_ADMIN_PASSWORD");

        if (await userManager.FindByEmailAsync(email!) == null)
        {
            var admin = new User { UserName = userName, Email = email };
            await userManager.CreateAsync(admin, password!);
            await userManager.AddToRoleAsync(admin, "Admin");
        }
    }
}

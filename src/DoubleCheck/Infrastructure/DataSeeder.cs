using DoubleCheck.Data;
using DoubleCheck.Entities;
using DoubleCheck.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DoubleCheck.Infrastructure;

/// <summary>Applies migrations and seeds roles, an admin user, and starter categories on startup.</summary>
public static class DataSeeder
{
    private static readonly string[] StarterCategories = { "Tech", "Food", "Health", "Finance", "Legal" };

    public static async Task SeedAsync(IServiceProvider services, IConfiguration config)
    {
        var db = services.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync();

        var roleManager = services.GetRequiredService<RoleManager<ApplicationRole>>();
        foreach (var role in Roles.All)
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new ApplicationRole(role));

        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var adminEmail = config["Seed:AdminEmail"] ?? "admin@doublecheck.local";
        if (await userManager.FindByEmailAsync(adminEmail) is null)
        {
            var admin = new ApplicationUser
            {
                UserName = adminEmail, Email = adminEmail, EmailConfirmed = true,
                DisplayName = config["Seed:AdminDisplayName"] ?? "System Admin", Balance = 0m
            };
            var result = await userManager.CreateAsync(admin, config["Seed:AdminPassword"] ?? "Admin#12345");
            if (result.Succeeded)
                await userManager.AddToRoleAsync(admin, Roles.Admin);
        }

        foreach (var name in StarterCategories)
            if (!await db.Categories.AnyAsync(c => c.Name == name))
                db.Categories.Add(new Category { Name = name });
        await db.SaveChangesAsync();
    }
}

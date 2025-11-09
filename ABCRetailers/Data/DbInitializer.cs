using Microsoft.AspNetCore.Identity;
using ABCRetailers.Data; // Make sure this namespace matches your project

public static class DbInitializer
{
    // Define your constants for the roles
    public const string AdminRole = "Admin";
    public const string CustomerRole = "Customer";

    public static async Task Initialize(IServiceProvider serviceProvider)
    {
        // Get the RoleManager and UserManager services
        using var scope = serviceProvider.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

        // 1. Create Roles if they don't exist
        await CreateRole(roleManager, AdminRole);
        await CreateRole(roleManager, CustomerRole);

        // 2. Create the Admin User
        var adminEmail = "admin@abcrent.com"; // You can change this
        var adminPassword = "AdminPassword123!"; // CHANGE THIS to a strong password

        // Check if admin user already exists
        if (await userManager.FindByEmailAsync(adminEmail) == null)
        {
            var adminUser = new IdentityUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true // Bypasses email confirmation
            };

            // Create the user and set the password
            var result = await userManager.CreateAsync(adminUser, adminPassword);

            // Assign the Admin role
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, AdminRole);
            }
        }
    }

    private static async Task CreateRole(RoleManager<IdentityRole> roleManager, string roleName)
    {
        if (!await roleManager.RoleExistsAsync(roleName))
        {
            await roleManager.CreateAsync(new IdentityRole(roleName));
        }
    }
}
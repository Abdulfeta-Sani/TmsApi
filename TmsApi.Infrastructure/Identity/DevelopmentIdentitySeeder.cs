using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TmsApi.Infrastructure.Identity;

namespace TmsApi.Api.Identity;

public static class DevelopmentIdentitySeeder
{
    public static async Task SeedAsync(
        IServiceProvider services,
        IConfiguration configuration)
    {
        var userManager =
            services.GetRequiredService<UserManager<TmsUser>>();

        var roleManager =
            services.GetRequiredService<RoleManager<IdentityRole>>();

        var email =
            configuration["SeedAdmin:Email"];

        var password =
            configuration["SeedAdmin:Password"];

        var firstName =
            configuration["SeedAdmin:FirstName"] ?? "System";

        var lastName =
            configuration["SeedAdmin:LastName"] ?? "Administrator";

        var role =
            configuration["SeedAdmin:Role"] ?? "Admin";

        if (string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(password))
        {
            return;
        }

        if (!await roleManager.RoleExistsAsync(role))
        {
            var roleResult =
                await roleManager.CreateAsync(
                    new IdentityRole(role));

            if (!roleResult.Succeeded)
            {
                throw new InvalidOperationException(
                    "The configured seed role could not be created.");
            }
        }

        var user =
            await userManager.FindByEmailAsync(email);

        if (user is null)
        {
            user = new TmsUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                FirstName = firstName,
                LastName = lastName
            };

            var userResult =
                await userManager.CreateAsync(user, password);

            if (!userResult.Succeeded)
            {
                var errors = string.Join(
                    "; ",
                    userResult.Errors.Select(error => error.Description));

                throw new InvalidOperationException(
                    $"The configured seed admin could not be created: {errors}");
            }
        }

        if (!await userManager.IsInRoleAsync(user, role))
        {
            var roleResult =
                await userManager.AddToRoleAsync(user, role);

            if (!roleResult.Succeeded)
            {
                throw new InvalidOperationException(
                    "The configured seed admin could not be assigned its role.");
            }
        }
    }
}
using ATRIUM.Domain.Constants;
using ATRIUM.Domain.Models;
using ATRIUM.Infrastructure.Context;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ATRIUM.Infrastructure.SeedData;

public static class SeedData
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<Usuario>>();
        var configuration = serviceProvider.GetRequiredService<IConfiguration>();
        var context = serviceProvider.GetRequiredService<AtriumDbContext>();

        foreach (var roleName in AppRoles.All)
        {
            await EnsureRoleExistsAsync(roleManager, roleName);
        }

        var adminEmail = configuration["AdminSeed:Email"]?.Trim();
        var adminPassword = configuration["AdminSeed:Password"];

        await AssignStudentRoleToLegacyUsersAsync(userManager, context, adminEmail);

        if (string.IsNullOrWhiteSpace(adminEmail) || string.IsNullOrWhiteSpace(adminPassword))
            return;

        var admin = await userManager.FindByEmailAsync(adminEmail);
        if (admin is null)
        {
            admin = new Usuario
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true,
                Nombre = configuration["AdminSeed:Nombre"]?.Trim() ?? "Administrador",
                Apellido = configuration["AdminSeed:Apellido"]?.Trim() ?? "ATRIUM",
                Direccion = configuration["AdminSeed:Direccion"]?.Trim() ?? "Cajamarca",
                Telefono = configuration["AdminSeed:Telefono"]?.Trim() ?? string.Empty,
                FechaDeNacimiento = new DateTime(1995, 1, 1)
            };

            var createResult = await userManager.CreateAsync(admin, adminPassword);
            EnsureSucceeded(createResult, "crear el usuario administrador");
        }

        if (!await userManager.IsInRoleAsync(admin, AppRoles.Administrator))
        {
            var roleResult = await userManager.AddToRoleAsync(admin, AppRoles.Administrator);
            EnsureSucceeded(roleResult, "asignar el rol Administrador");
        }

        if (await userManager.IsInRoleAsync(admin, AppRoles.Student))
        {
            var removeStudentRoleResult = await userManager.RemoveFromRoleAsync(admin, AppRoles.Student);
            EnsureSucceeded(removeStudentRoleResult, "retirar el rol Estudiante del administrador");
        }
    }

    private static async Task EnsureRoleExistsAsync(
        RoleManager<IdentityRole> roleManager,
        string roleName)
    {
        if (await roleManager.RoleExistsAsync(roleName))
            return;

        var result = await roleManager.CreateAsync(new IdentityRole(roleName));
        EnsureSucceeded(result, $"crear el rol {roleName}");
    }

    private static async Task AssignStudentRoleToLegacyUsersAsync(
        UserManager<Usuario> userManager,
        AtriumDbContext context,
        string? adminEmail)
    {
        var rolelessUsers = await context.Users
            .AsNoTracking()
            .Where(user => !context.UserRoles.Any(userRole => userRole.UserId == user.Id))
            .Where(user => string.IsNullOrWhiteSpace(adminEmail) || user.Email != adminEmail)
            .ToListAsync();

        foreach (var existingUser in rolelessUsers)
        {
            var roleResult = await userManager.AddToRoleAsync(existingUser, AppRoles.Student);
            EnsureSucceeded(roleResult, $"asignar el rol Estudiante a {existingUser.Email ?? existingUser.Id}");
        }
    }

    private static void EnsureSucceeded(IdentityResult result, string operation)
    {
        if (result.Succeeded)
            return;

        var details = string.Join(", ", result.Errors.Select(error => error.Description));
        throw new InvalidOperationException($"No se pudo {operation}: {details}");
    }
}

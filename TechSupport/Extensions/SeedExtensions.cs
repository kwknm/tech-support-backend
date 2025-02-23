using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TechSupport.Database;
using TechSupport.Database.Entities;

namespace TechSupport.Extensions;

public static class SeedExtensions
{
    public static async Task SeedDatabase(this IApplicationBuilder app)
    {
        using var serviceScope = app.ApplicationServices.CreateScope();
        await using var context = serviceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        var roleManager = serviceScope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        if (await roleManager.Roles.FirstOrDefaultAsync(x => x.Name == RolesEnum.Support) is null)
            await roleManager.CreateAsync(new IdentityRole(RolesEnum.Support));

        if ((await context.IssueTypes.ToListAsync()).Count == 0)
        {
            await context.IssueTypes.AddAsync(new IssueType { Name = "Проблемы со входом" });
            await context.IssueTypes.AddAsync(new IssueType { Name = "Проблемы с оплатой" });
            await context.IssueTypes.AddAsync(new IssueType { Name = "Неккоректное отображение сайта" });
            await context.IssueTypes.AddAsync(new IssueType { Name = "Проблема с доступом к сайту" });
        }

        await context.SaveChangesAsync();
    }
}
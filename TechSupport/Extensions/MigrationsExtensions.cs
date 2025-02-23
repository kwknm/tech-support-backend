using Microsoft.EntityFrameworkCore;
using TechSupport.Database;

namespace TechSupport.Extensions;

public static class MigrationsExtensions
{
    public static void ApplyMigrations(this IApplicationBuilder app)
    {
        using var serviceScope = app.ApplicationServices.CreateScope();
        using var context = serviceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        context.Database.Migrate();
    }
}
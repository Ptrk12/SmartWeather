using Core.Enums;
using Microsoft.AspNetCore.Identity;

namespace SmartWeather.Extensions
{
    public static class DbSeeder
    {
        public static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
        {
            if (!await roleManager.RoleExistsAsync(GroupRole.Admin.ToString()))
                await roleManager.CreateAsync(new IdentityRole(GroupRole.Admin.ToString()));

            if (!await roleManager.RoleExistsAsync(GroupRole.Member.ToString()))
                await roleManager.CreateAsync(new IdentityRole(GroupRole.Member.ToString()));
        }
    }
}

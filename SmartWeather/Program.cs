using Interfaces.Managers;
using Interfaces.Repositories;
using Managers;
using Managers.auth;
using Managers.validation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Repositories;
using Repositories.SqlContext;
using SmartWeather.Extensions;
using SmartWeather.filters;
using SmartWeather.Filters;
using SmartWeather.middlewares;

namespace SmartWeather
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddHttpContextAccessor();

            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("GroupMember", policy =>
                    policy.Requirements.Add(new RoleRequirement("Member", "Admin")));

                options.AddPolicy("GroupAdmin", policy =>
                    policy.Requirements.Add(new RoleRequirement("Admin")));
            });

            builder.Services.AddControllers(options =>
            {
                options.Filters.Add<ValidateModelFilter>();
            })
                .ConfigureApiBehaviorOptions(options =>
            {
                options.SuppressModelStateInvalidFilter = true;
            });


            builder.Services.AddScoped<AuthManager>();
            builder.Services.AddSingleton<ValidationManager>();
            builder.Services.AddScoped<IAuthorizationHandler, RoleHandler>();

            builder.Services.AddScoped<IUserManager, UserManager>();
            builder.Services.AddScoped<IGroupManager, GroupManager>();
            builder.Services.AddScoped<IGroupRepository, GroupRepository>();


            builder.Services.AddDbContext<SqlDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("Sql")));

            builder.Services.AddIdentityAndAuthentication(builder.Configuration);
            builder.Services.AddSwaggerServices();

            builder.Services.AddTransient<ErrorHandlerMiddleware>();

            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                await DbSeeder.SeedRolesAsync(roleManager);
            }

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseMiddleware<ErrorHandlerMiddleware>();

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
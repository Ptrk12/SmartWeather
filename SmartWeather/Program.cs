using Serilog;
using SmartWeather.extensions;
using SmartWeather.Extensions;
using SmartWeather.middlewares;


namespace SmartWeather
{
    public class Program
    {
        private const string LocalhostCorsPolicy = "LocalhostClientPolicy";
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(builder.Configuration)
                .CreateLogger();

            builder.Host.UseSerilog();
            try
            {
                builder.Services.AddCors(options =>
                {
                    options.AddPolicy(name: LocalhostCorsPolicy,
                        policy =>
                        {
                            policy.WithOrigins("http://localhost:3000")
                                .AllowAnyHeader()
                                .AllowAnyMethod()
                                .AllowCredentials();
                        });
                });

                builder.Services.AddHttpContextAccessor();
                builder.Services.AddAiChatServices(builder.Configuration);            
                builder.Services.AddIdentityAndAuthentication(builder.Configuration);
                builder.Services.AddSwaggerServices();
                builder.Services.AddDistributedMemoryCache();

                builder.Services.AddBusinessServices();
                builder.Services.AddInfrastructure(builder.Configuration);

                builder.Host.UseDefaultServiceProvider((context, options) =>
                {
                    options.ValidateScopes = true; 
                    options.ValidateOnBuild = true;
                });

                var app = builder.Build();

                await app.SeedDatabaseAsync();

                if (app.Environment.IsDevelopment())
                {
                    app.UseSwagger();
                    app.UseSwaggerUI();
                }

                app.UseMiddleware<ErrorHandlerMiddleware>();
                app.UseHttpsRedirection();
                app.UseRouting();
                app.UseCors(LocalhostCorsPolicy);
                app.UseAuthentication();
                app.UseAuthorization();
                app.MapControllers();

                try
                {
                    Log.Information("Starting an application");
                    app.Run();
                }
                catch (Exception ex)
                {
                    Log.Fatal(ex, "Fatal error during running an application");
                }
                finally
                {
                    Log.CloseAndFlush();
                }
            }
            catch (Exception ex)
            {
                Log.Fatal($"Error: {ex.Message}");
            }
        }
    }
}
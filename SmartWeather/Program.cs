using Google.Apis.Auth.OAuth2;
using Google.Cloud.Firestore;
using Interfaces.Managers;
using Interfaces.Repositories;
using Interfaces.Repositories.firebase;
using Managers;
using Managers.auth;
using Managers.validation;
using Managers.workers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MQTTnet;
using Repositories;
using Repositories.firebase;
using Repositories.SqlContext;
using Serilog;
using SmartWeather.Extensions;
using SmartWeather.filters;
using SmartWeather.Filters;
using SmartWeather.middlewares;
using System.Text;
using System.Text.Json;

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

                builder.Services.AddAuthorization(options =>
                {
                    options.AddPolicy("AllRoles", policy =>
                        policy.Requirements.Add(new RoleRequirement("Member", "Admin")));

                    options.AddPolicy("Admin", policy =>
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
                builder.Services.AddSingleton<EfExceptionLoggingInterceptor>();

                builder.Services.AddScoped(typeof(IGenericCrudRepository<>), typeof(GenericCrudRepository<>));
                builder.Services.AddScoped<IUserManager, UserManager>();
                builder.Services.AddScoped<IGroupManager, GroupManager>();
                builder.Services.AddScoped<IGroupRepository, GroupRepository>();
                builder.Services.AddScoped<IDeviceManager, DeviceManager>();
                builder.Services.AddScoped<IDeviceRepository, DeviceRepository>();
                builder.Services.AddScoped<ISensorMetricRepository, SensorMetricRepository>();
                builder.Services.AddScoped<ISensorMetricManager, SensorMetricManager>();
                builder.Services.AddScoped<IAlertRepository, AlertRepository>();
                builder.Services.AddScoped<IAlertManager, AlertManager>();
                builder.Services.AddScoped<IAlertLogsRepository, AlertLogsRepository>();
                builder.Services.AddScoped<IAlertLogsManager, AlertLogsManager>();
                builder.Services.AddScoped<IImageManager, ImageManager>();
                builder.Services.AddScoped<IDeviceMonitorManager, DeviceMonitorManager>();

                builder.Services.AddDbContext<SqlDbContext>((serviceProvider, options) =>
                {
                    var loggingInterceptor = serviceProvider.GetRequiredService<EfExceptionLoggingInterceptor>();

                    options.UseSqlServer(builder.Configuration.GetConnectionString("Sql"))
                           .AddInterceptors(loggingInterceptor);
                });

                builder.Services.AddSingleton(sp =>
                {
                    var firebaseConfigSection = builder.Configuration.GetSection("Firebase");
                    var credsDict = firebaseConfigSection.Get<Dictionary<string, string>>();
                    string credentialsJson = JsonSerializer.Serialize(credsDict);
                    using var stream = new MemoryStream(Encoding.UTF8.GetBytes(credentialsJson));

                    ServiceAccountCredential serviceAccountCredential = ServiceAccountCredential.FromServiceAccountData(stream);

                    var credential = GoogleCredential.FromServiceAccountCredential(serviceAccountCredential);
                    string projectId = credsDict["project_id"];

                    var dbBuilder = new FirestoreDbBuilder
                    {
                        ProjectId = projectId,
                        Credential = credential
                    };

                    return dbBuilder.Build();
                });
                builder.Services.AddScoped<IFirebaseRepository,FirebaseRepository>();

                builder.Services.AddIdentityAndAuthentication(builder.Configuration);
                builder.Services.AddSwaggerServices();
                builder.Services.AddDistributedMemoryCache();

                builder.Services.AddHostedService<MqttBackgroundWorker>();
                builder.Services.AddSingleton<IMqttClient>(new MqttClientFactory().CreateMqttClient());


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
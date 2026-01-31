using Google.Apis.Auth.OAuth2;
using Google.Cloud.Firestore;
using Interfaces.Managers;
using Interfaces.Repositories;
using Interfaces.Repositories.firebase;
using Managers;
using Managers.workers;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MQTTnet;
using Repositories;
using Repositories.firebase;
using Repositories.SqlContext;
using SmartWeather.Filters;
using SmartWeather.middlewares;
using System.Text;
using System.Text.Json;

namespace SmartWeather.Extensions
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddBusinessServices(this IServiceCollection services)
        {
            services.AddScoped(typeof(IGenericCrudRepository<>), typeof(GenericCrudRepository<>));
            services.AddScoped<IGroupRepository, GroupRepository>();
            services.AddScoped<IDeviceRepository, DeviceRepository>();
            services.AddScoped<ISensorMetricRepository, SensorMetricRepository>();
            services.AddScoped<IAlertRepository, AlertRepository>();
            services.AddScoped<IAlertLogsRepository, AlertLogsRepository>();

            services.AddScoped<IUserManager, UserManager>();
            services.AddScoped<IGroupManager, GroupManager>();
            services.AddScoped<IDeviceManager, DeviceManager>();
            services.AddScoped<ISensorMetricManager, SensorMetricManager>();
            services.AddScoped<IAlertManager, AlertManager>();
            services.AddScoped<IAlertLogsManager, AlertLogsManager>();
            services.AddScoped<IImageManager, ImageManager>();
            services.AddScoped<IDeviceMonitorManager, DeviceMonitorManager>();

            return services;
        }
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services
                .AddSqlPersistence(configuration)
                .AddFirebasePersistence(configuration)
                .AddExternalHttpClients(configuration)
                .AddMqttInfrastructure()
                .AddApiConfiguration()
                .AddCoreMiddleware();

            return services;
        }

        public static async Task SeedDatabaseAsync(this WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            await DbSeeder.SeedRolesAsync(roleManager);
        }

        private static IServiceCollection AddSqlPersistence(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<EfExceptionLoggingInterceptor>();

            services.AddDbContext<SqlDbContext>((sp, options) =>
            {
                var interceptor = sp.GetRequiredService<EfExceptionLoggingInterceptor>();
                options.UseSqlServer(configuration.GetConnectionString("Sql"))
                       .AddInterceptors(interceptor);
            });

            return services;
        }

        private static IServiceCollection AddFirebasePersistence(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IFirebaseRepository, FirebaseRepository>();

            services.AddSingleton(sp =>
            {
                var firebaseConfigSection = configuration.GetSection("Firebase");
                var credsDict = firebaseConfigSection.Get<Dictionary<string, string>>();

                if (credsDict == null || !credsDict.ContainsKey("project_id"))
                {
                    throw new InvalidOperationException("Firebase configuration is missing or invalid in appsettings.json");
                }

                string projectId = credsDict["project_id"];
                string credentialsJson = JsonSerializer.Serialize(credsDict);

                using var stream = new MemoryStream(Encoding.UTF8.GetBytes(credentialsJson));
                var serviceAccountCredential = ServiceAccountCredential.FromServiceAccountData(stream);
                var credential = GoogleCredential.FromServiceAccountCredential(serviceAccountCredential);

                return new FirestoreDbBuilder
                {
                    ProjectId = projectId,
                    Credential = credential
                }.Build();
            });

            return services;
        }

        private static IServiceCollection AddExternalHttpClients(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddHttpClient("PredictWeatherClient", client =>
            {
                var url = configuration["Functions:PredictionFunctionUrl"];
                if (string.IsNullOrEmpty(url)) throw new InvalidOperationException("PredictionFunctionUrl is missing configuration.");

                client.BaseAddress = new Uri(url);
                client.Timeout = TimeSpan.FromSeconds(60);
            });

            return services;
        }

        private static IServiceCollection AddMqttInfrastructure(this IServiceCollection services)
        {
            services.AddHostedService<MqttBackgroundWorker>();
            services.AddSingleton<IMqttClient>(sp => new MqttClientFactory().CreateMqttClient());
            return services;
        }

        private static IServiceCollection AddApiConfiguration(this IServiceCollection services)
        {
            services.AddControllers(options =>
            {
                options.Filters.Add<ValidateModelFilter>();
            })
            .ConfigureApiBehaviorOptions(options =>
            {
                options.SuppressModelStateInvalidFilter = true;
            });

            return services;
        }

        private static IServiceCollection AddCoreMiddleware(this IServiceCollection services)
        {
            services.AddTransient<ErrorHandlerMiddleware>();
            return services;
        }
    }
}
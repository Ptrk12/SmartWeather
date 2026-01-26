using AiChat.Plugins;
using AiChat.Services;
using Microsoft.SemanticKernel;

namespace SmartWeather.extensions
{
    public static class AiChatExtensions
    {
        public static IServiceCollection AddAiChatServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<IntentGuard>();
            services.AddScoped<AiChatPlugin>();
            services.AddScoped<QdrantSeeder>();
            services.AddScoped<KnoweldgeBasePlugin>();


            services.AddScoped<Kernel>(sp =>
            {
                var kernelBuilder = Kernel.CreateBuilder();

                var apiKey = configuration["Gemini:ApiKey"];
                var model = configuration["Gemini:Model"];

                if (string.IsNullOrEmpty(apiKey))
                {
                    throw new ArgumentNullException("Gemini API Key is not configured");
                }
                if(string.IsNullOrEmpty(model))
                {
                    throw new ArgumentNullException("Gemini Model is not configured");
                }

                kernelBuilder.AddGoogleAIGeminiChatCompletion(model, apiKey);

                var aiChatPlugin = sp.GetRequiredService<AiChatPlugin>();
                var knoweldgeBasePlugin = sp.GetRequiredService<KnoweldgeBasePlugin>();

                kernelBuilder.Plugins.AddFromObject(aiChatPlugin, nameof(AiChatPlugin));         
                kernelBuilder.Plugins.AddFromObject(knoweldgeBasePlugin, nameof(KnoweldgeBasePlugin));

                return kernelBuilder.Build();
            });

            return services;
        }
    }
}

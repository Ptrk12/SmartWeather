using System.Net;
using System.Text.Json;

namespace SmartWeather.middlewares
{
    public class ErrorHandlerMiddleware : IMiddleware
    {
        private readonly ILogger<ErrorHandlerMiddleware> _log;

        public ErrorHandlerMiddleware(ILogger<ErrorHandlerMiddleware> log)
        {
            _log = log;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            try
            {
                await next(context);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Exception occured: {message}", ex.Message);

                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                context.Response.ContentType = "application/json";

                var result = JsonSerializer.Serialize(new
                {
                    error = ex.Message,
                    details = ex.InnerException?.Message,
                    type = ex.GetType().Name
                });

                await context.Response.WriteAsync(result);
            }
        }
    }
}

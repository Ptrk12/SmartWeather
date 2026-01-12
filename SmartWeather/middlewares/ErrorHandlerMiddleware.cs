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
                _log.LogError(ex, "Exception occured: message: {message} type: {type}", ex.Message, ex.GetType().Name);

                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                context.Response.ContentType = "application/json";

                var result = JsonSerializer.Serialize(new
                {
                    error = "Unexpected error occured"
                });

                await context.Response.WriteAsync(result);
            }
        }
    }
}

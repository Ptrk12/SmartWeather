using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using System.Data.Common;

public class EfExceptionLoggingInterceptor : DbCommandInterceptor
{
    private readonly ILogger<EfExceptionLoggingInterceptor> _logger;

    public EfExceptionLoggingInterceptor(ILogger<EfExceptionLoggingInterceptor> logger)
    {
        _logger = logger;
    }
    public override Task CommandFailedAsync(DbCommand command, CommandErrorEventData eventData, CancellationToken cancellationToken = default)
    {
        _logger.LogError(eventData.Exception,
            "ERROR COMMAND EF Core (Async). ERROR: {Error}",
            eventData.Exception);

        return base.CommandFailedAsync(command, eventData, cancellationToken);
    }

    public override void CommandFailed(DbCommand command, CommandErrorEventData eventData)
    {
        _logger.LogError(eventData.Exception,
            "ERROR COMMAND EF Core (Sync). ERROR: {Error}",
            eventData.Exception);

        base.CommandFailed(command, eventData);
    }
}
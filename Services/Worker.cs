namespace cache_me_if_you_can.Services;

public class Worker(ILogger<Worker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            const string testString = " SET user:1 data";
            var testMultispan = ComandanteParser.Parse(testString.AsSpan());
            var testCheck = testMultispan.Value.IsEmpty;
            
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            }

            await Task.Delay(1000, stoppingToken);
        }
    }
}
namespace cache_me_if_you_can.Services;

public class TcpServerWorker(
    IServiceScopeFactory scopeFactory
    ) : BackgroundService
{
    private readonly SemaphoreSlim _bottleNeck = new(1, 1);
    
    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            await _bottleNeck.WaitAsync(ct);
            try
            {
                using var scope = scopeFactory.CreateScope();
                using var scopedTcpServer = scope.ServiceProvider.GetRequiredService<TerribleConnectionProblemServer>();
                await scopedTcpServer.StartAsync(ct);
            }
            finally
            {
                _bottleNeck.Release();
            }
        }
    }
}
namespace cache_me_if_you_can.Extensions;

public static class ProgramServicesExtensions
{
    private const string ConfigFileSectionName = "Global";

    public static void AddMaintainServices(this HostApplicationBuilder builder)
    {
        builder.Services.Configure<ConfigFile>(
            builder.Configuration.GetSection(ConfigFileSectionName));
        
        builder.Services.AddHostedService<TcpServerWorker>();
    }
    
    public static void AddProcessServices(this HostApplicationBuilder builder)
    {
        builder.Services.AddScoped<TerribleConnectionProblemServer>();
    }
}
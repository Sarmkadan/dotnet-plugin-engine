#nullable enable
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using PluginEngine.Configuration;

namespace PluginEngine.Examples;

/// <summary>
/// IntegrationExample shows how to register the engine in an ASP.NET Core application.
/// </summary>
public static class IntegrationExample
{
    public static void ConfigureServices(IServiceCollection services)
    {
        // Register in ASP.NET Core DI
        services.AddPluginEngine(options =>
        {
            options.PluginDirectory = "./plugins";
        });
    }

    public static void Configure(IApplicationBuilder app)
    {
        // Initialize at startup if needed, or inject into controllers/services
    }
}

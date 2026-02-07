using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MassifCentral.Lib;
using MassifCentral.Lib.Utilities;

// Build host with dependency injection configuration
var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        // Register all MassifCentral application services
        services.AddMassifCentralServices();
    })
    .Build();

try
{
    // Resolve logger from the service provider
    var logger = host.Services.GetRequiredService<ILogger>();
    
    logger.LogInfo($"Starting {Constants.ApplicationName} v{Constants.Version}");

    // Application logic goes here
    logger.LogInfo("Application initialized successfully");
    Console.WriteLine("Welcome to MassifCentral!");

    logger.LogInfo("Application completed successfully");
}
catch (Exception ex)
{
    // If logger couldn't be resolved, use static access as fallback
    var logger = host.Services.GetService<ILogger>();
    if (logger != null)
    {
        logger.LogError("An error occurred during application execution", ex);
    }
    else
    {
        Console.WriteLine($"CRITICAL ERROR: {ex.Message}");
        Console.WriteLine($"StackTrace: {ex.StackTrace}");
    }
    
    Environment.Exit(1);
}

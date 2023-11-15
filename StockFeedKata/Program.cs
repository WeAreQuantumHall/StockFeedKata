using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StockFeedKata.Managers;
using StockFeedKata.Orchestrator;

var builder = Host
    .CreateDefaultBuilder()
    .ConfigureServices(services =>
    {
        services.AddSingleton<StockFileOrchestrator>();
        services.AddSingleton<StockManager>();
    });

var host = builder.Build();

var orchestrator = host.Services.GetRequiredService<StockFileOrchestrator>();
await orchestrator.RunAsync("./Resources/stock.txt");
    

 
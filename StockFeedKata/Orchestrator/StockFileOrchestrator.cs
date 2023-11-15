using Microsoft.Extensions.Logging;
using StockFeedKata.Managers;
using StockFeedKata.Statics;

namespace StockFeedKata.Orchestrator;

public class StockFileOrchestrator
{
    private readonly ILogger<StockFileOrchestrator> _logger;
    private readonly StockManager _stockManager;

    public StockFileOrchestrator(StockManager stockManager, ILogger<StockFileOrchestrator> logger)
    {
        _stockManager = stockManager;
        _logger = logger;
    }

    public async Task RunAsync(string filepath)
    {
        var stockFileTransactions = await StockFileLoader.Load(filepath);
        foreach(var transaction in stockFileTransactions) _stockManager.ProcessTransaction(transaction);
        OutputStock();
    }

    private void OutputStock()
    {
        _logger.LogInformation("{Output}", string.Join(Environment.NewLine, _stockManager.Stock));
    }
}
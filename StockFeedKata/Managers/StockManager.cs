using Microsoft.Extensions.Logging;
using StockFeedKata.Statics;

namespace StockFeedKata.Managers;

public class StockManager
{
    private readonly ILogger<StockManager> _logger;
    private readonly IDictionary<string, int> _stock = new SortedDictionary<string, int>();
    private readonly IDictionary<string, IEnumerable<string>> _orders = new Dictionary<string, IEnumerable<string>>();
    public IReadOnlyDictionary<string, int> Stock => _stock.AsReadOnly();

    public StockManager(ILogger<StockManager> logger)
    {
        _logger = logger;
    }

    public void ProcessTransaction(string transactionString)
    {
        var splitTransactions = transactionString
            .Split(' ', StringSplitOptions.RemoveEmptyEntries);

        var commandType = splitTransactions.First();
        var transactions = splitTransactions.Skip(1).ToList();

        var wasSuccessful = commandType switch
        {
            TransactionType.SetStockTransaction => SetStock(transactions),
            TransactionType.AddStockTransaction => AddStock(transactions),
            TransactionType.OrderTransaction => FulfilOrder(transactions),
            TransactionType.CancelTransaction => CancelOrder(transactions.First()),
            TransactionType.AssembleTransaction => AssembleStock(transactions),
            _ => throw new SystemException($"Not a valid transactionString type: {splitTransactions.First()}")
        };

        if (!wasSuccessful)
        {
            _logger.LogInformation("Could not process transaction: {Transaction}", transactionString);
        }
    }

    private bool SetStock(IReadOnlyList<string> transactions)
    {
        Dictionary<string, int> groupedTransactions = new();

        for (var i = 0; i < transactions.Count; i += 2)
        {
            if (groupedTransactions.TryGetValue(transactions[i], out var existingValue))
            {
                groupedTransactions[transactions[i]] = existingValue + int.Parse(transactions[i + 1]);
            }
            else
            {
                groupedTransactions[transactions[i]] = int.Parse(transactions[i + 1]);
            }
        }

        foreach (var groupedTransaction in groupedTransactions)
        {
            _stock[groupedTransaction.Key] = groupedTransaction.Value;
            if (groupedTransaction.Value < 0)
            {
                _logger.LogError("Stock level negative: {Sku}", groupedTransaction.Key);
            }
        }

        return true;
    }

    private bool AddStock(IReadOnlyList<string> transactions)
    {
        for (var i = 0; i < transactions.Count; i += 2)
        {
            var amountToSet = _stock.TryGetValue(transactions[i], out var value)
                ? value + int.Parse(transactions[i + 1])
                : int.Parse(transactions[i + 1]);

            _stock[transactions[i]] = amountToSet;
        }

        return true;
    }

    private bool FulfilOrder(IReadOnlyList<string> transactions)
    {
        var orderRef = transactions[0];

        if (!RemoveStock(transactions.Skip(1).ToList()))
        {
            return false;
        }
        
        _orders[orderRef] = transactions.Skip(1);
        return true;
    }

    private bool CancelOrder(string orderRef)
    {
        if(!_orders.TryGetValue(orderRef, out var transactions))
        {
            _logger.LogInformation("Order not found: {OrderRef}", orderRef);
            return false;
        }

        AddStock(transactions.ToList());
        _orders.Remove(orderRef);
        
        return true;
    }

    private bool AssembleStock(IReadOnlyList<string> transactions)
    {
        var sku = transactions[0];
        
        RemoveStock(transactions.Skip(1).ToList());
        AddStock(new List<string>
        {
            sku,
            "1"
        });

        return true;
    }
    
    private bool RemoveStock(IReadOnlyList<string> transactions)
    {
        for (var i = 0; i < transactions.Count - 1; i += 2)
        {
            var amountToSet = _stock.TryGetValue(transactions[i], out var value)
                ? value - int.Parse(transactions[i + 1])
                : 0 - int.Parse(transactions[i + 1]);

            _stock[transactions[i]] = amountToSet;
            
            if (amountToSet < 0)
            {
                _logger.LogError("Stock level negative: {Sku}", transactions[i]);
            }
        }

        return true;
    }
}
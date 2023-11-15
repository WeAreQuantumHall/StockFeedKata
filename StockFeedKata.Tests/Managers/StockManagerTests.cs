using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using StockFeedKata.Managers;
using StockFeedKata.Statics;
using StockFeedKata.Tests.Mocks;

namespace StockFeedKata.Tests.Managers;

public class StockManagerTests
{
    [Fact]
    public void Creating_a_stock_manager_should_create_an_empty_order_dictionary()
    {
        var stockManager = new StockManager(NullLogger<StockManager>.Instance);
        
        Assert.Multiple(
            () => Assert.NotNull(stockManager.Stock),
            () => Assert.Empty(stockManager.Stock));
    }

    [Fact]
    public void ProcessTransaction_when_one_Set_transaction_is_sent_should_set_the_stock_level()
    {
        const string sku = "XX-111";
        const int amount = 10;
        
        var expectedStock = new SortedDictionary<string, int>
        {
            {sku, amount}
        };
        
        var transactionString = $"{TransactionType.SetStockTransaction} {sku} {amount}"; 
        
        var stockManager = new StockManager(NullLogger<StockManager>.Instance);
        stockManager.ProcessTransaction(transactionString);

        Assert.Equal(expectedStock, stockManager.Stock);
    }
    
    [Fact]
    public void ProcessTransaction_when_multiple_Set_transactions_are_sent_should_set_the_correct_stock_level()
    {
        const string sku = "XX-111";
        const string sku2 = "XX-222";
        const int amount = 10;
        const int amount2 = 20;

        var expectedStock = new SortedDictionary<string, int>
        {
            {sku, amount}, 
            {sku2, amount2}
        };
        
        var transactionString = $"{TransactionType.SetStockTransaction} {sku} {amount} {sku2} {amount2}"; 
        
        var stockManager = new StockManager(NullLogger<StockManager>.Instance);
        stockManager.ProcessTransaction(transactionString);

        Assert.Equal(expectedStock, stockManager.Stock);
    }

    [Fact]
    public void ProcessTransaction_when_Set_and_sku_is_not_present_should_set_the_stock_to_amount()
    {
        const string sku = "XX-111";
        const int amount = 10;
        
        var expectedStock = new SortedDictionary<string, int>
        {
            {sku, amount}
        };
        
        var transactionString = $"{TransactionType.SetStockTransaction} {sku} {amount}"; 
        
        var stockManager = new StockManager(NullLogger<StockManager>.Instance);
        stockManager.ProcessTransaction(transactionString);

        Assert.Equal(expectedStock, stockManager.Stock);
    }
    
    [Fact]
    public void ProcessTransaction_when_Set_and_sku_is_already_present_should_set_the_stock_to_amount()
    {
        const string sku = "XX-111";
        const int initialAmountToSet = 10;
        const int secondAmountToSet = 5;
       
        var expectedStock = new SortedDictionary<string, int>
        {
            {sku, secondAmountToSet}
        };
        var initialSetTransaction = $"{TransactionType.SetStockTransaction} {sku} {initialAmountToSet}";
        var transactionString = $"{TransactionType.SetStockTransaction} {sku} {secondAmountToSet}"; 
        
        var stockManager = new StockManager(NullLogger<StockManager>.Instance);
        stockManager.ProcessTransaction(initialSetTransaction);
        stockManager.ProcessTransaction(transactionString);

        Assert.Equal(expectedStock, stockManager.Stock);
    }
    
    [Fact]
    public void ProcessTransaction_when_Set_and_stock_level_negative_should_output_error_with_sku()
    {
        const string sku = "XX-111";
        const int negativeAmount = -10;
        var mockLogger = Substitute.For<MockLogger<StockManager>>();
        var initialSetTransaction = $"{TransactionType.SetStockTransaction} {sku} {negativeAmount}";

        var stockManager = new StockManager(mockLogger);
        stockManager.ProcessTransaction(initialSetTransaction);

        mockLogger
            .Received()
            .Log(LogLevel.Error, "Stock level negative: XX-111");
    }
    
    [Fact]
    public void ProcessTransaction_when_Set_and_sku_appears_twice_should_add_correct_amount()
    {
        const string sku = "XX-111";
        const int firstAmount = 10;
        const int secondAmount = 5;
        const int expectedAmount = firstAmount + secondAmount;
        
        var expectedStock = new SortedDictionary<string, int>
        {
            {sku, expectedAmount}
        };
        var transactionString = $"{TransactionType.SetStockTransaction} {sku} {firstAmount} {sku} {secondAmount}"; 
        
        var stockManager = new StockManager(NullLogger<StockManager>.Instance);
        stockManager.ProcessTransaction(transactionString);
        
        Assert.Equal(expectedStock, stockManager.Stock);
    }

    [Fact]
    public void ProcessTransaction_when_Add_and_sku_is_not_present_should_set_the_correct_amount()
    {
        const string sku = "X-111";
        const int expectedAmount = 10;
        SortedDictionary<string, int> expectedStock = new()
        {
            {sku, expectedAmount}
        };
        var transactionString = $"{TransactionType.AddStockTransaction} {sku} {expectedAmount}";
        
        var stockManager = new StockManager(NullLogger<StockManager>.Instance);
        stockManager.ProcessTransaction(transactionString);
        
        Assert.Equal(expectedStock, stockManager.Stock);
    }
    
    [Fact]
    public void ProcessTransaction_when_Add_and_sku_is_already_present_should_set_the_correct_amount()
    {
        const string sku = "X-111";
        const int firstAmountToAdd = 10;
        const int secondAmountToAdd = 20;
        const int expectedAmount = firstAmountToAdd + secondAmountToAdd;
        
        SortedDictionary<string, int> expectedStock = new()
        {
            {sku, expectedAmount}
        };
        var initialSetTransaction = $"{TransactionType.AddStockTransaction} {sku} {firstAmountToAdd}"; 
        var transactionString = $"{TransactionType.AddStockTransaction} {sku} {secondAmountToAdd}";
        
        var stockManager = new StockManager(NullLogger<StockManager>.Instance);
        stockManager.ProcessTransaction(initialSetTransaction);
        stockManager.ProcessTransaction(transactionString);
        
        Assert.Equal(expectedStock, stockManager.Stock);
    }
    
    [Fact]
    public void ProcessTransaction_when_Add_and_sku_appears_twice_should_set_the_correct_amount()
    {
        const string sku = "X-111";
        const int firstAmountToAdd = 10;
        const int secondAmountToAdd = 20;
        const int expectedAmount = firstAmountToAdd + secondAmountToAdd;
        
        SortedDictionary<string, int> expectedStock = new()
        {
            {sku, expectedAmount}
        };
        var initialSetTransaction = $"{TransactionType.AddStockTransaction} {sku} {firstAmountToAdd}"; 
        var transactionString = $"{TransactionType.AddStockTransaction} {sku} {secondAmountToAdd}";
        
        var stockManager = new StockManager(NullLogger<StockManager>.Instance);
        stockManager.ProcessTransaction(initialSetTransaction);
        stockManager.ProcessTransaction(transactionString);
        
        Assert.Equal(expectedStock, stockManager.Stock);
    }
    
    
    [Fact]
    public void ProcessTransaction_when_Order_and_sku_is_not_present_should_set_the_correct_negative_amount()
    {
        const string orderRef = "OO-111";
        const string sku = "XX-111";
        const int amountToRemove = 10;
        const int expectedAmount = 0 - amountToRemove;
        
        SortedDictionary<string, int> expectedStock = new()
        {
            {sku, expectedAmount}
        };
        var transactionString = $"{TransactionType.OrderTransaction} {orderRef} {sku} {amountToRemove}"; 
        
        var stockManager = new StockManager(NullLogger<StockManager>.Instance);
        stockManager.ProcessTransaction(transactionString);
        
        Assert.Equal(expectedStock, stockManager.Stock);
    }

    [Fact]
    public void ProcessTransaction_when_Order_and_sku_is_present_should_subtract_the_correct_amount_from_the_stock()
    {
        const string sku = "XX-111";
        const string orderRef = "OO-111";
        const int initialAmount = 20;
        const int amountToRemove = 5;
        const int expectedAmount = initialAmount - amountToRemove;
        
        var expectedStock = new SortedDictionary<string, int>
        {
            {sku, expectedAmount}
        };
        var initialSetTransaction = $"{TransactionType.SetStockTransaction} {sku} {initialAmount}";
        var transactionString = $"{TransactionType.OrderTransaction} {orderRef} {sku} {amountToRemove}"; 
        
        var stockManager = new StockManager(NullLogger<StockManager>.Instance);
        stockManager.ProcessTransaction(initialSetTransaction);
        stockManager.ProcessTransaction(transactionString);
        
        Assert.Equal(expectedStock, stockManager.Stock);
    }

    [Fact]
    public void ProcessTransaction_when_Order_and_sku_appears_twice_should_add_correct_amount()
    {
        const string sku = "XX-111";
        const string orderRef = "OO-111";
        const int initialAmount = 20;
        const int firstAmountToRemove = 10;
        const int secondAmountToRemove = 5;
        const int expectedAmount = initialAmount - firstAmountToRemove - secondAmountToRemove;
        
        var expectedStock = new SortedDictionary<string, int>
        {
            {sku, expectedAmount}
        };
        var initialSetTransaction = $"{TransactionType.SetStockTransaction} {sku} {initialAmount}";
        var transactionString = 
            $"{TransactionType.OrderTransaction} {orderRef} {sku} {firstAmountToRemove} {sku} {secondAmountToRemove}"; 
        
        var stockManager = new StockManager(NullLogger<StockManager>.Instance);
        stockManager.ProcessTransaction(initialSetTransaction);
        stockManager.ProcessTransaction(transactionString);
        
        Assert.Equal(expectedStock, stockManager.Stock);
    }

    [Fact]
    public void ProcessTransaction_when_Order_and_stock_level_negative_should_output_error_with_sku()
    {
        const string sku = "XX-111";
        const string orderRef = "OO-111";
        const int initialAmount = 10;
        const int amountToRemove = 20;
        var mockLogger = Substitute.For<MockLogger<StockManager>>();
        var initialSetTransaction = $"{TransactionType.SetStockTransaction} {sku} {initialAmount}";
        var transactionString = $"{TransactionType.OrderTransaction} {orderRef} {sku} {amountToRemove}"; 
        
        var stockManager = new StockManager(mockLogger);
        stockManager.ProcessTransaction(initialSetTransaction);
        stockManager.ProcessTransaction(transactionString);

        mockLogger
            .Received()
            .Log(LogLevel.Error, "Stock level negative: XX-111");
    }

    
    [Fact]
    public void ProcessTransaction_when_Cancelled_and_order_id_is_present_should_reverse_the_order()
    {
        const string orderRef = "00-111";
        const string sku1 = "XX-111";
        const string sku2 = "XX-222";
        const int amount1 = 10;
        const int amount2 = 5;
        const int initialAmount1 = 10;
        const int initialAmount2 = 20;
        
        var expectedStock = new SortedDictionary<string, int>
        {
            {sku1, initialAmount1},
            {sku2, initialAmount2}
        };
        
        var initialSetTransaction = 
            $"{TransactionType.SetStockTransaction} {sku1} {initialAmount1} {sku2} {initialAmount2}";
        var orderTransaction =
            $"{TransactionType.OrderTransaction} {orderRef} {sku1} {amount1} {sku2} {amount2}";
        const string transactionString = $"{TransactionType.CancelTransaction} {orderRef}";
        
        var stockManager = new StockManager(NullLogger<StockManager>.Instance);
        stockManager.ProcessTransaction(initialSetTransaction);
        stockManager.ProcessTransaction(orderTransaction);
        stockManager.ProcessTransaction(transactionString);
        
        Assert.Equal(expectedStock, stockManager.Stock);
    }

    [Fact]
    public void ProcessTransaction_when_Assemble_should_set_correct_stock_levels()
    {
        const string assembleSku = "00-111";
        const string sku1 = "XX-111";
        const string sku2 = "XX-222";
        const int initialAmountSku1 = 20;
        const int initialAmountSku2 = 20;
        const int assembleSku1 = 15;
        const int assembleSku2 = 1;
        const int expectedAmountAssembleSku = 1;
        const int expectedAmountSku1 = 5;
        const int expectedAmountSku2 = 19;
        
        var expectedStock = new SortedDictionary<string, int>
        {
            {sku1, expectedAmountSku1},
            {sku2, expectedAmountSku2},
            {assembleSku, expectedAmountAssembleSku}
        };
        
        var initialSetTransaction = 
            $"{TransactionType.SetStockTransaction} {sku1} {initialAmountSku1} {sku2} {initialAmountSku2}";
        var assembleTransaction =
            $"{TransactionType.AssembleTransaction} {assembleSku} {sku1} {assembleSku1} {sku2} {assembleSku2}";
        
        var stockManager = new StockManager(NullLogger<StockManager>.Instance);
        stockManager.ProcessTransaction(initialSetTransaction);
        stockManager.ProcessTransaction(assembleTransaction);
        
        Assert.Equal(expectedStock, stockManager.Stock);
    }

    [Fact]
    public void ProcessTransaction_when_Assemble_and_stock_level_negative_should_output_error_with_sku()
    {
        const string assembleSku = "00-111";
        const string sku = "XX-111";
        const int initialAmountSku = 20;
        const int assembleSku1 = 25;
        var mockLogger = Substitute.For<MockLogger<StockManager>>();
        var initialSetTransaction = 
            $"{TransactionType.SetStockTransaction} {sku} {initialAmountSku}";
        var assembleTransaction =
            $"{TransactionType.AssembleTransaction} {assembleSku} {sku} {assembleSku1}";
        
        var stockManager = new StockManager(mockLogger);
        stockManager.ProcessTransaction(initialSetTransaction);
        stockManager.ProcessTransaction(assembleTransaction);
        
        mockLogger
            .Received()
            .Log(LogLevel.Error, "Stock level negative: XX-111");
    }
}
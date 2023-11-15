namespace StockFeedKata.Statics;

public static class StockFileLoader
{
    public static async Task<IEnumerable<string>> Load(string filename)
    {
        if (!File.Exists(filename))
        {
            throw new FileNotFoundException("file not found", filename);
        }

        var stockFileTransactions = await File.ReadAllLinesAsync(filename);

        if (!stockFileTransactions.Any())
        {
            throw new IOException($"Empty file: {filename}");
        }

        return stockFileTransactions;
    }
}
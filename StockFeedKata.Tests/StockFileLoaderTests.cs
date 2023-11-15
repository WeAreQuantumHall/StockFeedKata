using StockFeedKata.Statics;

namespace StockFeedKata.Tests;

public class StockFileLoaderTests
{
    
    [Fact]
    public async Task Trying_to_load_a_file_which_does_not_exist_should_throw_exception()
    {
        const string inputFile = "not/a/file.text";

        var exception = await Assert.ThrowsAsync<FileNotFoundException>(() => StockFileLoader.Load(inputFile));
        Assert.Equal(inputFile, exception.FileName);
    }

    [Fact]
    public async Task Trying_to_load_a_file_which_is_empty_should_throw_exception()
    {
        var inputFile = $"{Environment.CurrentDirectory}/Resources/emptyStockFile.txt";

        await Assert.ThrowsAsync<IOException>(() => StockFileLoader.Load(inputFile));
    }

    [Fact]
    public async Task Trying_to_load_a_valid_file_should_return_the_contents_of_that_file()
    {
        const string fileContent = """
                                   set-stock AB-6 100 CD-3 200 DE-1 200 FG-4 300
                                   add-stock AB-6 20
                                   add-stock CD-3 10 DE-1 10
                                   order ON-123 AB-6 2
                                   order ON-234 CD-3 1 DE-1 1
                                   add-stock CD-3  5
                                   """;
        
        var expectedResult = fileContent.Split(Environment.NewLine);
        var inputFile = $"{Environment.CurrentDirectory}/Resources/validStockFile.txt";

        var result = await StockFileLoader.Load(inputFile);
        
        Assert.Equal(expectedResult, result);
    }
}
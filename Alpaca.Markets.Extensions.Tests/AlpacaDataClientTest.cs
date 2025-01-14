namespace Alpaca.Markets.Extensions.Tests;

[Collection("MockEnvironment")]
public sealed partial class AlpacaDataClientTest
{
    private static readonly Interval<DateTime> _timeInterval = getTimeInterval();

    private readonly MockClientsFactoryFixture _mockClientsFactory;

    private static readonly String[] _symbols = { Stock, Other };

    private const Decimal Volume = 1_000M;

    private const String Stock = "AAPL";

    private const String Other = "MSFT";

    private const Decimal Price = 100M;

    private const Decimal Size = 10M;

    private const Int32 Pages = 5;

    public AlpacaDataClientTest(
        MockClientsFactoryFixture mockClientsFactory) =>
        _mockClientsFactory = mockClientsFactory;

    private static void addPaginatedResponses<TConfiguration, TClient>(
        MockClient<TConfiguration, TClient> mock,
        Action<MockClient<TConfiguration, TClient>, String?> singleResponseFactory)
        where TConfiguration : AlpacaClientConfigurationBase
        where TClient : class, IDisposable
    {
        for (var index = 1; index <= Pages; ++index)
        {
            singleResponseFactory(mock, index != Pages
                ? Guid.NewGuid().ToString("D") : null);
        }
    }

    private static async ValueTask<Int32> validateList<TItem>(
        IAsyncEnumerable<TItem> trades) =>
        await trades.CountAsync();

    private static async ValueTask<Int32> validateListOfLists<TItem>(
        IAsyncEnumerable<IReadOnlyList<TItem>> pages) =>
        await pages.SumAsync(_ => _.Count);

    private static async Task<Int32> validateDictionaryOfLists<TItem>(
        IReadOnlyDictionary<String, IAsyncEnumerable<TItem>> dictionary) =>
        (await Task.WhenAll(dictionary.Values.Select(_ => validateList(_).AsTask()))).Sum();

    private static async ValueTask<Int32> validateListOfDictionariesOfLists<TItem>(
        IAsyncEnumerable<IReadOnlyDictionary<String, IReadOnlyList<TItem>>> pages) =>
        await pages.SumAsync(_ => _.Values.Sum(__ => __.Count));

    private static Interval<DateTime> getTimeInterval()
    {
        var today = DateTime.Today;
        var yesterday = today.AddDays(-1);
        return new Interval<DateTime>(yesterday, today);
    }
}

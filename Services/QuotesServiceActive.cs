using Models.Interfaces;
using Models.DTO;

namespace Services;

public class QuotesServiceActive : IQuotesService
{
    private readonly IDataSourceActive _active;
    private readonly QuotesServiceWapi _wapi;
    private readonly QuotesServiceLocal _local;

    public QuotesServiceActive(IDataSourceActive active, QuotesServiceWapi wapi, QuotesServiceLocal local)
    {
        _active = active;
        _wapi = wapi;
        _local = local;
    }

    private IQuotesService Current => _active.ActiveDataSource == DataSource.WebApi
        ? _wapi
        : _local;

    public Task<IQuote> DeleteQuoteAsync(Guid id)
        => Current.DeleteQuoteAsync(id);

    public Task<QuoteCuDto> ReadQuoteCuDtoAsync(Guid id)
        => Current.ReadQuoteCuDtoAsync(id);

    public Task<IQuote> CreateQuoteAsync(QuoteCuDto item)
        => Current.CreateQuoteAsync(item);

    public Task<IQuote> UpdateQuoteAsync(Guid id, QuoteCuDto item)
        => Current.UpdateQuoteAsync(id, item);
}

using Models.Interfaces;
using Models.DTO;
using Services.InMemory;

namespace Services;

public class QuotesServiceLocal : IQuotesService
{
    private readonly InMemoryFriendsStore _store;

    public QuotesServiceLocal(InMemoryFriendsStore store)
    {
        _store = store;
    }

    public Task<IQuote> DeleteQuoteAsync(Guid id)
        => Task.FromResult(_store.DeleteQuote(id));

    public Task<QuoteCuDto> ReadQuoteCuDtoAsync(Guid id)
        => Task.FromResult(_store.ReadQuoteCuDto(id));

    public Task<IQuote> CreateQuoteAsync(QuoteCuDto item)
        => Task.FromResult(_store.CreateQuote(item));

    public Task<IQuote> UpdateQuoteAsync(Guid id, QuoteCuDto item)
        => Task.FromResult(_store.UpdateQuote(id, item));
}

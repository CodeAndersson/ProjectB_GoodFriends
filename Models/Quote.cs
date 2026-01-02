using Models.Interfaces;

namespace Models;

public class Quote : IQuote
{
    public Guid QuoteId { get; set; }
    public string QuoteText { get; set; }
    public string Author { get; set; }

    public List<IFriend> Friends { get; set; } = new();
}

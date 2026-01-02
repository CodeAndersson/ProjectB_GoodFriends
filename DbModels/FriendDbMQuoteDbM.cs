using System.ComponentModel.DataAnnotations.Schema;

namespace DbModels;

[Table("FriendDbMQuoteDbM", Schema = "supusr")]
public class FriendDbMQuoteDbM
{
    public Guid FriendsDbMFriendId { get; set; }
    public Guid QuotesDbMQuoteId { get; set; }

    public FriendDbM FriendsDbM { get; set; }
    public QuoteDbM QuotesDbM { get; set; }
}

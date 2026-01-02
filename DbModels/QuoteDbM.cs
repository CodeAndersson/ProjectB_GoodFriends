using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace DbModels;

[Table("Quotes", Schema = "supusr")]
public class QuoteDbM
{
    [Key]
    public Guid QuoteId { get; set; }

    public string QuoteText { get; set; }

    public string Author { get; set; }

    public bool Seeded { get; set; }

    [JsonIgnore]
    public virtual List<FriendDbM> FriendsDbM { get; set; } = new();
}

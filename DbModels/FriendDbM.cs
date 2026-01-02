using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace DbModels;

[Table("Friends", Schema = "supusr")]
public class FriendDbM
{
    [Key]
    public Guid FriendId { get; set; }

    [Required]
    public string FirstName { get; set; }

    public string LastName { get; set; }

    public string Email { get; set; }

    public DateTime? Birthday { get; set; }

    public Guid? AddressId { get; set; }

    public bool Seeded { get; set; }

    [JsonIgnore]
    public virtual AddressDbM AddressDbM { get; set; }

    [JsonIgnore]
    public virtual List<PetDbM> PetsDbM { get; set; } = new();

    [JsonIgnore]
    public virtual List<QuoteDbM> QuotesDbM { get; set; } = new();
}

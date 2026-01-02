using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace DbModels;

[Table("Addresses", Schema = "supusr")]
public class AddressDbM
{
    [Key]
    public Guid AddressId { get; set; }

    [Required]
    public string StreetAddress { get; set; }

    public int ZipCode { get; set; }

    [Required]
    public string City { get; set; }

    [Required]
    public string Country { get; set; }

    public bool Seeded { get; set; }

    [JsonIgnore]
    public virtual List<FriendDbM> FriendsDbM { get; set; } = new();
}

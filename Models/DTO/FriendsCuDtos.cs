using Newtonsoft.Json;

namespace Models.DTO;

public class FriendCuDto
{
    [JsonProperty("friendId")]
    public Guid? FriendId { get; set; }

    [JsonProperty("firstName")]
    public string FirstName { get; set; }

    [JsonProperty("lastName")]
    public string LastName { get; set; }

    [JsonProperty("email")]
    public string Email { get; set; }

    [JsonProperty("birthday")]
    public DateTime? Birthday { get; set; }

    [JsonProperty("addressId")]
    public Guid? AddressId { get; set; }

    [JsonProperty("petsId")]
    public List<Guid> PetsId { get; set; }

    [JsonProperty("quotesId")]
    public List<Guid> QuotesId { get; set; }
}

public class AddressCuDto
{
    [JsonProperty("addressId")]
    public Guid? AddressId { get; set; }

    [JsonProperty("streetAddress")]
    public string StreetAddress { get; set; }

    [JsonProperty("zipCode")]
    public int ZipCode { get; set; }

    [JsonProperty("city")]
    public string City { get; set; }

    [JsonProperty("country")]
    public string Country { get; set; }

    [JsonProperty("friendsId")]
    public List<Guid> FriendsId { get; set; }
}

public class PetCuDto
{
    [JsonProperty("friendId")]
    public Guid FriendId { get; set; }

    [JsonProperty("petId")]
    public Guid? PetId { get; set; }

    [JsonProperty("kind")]
    public Models.Interfaces.AnimalKind Kind { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("mood")]
    public Models.Interfaces.AnimalMood Mood { get; set; }
}

public class QuoteCuDto
{
    [JsonProperty("quoteId")]
    public Guid? QuoteId { get; set; }

    // Friends WebAPI schema uses field name "quote" for the text
    [JsonProperty("quote")]
    public string Quote { get; set; }

    [JsonProperty("author")]
    public string Author { get; set; }

    [JsonProperty("friendsId")]
    public List<Guid> FriendsId { get; set; }
}

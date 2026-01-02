using Newtonsoft.Json;

namespace Models.DTO;

public sealed class FriendReadDto
{
    [JsonProperty("friendId")]
    public Guid FriendId { get; set; }

    [JsonProperty("firstName")]
    public string FirstName { get; set; }

    [JsonProperty("lastName")]
    public string LastName { get; set; }

    [JsonProperty("email")]
    public string Email { get; set; }

    [JsonProperty("birthday")]
    public DateTime? Birthday { get; set; }

    [JsonProperty("address")]
    public AddressReadDto Address { get; set; }

    [JsonProperty("pets")]
    public List<PetReadDto> Pets { get; set; }

    [JsonProperty("quotes")]
    public List<QuoteReadDto> Quotes { get; set; }
}

public sealed class AddressReadDto
{
    [JsonProperty("addressId")]
    public Guid AddressId { get; set; }

    [JsonProperty("streetAddress")]
    public string StreetAddress { get; set; }

    [JsonProperty("zipCode")]
    public int ZipCode { get; set; }

    [JsonProperty("city")]
    public string City { get; set; }

    [JsonProperty("country")]
    public string Country { get; set; }

    // Swagger shows this as string[] (often friend names/ids). Not needed in UI.
    [JsonProperty("friends")]
    public List<string> Friends { get; set; }
}

public sealed class PetReadDto
{
    [JsonProperty("petId")]
    public Guid PetId { get; set; }

    [JsonProperty("kind")]
    public int Kind { get; set; }

    [JsonProperty("mood")]
    public int Mood { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; }

    // Swagger shows this as string. Not needed in UI.
    [JsonProperty("friend")]
    public string Friend { get; set; }
}

public sealed class QuoteReadDto
{
    [JsonProperty("quoteId")]
    public Guid QuoteId { get; set; }

    [JsonProperty("quoteText")]
    public string QuoteText { get; set; }

    [JsonProperty("author")]
    public string Author { get; set; }

    // Swagger shows this as string[]. Not needed in UI.
    [JsonProperty("friends")]
    public List<string> Friends { get; set; }
}

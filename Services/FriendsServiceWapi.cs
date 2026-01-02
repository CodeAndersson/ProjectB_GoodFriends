using Microsoft.Extensions.Logging;
using Models;
using Models.DTO;
using Models.Interfaces;
using Newtonsoft.Json;

namespace Services;

public class FriendsServiceWapi : IFriendsService
{
    private readonly ILogger<FriendsServiceWapi> _logger;
    private readonly HttpClient _httpClient;

    private readonly JsonSerializerSettings _jsonSettings = new JsonSerializerSettings
    {
        Converters = {
            new AbstractConverter<Friend, IFriend>(),
            new AbstractConverter<Address, IAddress>(),
            new AbstractConverter<Pet, IPet>(),
            new AbstractConverter<Quote, IQuote>()
        }
    };

    public FriendsServiceWapi(IHttpClientFactory httpClientFactory, ILogger<FriendsServiceWapi> logger)
    {
        _logger = logger;
        _httpClient = httpClientFactory.CreateClient(name: "FriendsWebApi");
    }

    public async Task<ResponsePageDto<IFriend>> ReadFriendsAsync(bool seeded, bool flat, string filter, int pageNumber, int pageSize)
    {
        // Friends WebApi filtering appears to be name-centric. To support searching by City/Country/etc in the UI,
        // we fetch (paged) without server-side filter and then apply a richer filter client-side.
        if (!string.IsNullOrWhiteSpace(filter))
        {
            var all = new List<IFriend>();
            var fetchPageSize = 200;
            var fetchPageNr = 0;

            ResponsePageDto<IFriend> first;
            {
                string uri0 = $"Friends/Read?seeded={seeded.ToString().ToLowerInvariant()}&flat={flat.ToString().ToLowerInvariant()}&filter={Uri.EscapeDataString(string.Empty)}&pageNr={fetchPageNr}&pageSize={fetchPageSize}";
                HttpResponseMessage response0 = await _httpClient.GetAsync(uri0);
                await response0.EnsureSuccessStatusMessage();
                string s0 = await response0.Content.ReadAsStringAsync();
                first = JsonConvert.DeserializeObject<ResponsePageDto<IFriend>>(s0, _jsonSettings);
            }

            if (first?.PageItems is not null)
                all.AddRange(first.PageItems);

            var total = first?.DbItemsCount ?? all.Count;
            var totalPages = fetchPageSize > 0 ? (int)Math.Ceiling((double)total / fetchPageSize) : 1;

            for (var p = 1; p < totalPages; p++)
            {
                string uri = $"Friends/Read?seeded={seeded.ToString().ToLowerInvariant()}&flat={flat.ToString().ToLowerInvariant()}&filter={Uri.EscapeDataString(string.Empty)}&pageNr={p}&pageSize={fetchPageSize}";
                HttpResponseMessage response = await _httpClient.GetAsync(uri);
                await response.EnsureSuccessStatusMessage();
                string s = await response.Content.ReadAsStringAsync();
                var page = JsonConvert.DeserializeObject<ResponsePageDto<IFriend>>(s, _jsonSettings);
                if (page?.PageItems is not null)
                    all.AddRange(page.PageItems);
            }

            var tokens = filter
                .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(t => t.Trim())
                .Where(t => t.Length > 0)
                .ToArray();

            var filtered = tokens.Length == 0
                ? all
                : all.Where(f => MatchesAllTokens(f, tokens)).ToList();

            var paged = filtered
                .Skip(pageNumber * pageSize)
                .Take(pageSize)
                .ToList();

            return new ResponsePageDto<IFriend>
            {
                PageItems = paged,
                DbItemsCount = filtered.Count,
                PageNr = pageNumber,
                PageSize = pageSize
            };
        }

        {
            string uri = $"Friends/Read?seeded={seeded.ToString().ToLowerInvariant()}&flat={flat.ToString().ToLowerInvariant()}&filter={Uri.EscapeDataString(filter ?? string.Empty)}&pageNr={pageNumber}&pageSize={pageSize}";
            HttpResponseMessage response = await _httpClient.GetAsync(uri);
            await response.EnsureSuccessStatusMessage();

            string s = await response.Content.ReadAsStringAsync();
            var resp = JsonConvert.DeserializeObject<ResponsePageDto<IFriend>>(s, _jsonSettings);
            return resp;
        }
    }

    private static bool MatchesAllTokens(IFriend friend, string[] tokens)
    {
        foreach (var token in tokens)
        {
            if (!MatchesToken(friend, token))
                return false;
        }

        return true;
    }

    private static bool MatchesToken(IFriend friend, string token)
    {
        static bool Contains(string value, string token)
            => !string.IsNullOrWhiteSpace(value) && value.Contains(token, StringComparison.OrdinalIgnoreCase);

        if (Contains(friend.FirstName, token)) return true;
        if (Contains(friend.LastName, token)) return true;
        if (Contains(friend.Email, token)) return true;

        var a = friend.Address;
        if (a is not null)
        {
            if (Contains(a.StreetAddress, token)) return true;
            if (a.ZipCode.ToString().Contains(token, StringComparison.OrdinalIgnoreCase)) return true;
            if (Contains(a.City, token)) return true;
            if (Contains(a.Country, token)) return true;
        }

        foreach (var pet in friend.Pets ?? Enumerable.Empty<IPet>())
        {
            if (Contains(pet.Name, token)) return true;
            if (pet.Kind.ToString().Contains(token, StringComparison.OrdinalIgnoreCase)) return true;
            if (pet.Mood.ToString().Contains(token, StringComparison.OrdinalIgnoreCase)) return true;
        }

        foreach (var quote in friend.Quotes ?? Enumerable.Empty<IQuote>())
        {
            if (Contains(quote.Author, token)) return true;
            if (Contains(quote.QuoteText, token)) return true;
        }

        return false;
    }

    public async Task<IFriend> ReadFriendAsync(Guid id, bool flat)
    {
        string uri = $"Friends/ReadItem?id={id}&flat={flat.ToString().ToLowerInvariant()}";
        HttpResponseMessage response = await _httpClient.GetAsync(uri);
        await response.EnsureSuccessStatusMessage();

        string s = await response.Content.ReadAsStringAsync();

        // Friends/ReadItem is documented as returning the item directly, but other endpoints may wrap in { item: ... }.
        // Also: Swagger shows nested shapes that don't map 1:1 to our domain interfaces (e.g., pet.friend is a string).
        // So we deserialize into swagger-aligned DTOs and then map into our concrete domain models.
        FriendReadDto dto = null;

        var wrappedDto = JsonConvert.DeserializeObject<ResponseItemDto<FriendReadDto>>(s);
        if (wrappedDto?.Item is not null) dto = wrappedDto.Item;

        dto ??= JsonConvert.DeserializeObject<FriendReadDto>(s);
        if (dto is null) return null;

        var friend = new Friend
        {
            FriendId = dto.FriendId,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            Birthday = dto.Birthday,
            Address = dto.Address is null
                ? null
                : new Address
                {
                    AddressId = dto.Address.AddressId,
                    StreetAddress = dto.Address.StreetAddress,
                    ZipCode = dto.Address.ZipCode,
                    City = dto.Address.City,
                    Country = dto.Address.Country
                }
        };

        if (dto.Pets is not null)
        {
            foreach (var p in dto.Pets)
            {
                friend.Pets.Add(new Pet
                {
                    PetId = p.PetId,
                    Name = p.Name,
                    Kind = (AnimalKind)p.Kind,
                    Mood = (AnimalMood)p.Mood,
                    Friend = null
                });
            }
        }

        if (dto.Quotes is not null)
        {
            foreach (var q in dto.Quotes)
            {
                friend.Quotes.Add(new Quote
                {
                    QuoteId = q.QuoteId,
                    QuoteText = q.QuoteText,
                    Author = q.Author
                });
            }
        }

        return friend;
    }

    public async Task<FriendCuDto> ReadFriendCuDtoAsync(Guid id)
    {
        string uri = $"Friends/ReadItemDto?id={id}";
        HttpResponseMessage response = await _httpClient.GetAsync(uri);
        await response.EnsureSuccessStatusMessage();

        string s = await response.Content.ReadAsStringAsync();
        var wrapped = JsonConvert.DeserializeObject<ResponseItemDto<FriendCuDto>>(s);
        if (wrapped?.Item is not null) return wrapped.Item;

        return JsonConvert.DeserializeObject<FriendCuDto>(s);
    }

    public async Task<IFriend> UpdateFriendAsync(Guid id, FriendCuDto item)
    {
        string uri = $"Friends/UpdateItem/{id}";

        string body = JsonConvert.SerializeObject(item);
        var requestContent = new StringContent(body, System.Text.Encoding.UTF8, "application/json");

        HttpResponseMessage response = await _httpClient.PutAsync(uri, requestContent);
        await response.EnsureSuccessStatusMessage();

        string s = await response.Content.ReadAsStringAsync();
        var wrapped = JsonConvert.DeserializeObject<ResponseItemDto<IFriend>>(s, _jsonSettings);
        if (wrapped?.Item is not null) return wrapped.Item;

        return JsonConvert.DeserializeObject<IFriend>(s, _jsonSettings);
    }

    public async Task<IFriend> DeleteFriendAsync(Guid id)
    {
        string uri = $"Friends/DeleteItem/{id}";
        HttpResponseMessage response = await _httpClient.DeleteAsync(uri);
        await response.EnsureSuccessStatusMessage();

        string s = await response.Content.ReadAsStringAsync();
        var wrapped = JsonConvert.DeserializeObject<ResponseItemDto<IFriend>>(s, _jsonSettings);
        if (wrapped?.Item is not null) return wrapped.Item;

        return JsonConvert.DeserializeObject<IFriend>(s, _jsonSettings);
    }
}

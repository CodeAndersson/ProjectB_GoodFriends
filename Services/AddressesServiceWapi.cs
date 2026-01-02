using Microsoft.Extensions.Logging;
using Models;
using Models.DTO;
using Models.Interfaces;
using Newtonsoft.Json;

namespace Services;

public class AddressesServiceWapi : IAddressesService
{
    private readonly ILogger<AddressesServiceWapi> _logger;
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

    public AddressesServiceWapi(IHttpClientFactory httpClientFactory, ILogger<AddressesServiceWapi> logger)
    {
        _logger = logger;
        _httpClient = httpClientFactory.CreateClient(name: "FriendsWebApi");
    }

    public async Task<IAddress> ReadAddressAsync(Guid id, bool flat)
    {
        string uri = $"Addresses/ReadItem?id={id}&flat={flat.ToString().ToLowerInvariant()}";
        HttpResponseMessage response = await _httpClient.GetAsync(uri);
        await response.EnsureSuccessStatusMessage();

        string s = await response.Content.ReadAsStringAsync();
        var wrapped = JsonConvert.DeserializeObject<ResponseItemDto<IAddress>>(s, _jsonSettings);
        if (wrapped?.Item is not null) return wrapped.Item;

        return JsonConvert.DeserializeObject<IAddress>(s, _jsonSettings);
    }

    public async Task<AddressCuDto> ReadAddressCuDtoAsync(Guid id)
    {
        string uri = $"Addresses/ReadItemDto?id={id}";
        HttpResponseMessage response = await _httpClient.GetAsync(uri);
        await response.EnsureSuccessStatusMessage();

        string s = await response.Content.ReadAsStringAsync();
        var wrapped = JsonConvert.DeserializeObject<ResponseItemDto<AddressCuDto>>(s);
        if (wrapped?.Item is not null) return wrapped.Item;

        return JsonConvert.DeserializeObject<AddressCuDto>(s);
    }

    public async Task<IAddress> UpdateAddressAsync(Guid id, AddressCuDto item)
    {
        string uri = $"Addresses/UpdateItem/{id}";

        string body = JsonConvert.SerializeObject(item);
        var requestContent = new StringContent(body, System.Text.Encoding.UTF8, "application/json");

        HttpResponseMessage response = await _httpClient.PutAsync(uri, requestContent);
        await response.EnsureSuccessStatusMessage();

        string s = await response.Content.ReadAsStringAsync();
        var wrapped = JsonConvert.DeserializeObject<ResponseItemDto<IAddress>>(s, _jsonSettings);
        if (wrapped?.Item is not null) return wrapped.Item;

        return JsonConvert.DeserializeObject<IAddress>(s, _jsonSettings);
    }
}

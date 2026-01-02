using Microsoft.Extensions.Logging;
using Models;
using Models.DTO;
using Models.Interfaces;
using Newtonsoft.Json;

namespace Services;

public class PetsServiceWapi : IPetsService
{
    private readonly ILogger<PetsServiceWapi> _logger;
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

    public PetsServiceWapi(IHttpClientFactory httpClientFactory, ILogger<PetsServiceWapi> logger)
    {
        _logger = logger;
        _httpClient = httpClientFactory.CreateClient(name: "FriendsWebApi");
    }

    public async Task<IPet> DeletePetAsync(Guid id)
    {
        string uri = $"Pets/DeleteItem/{id}";
        HttpResponseMessage response = await _httpClient.DeleteAsync(uri);
        await response.EnsureSuccessStatusMessage();

        string s = await response.Content.ReadAsStringAsync();
        var wrapped = JsonConvert.DeserializeObject<Models.DTO.ResponseItemDto<IPet>>(s, _jsonSettings);
        if (wrapped?.Item is not null) return wrapped.Item;

        return JsonConvert.DeserializeObject<IPet>(s, _jsonSettings);
    }

    public async Task<PetCuDto> ReadPetCuDtoAsync(Guid id)
    {
        string uri = $"Pets/ReadItemDto?id={id}";
        HttpResponseMessage response = await _httpClient.GetAsync(uri);
        await response.EnsureSuccessStatusMessage();

        string s = await response.Content.ReadAsStringAsync();
        var wrapped = JsonConvert.DeserializeObject<PetCuDtoResponseItemDto>(s);
        if (wrapped?.Item is not null) return wrapped.Item;

        return JsonConvert.DeserializeObject<PetCuDto>(s);
    }

    public async Task<IPet> CreatePetAsync(PetCuDto item)
    {
        const string uri = "Pets/CreateItem";

        string body = JsonConvert.SerializeObject(item);
        var requestContent = new StringContent(body, System.Text.Encoding.UTF8, "application/json");

        HttpResponseMessage response = await _httpClient.PostAsync(uri, requestContent);
        await response.EnsureSuccessStatusMessage();

        string s = await response.Content.ReadAsStringAsync();
        var wrapped = JsonConvert.DeserializeObject<Models.DTO.ResponseItemDto<IPet>>(s, _jsonSettings);
        if (wrapped?.Item is not null) return wrapped.Item;

        return JsonConvert.DeserializeObject<IPet>(s, _jsonSettings);
    }

    public async Task<IPet> UpdatePetAsync(Guid id, PetCuDto item)
    {
        string uri = $"Pets/UpdateItem/{id}";

        string body = JsonConvert.SerializeObject(item);
        var requestContent = new StringContent(body, System.Text.Encoding.UTF8, "application/json");

        HttpResponseMessage response = await _httpClient.PutAsync(uri, requestContent);
        await response.EnsureSuccessStatusMessage();

        string s = await response.Content.ReadAsStringAsync();
        var wrapped = JsonConvert.DeserializeObject<Models.DTO.ResponseItemDto<IPet>>(s, _jsonSettings);
        if (wrapped?.Item is not null) return wrapped.Item;

        return JsonConvert.DeserializeObject<IPet>(s, _jsonSettings);
    }

    private sealed class PetCuDtoResponseItemDto
    {
        [JsonProperty("item")]
        public PetCuDto Item { get; set; }
    }
}

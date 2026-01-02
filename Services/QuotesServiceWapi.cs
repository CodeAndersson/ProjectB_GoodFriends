using Microsoft.Extensions.Logging;
using Models;
using Models.DTO;
using Models.Interfaces;
using Newtonsoft.Json;

namespace Services;

public class QuotesServiceWapi : IQuotesService
{
    private readonly ILogger<QuotesServiceWapi> _logger;
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

    public QuotesServiceWapi(IHttpClientFactory httpClientFactory, ILogger<QuotesServiceWapi> logger)
    {
        _logger = logger;
        _httpClient = httpClientFactory.CreateClient(name: "FriendsWebApi");
    }

    public async Task<IQuote> DeleteQuoteAsync(Guid id)
    {
        string uri = $"Quotes/DeleteItem/{id}";
        HttpResponseMessage response = await _httpClient.DeleteAsync(uri);
        await response.EnsureSuccessStatusMessage();

        string s = await response.Content.ReadAsStringAsync();
        var wrapped = JsonConvert.DeserializeObject<Models.DTO.ResponseItemDto<IQuote>>(s, _jsonSettings);
        if (wrapped?.Item is not null) return wrapped.Item;

        return JsonConvert.DeserializeObject<IQuote>(s, _jsonSettings);
    }

    public async Task<QuoteCuDto> ReadQuoteCuDtoAsync(Guid id)
    {
        string uri = $"Quotes/ReadItemDto?id={id}";
        HttpResponseMessage response = await _httpClient.GetAsync(uri);
        await response.EnsureSuccessStatusMessage();

        string s = await response.Content.ReadAsStringAsync();
        var wrapped = JsonConvert.DeserializeObject<QuoteCuDtoResponseItemDto>(s);
        if (wrapped?.Item is not null) return wrapped.Item;

        return JsonConvert.DeserializeObject<QuoteCuDto>(s);
    }

    public async Task<IQuote> CreateQuoteAsync(QuoteCuDto item)
    {
        const string uri = "Quotes/CreateItem";

        string body = JsonConvert.SerializeObject(item);
        var requestContent = new StringContent(body, System.Text.Encoding.UTF8, "application/json");

        HttpResponseMessage response = await _httpClient.PostAsync(uri, requestContent);
        await response.EnsureSuccessStatusMessage();

        string s = await response.Content.ReadAsStringAsync();
        var wrapped = JsonConvert.DeserializeObject<Models.DTO.ResponseItemDto<IQuote>>(s, _jsonSettings);
        if (wrapped?.Item is not null) return wrapped.Item;

        return JsonConvert.DeserializeObject<IQuote>(s, _jsonSettings);
    }

    public async Task<IQuote> UpdateQuoteAsync(Guid id, QuoteCuDto item)
    {
        string uri = $"Quotes/UpdateItem/{id}";

        string body = JsonConvert.SerializeObject(item);
        var requestContent = new StringContent(body, System.Text.Encoding.UTF8, "application/json");

        HttpResponseMessage response = await _httpClient.PutAsync(uri, requestContent);
        await response.EnsureSuccessStatusMessage();

        string s = await response.Content.ReadAsStringAsync();
        var wrapped = JsonConvert.DeserializeObject<Models.DTO.ResponseItemDto<IQuote>>(s, _jsonSettings);
        if (wrapped?.Item is not null) return wrapped.Item;

        return JsonConvert.DeserializeObject<IQuote>(s, _jsonSettings);
    }

    private sealed class QuoteCuDtoResponseItemDto
    {
        [JsonProperty("item")]
        public QuoteCuDto Item { get; set; }
    }
}

using System;
using Microsoft.Extensions.Logging;
using Models;
using Models.DTO;
using Models.Interfaces;
using Newtonsoft.Json;

namespace Services;

public class AdminServiceWapi : IAdminService
{
    private readonly ILogger<AdminServiceWapi> _logger;
    private readonly HttpClient _httpClient;

    //To ensure Json deserializern is using the class implementations instead of interfaces 
    private readonly JsonSerializerSettings _jsonSettings = new JsonSerializerSettings
    {
        Converters = {
            new AbstractConverter<Friend, IFriend>(),
            new AbstractConverter<Address, IAddress>(),
            new AbstractConverter<Pet, IPet>(),
            new AbstractConverter<Quote, IQuote>()
        },
    };

    public AdminServiceWapi(IHttpClientFactory httpClientFactory, ILogger<AdminServiceWapi> logger)
    {
        _logger = logger;
        _httpClient = httpClientFactory.CreateClient(name: "GuestWebApi");
    }
    
    public async Task<ResponseItemDto<GstUsrInfoAllDto>> GuestInfoAsync()
    {
        string uri = $"Guest/Info";

        //Send the HTTP Message and await the repsonse
        HttpResponseMessage response = await _httpClient.GetAsync(uri);

        //Throw an exception if the response is not successful
        await response.EnsureSuccessStatusMessage();

        //Get the response body
        string s = await response.Content.ReadAsStringAsync();

        // Friends WebApi often wraps results as { "item": { ... } }
        var wrapped = JsonConvert.DeserializeObject<ResponseItemDto<GstUsrInfoAllDto>>(s, _jsonSettings);
        if (wrapped?.Item != null)
        {
            return wrapped;
        }

        // Fallback for older/unwrapped payloads
        var info = JsonConvert.DeserializeObject<GstUsrInfoAllDto>(s, _jsonSettings);
        return new ResponseItemDto<GstUsrInfoAllDto>() { Item = info };
    }


    public async Task<ResponseItemDto<GstUsrInfoAllDto>> SeedAsync(int nrOfItems)
    {
        _logger.LogWarning("Friends WebAPI does not support seeding via API. SeedAsync is a no-op.");
        return await GuestInfoAsync();
    }
    public async Task<ResponseItemDto<GstUsrInfoAllDto>> RemoveSeedAsync(bool seeded)
    {
        _logger.LogWarning("Friends WebAPI does not support seed removal via API. RemoveSeedAsync is a no-op.");
        return await GuestInfoAsync();
    }

    public Task<ResponseItemDto<UsrInfoDto>> SeedUsersAsync(int nrOfUsers, int nrOfSuperUsers, int nrOfSysAdmin)
    {
        throw new NotImplementedException();
    }
}

public class AbstractConverter<TReal, TAbstract> : JsonConverter where TReal : TAbstract
{
    public override Boolean CanConvert(Type objectType)
        => objectType == typeof(TAbstract);

    public override Object ReadJson(JsonReader reader, Type objectType, Object existingValue, JsonSerializer serializer)
        => serializer.Deserialize<TReal>(reader);

    public override void WriteJson(JsonWriter writer, Object value, JsonSerializer serializer)
        => serializer.Serialize(writer, value);
}


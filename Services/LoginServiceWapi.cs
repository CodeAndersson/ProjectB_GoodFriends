using Microsoft.Extensions.Logging;
using Newtonsoft.Json;


using Models.DTO;

namespace Services;

// WebAPI login service to authenticate and obtain JWT tokens:

// - **Interface**: Implements `ILoginService` for WebAPI authentication
// - **Token Acquisition**: Calls WebAPI `/guest/loginuser` endpoint
// - **Response Handling**: Returns `LoginUserSessionDto` containing JWT token
// - **Error Management**: Ensures successful HTTP responses with proper error handling
public class LoginServiceWapi : ILoginService
{
    private readonly ILogger<LoginServiceWapi> _logger; 
    private readonly HttpClient _httpClient;
        
    public LoginServiceWapi(ILogger<LoginServiceWapi> logger, IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _httpClient = httpClientFactory.CreateClient(name: "GuestWebApi");
    }

    /// <summary>
    /// Logs in a user by sending their credentials to the WebAPI and retrieves a JWT token upon successful authentication.
    /// </summary>
    /// <param name="usrCreds">The user's login credentials.</param>
    /// <returns>A response containing the user's session information, including the JWT token.</returns>
    public async Task<ResponseItemDto<LoginUserSessionDto>> LoginUserAsync(LoginCredentialsDto usrCreds)
    {
        string uri = $"Guest/LoginUser";

        //Prepare the request content
        string body = JsonConvert.SerializeObject(usrCreds);
        // Swagger uses application/json-patch+json; API typically accepts application/json too.
        var requestContent = new StringContent(body, System.Text.Encoding.UTF8, "application/json-patch+json");

        //Send the HTTP Message and await the repsonse
        HttpResponseMessage response = await _httpClient.PostAsync(uri, requestContent);

        await response.EnsureSuccessStatusMessage();
      
        //Get the response data
        string s = await response.Content.ReadAsStringAsync();

        // Primary shape: { "item": { ...session... } }
        var wrapped = JsonConvert.DeserializeObject<ResponseItemDto<LoginUserSessionDto>>(s);
        if (wrapped?.Item is not null)
            return wrapped;

        // Fallback shape: { ...session... }
        var session = JsonConvert.DeserializeObject<LoginUserSessionDto>(s);
        return new ResponseItemDto<LoginUserSessionDto>() { Item = session };
    }
}


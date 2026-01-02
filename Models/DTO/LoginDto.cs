using Encryption;
using Newtonsoft.Json;

namespace Models.DTO;

public class LoginCredentialsDto
{
    [JsonProperty("userNameOrEmail")]
    public string UserNameOrEmail { get; set; }

    [JsonProperty("password")]
    public string Password { get; set; }
}

public class LoginUserSessionDto
{
    public Guid? UserId { get; set; }
    public string UserName { get; set; }
    public string UserRole { get; set; }
    public JwtToken JwtToken { get; set; }
}



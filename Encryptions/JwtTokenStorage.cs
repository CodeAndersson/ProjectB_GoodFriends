using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;

namespace Encryption;

// Static class for handling JWT token storage in authentication properties
// - **Purpose**: Centralized JWT token management and authentication property handling
// - **Token Storage**: Manages JWT tokens in ASP.NET Core authentication properties
// - **User Sign-in**: Handles user authentication with stored JWT tokens
// - **Separation of Concerns**: Dedicated class for token storage operations
public static class JwtTokenStorage
{
    /// <summary>
    /// Creates authentication properties with the JWT token stored for later retrieval
    /// </summary>
    /// <param name="jwtToken">The JWT token to store</param>
    /// <param name="isPersistent">Whether the authentication should persist across browser sessions</param>
    /// <returns>AuthenticationProperties with the JWT token stored</returns>
    private static AuthenticationProperties CreateAuthenticationPropertiesWithToken(JwtToken jwtToken, bool isPersistent)
    {
        if (jwtToken is null)
            throw new ArgumentNullException(nameof(jwtToken), "JWT token was null. WebAPI login likely failed or response was not parsed correctly.");
        if (string.IsNullOrWhiteSpace(jwtToken.EncryptedToken))
            throw new ArgumentException("JWT token was missing EncryptedToken.", nameof(jwtToken));

        var authProperties = new AuthenticationProperties
        {
            IsPersistent = isPersistent
        };
        authProperties.StoreTokens(
        [
            new AuthenticationToken { Name = "access_token", Value = jwtToken.EncryptedToken }
        ]);
        return authProperties;
    }

    /// <summary>
    /// Sign in User with the jwtToken stored in authentication properties
    /// </summary>
    /// <typeparam name="TUser">The type of the user</typeparam>
    /// <param name="signInManager">The SignInManager instance</param>
    /// <param name="user">The user to sign in</param>
    /// <param name="isPersistent">Whether the authentication should persist across browser sessions</param>
    /// <param name="jwtToken">The JWT token to store</param>
    /// <returns>True if sign-in was successful, false otherwise</returns>
    public static async Task StoreTokenAsync<TUser>(
        SignInManager<TUser> signInManager,
        TUser user,
        bool isPersistent,
        JwtToken jwtToken) where TUser : class
    {
        // Sign out to clear any existing authentication cookie
        await signInManager.SignOutAsync();

        // Create authentication properties with the WebAPI JWT token
        var authProperties = CreateAuthenticationPropertiesWithToken(jwtToken, isPersistent);

        // Sign in with the token stored in authentication properties
        await signInManager.SignInAsync(user, authProperties); 
    }
}
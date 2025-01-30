using ApplicationCodeWithPkceMauiApp.Security;
using System.ComponentModel;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace ApplicationCodeWithPkceMauiApp.ViewModels;

public partial class OAuthLoginViewModel : INotifyPropertyChanged
{
    const string baseAuthUrl = "https://api.smart-me.com/";
    const string authEndpoint = "oauth/authorize/";
    const string tokenEndpoint = "oauth/token/";
    const string clientId = "Please Add";
    const string redirectUri = "mysampleapp://callback/";
    const string scope = "device.read+offline_access";

    public event PropertyChangedEventHandler? PropertyChanged;

    public OAuthLoginViewModel()
    {
       
    }

    private string loginStatusText;

    public string LoginStatusText
    {
        get => loginStatusText;
        set
        {
            loginStatusText = value;
            OnPropertyChanged(nameof(LoginStatusText));
        }
    }

    public async Task<bool> TryLoginAndRedirectWithOauth()
    {
        try
        {
            LoginStatusText = "logging in...";

            var oauthTokens = await GetTokenByWebAuthenticator();

            if (oauthTokens == null)
            {
                LoginStatusText = $"Login failed";
                return false;
            }


            LoginStatusText = $"Login ok";
            return true;
        }
        catch (Exception ex)
        {
            // Show error message
            LoginStatusText = $"Login failed {ex.Message}";

            return false;
        }
    }

    private async Task<SmartMeOAuthTokens?> GetTokenByWebAuthenticator()
    {
        // Step 1: Generate an auth url
        var codeMethod = "S256";
        var pkce = Pkce.Generate();
        var codeVerifier = pkce.verifier;
        var shaVerifier = pkce.code_challenge;
        var state = Guid.NewGuid().ToString();

        var authUrl = $"{baseAuthUrl}{authEndpoint}?client_id={clientId}&response_type=code&redirect_uri={redirectUri}" +
            $"&scope={scope}&state={state}&code_challenge_method={codeMethod}&code_challenge={shaVerifier}";

        // Step 2: Redirect to Keycloak for Authentication and waiting for callback for code
        WebAuthenticatorResult authResult = await WebAuthenticator.Default.AuthenticateAsync(
            new WebAuthenticatorOptions()
            {
                Url = new Uri(authUrl),
                CallbackUrl = new Uri(redirectUri),
            });

        string? code = null;
        authResult?.Properties.TryGetValue("code", out code);

        // Step 3: Exchange Authorization Code for Access Token
        return await ExchangeCodeForTokenAsync($"{baseAuthUrl}{tokenEndpoint}", clientId, redirectUri, code, codeVerifier);
    }

    private async Task<SmartMeOAuthTokens?> ExchangeCodeForTokenAsync(string tokenEndpoint, string clientId, string redirectUri, string? code, string codeVerifier)
    {
        using (var client = new HttpClient())
        {
            var requestData = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("grant_type", "authorization_code"),
                new KeyValuePair<string, string>("code", code),
                new KeyValuePair<string, string>("redirect_uri", redirectUri),
                new KeyValuePair<string, string>("client_id", clientId),
                new KeyValuePair<string, string>("code_verifier", codeVerifier)
            };
            var request = new HttpRequestMessage(HttpMethod.Post, tokenEndpoint)
            {
                Content = new FormUrlEncodedContent(requestData)
            };
            var response = await client.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var tokenResponse = await response.Content.ReadAsStringAsync();
                var json = JsonSerializer.Deserialize<JsonNode>(tokenResponse);
                var accessToken = json["access_token"].ToString();
                var refreshToken = json["refresh_token"].ToString();

                return new SmartMeOAuthTokens(new OAuthToken(accessToken), new OAuthToken(refreshToken));
            }
            else
            {
                var result = await response.Content.ReadAsStringAsync();
                throw new Exception($"Failed to retrieve token. Status code: {response.StatusCode}, Result: {result}");
            }
        }
    }

    private void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

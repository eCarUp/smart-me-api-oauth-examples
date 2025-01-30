using System.Text.Json.Serialization;
using System.Text.Json;
using RestSharp;

namespace DeviceCodeSample;

public class AuthWithDeviceCode
{
    // Get this from the smart-me API Page (OAuth 
    private const string ClientId = "Please Add";
    private const string AuthorizationBaseUrl = "https://api.smart-me.com/";
    private const string DeviceAuthorizationEndpoint = "oauth/device";
    private const string TokenEndpoint = "oauth/token";
    private const string ApiEndpoint = "https://api.smart-me.com/devices";

    public async Task<bool> TryLoginAndCallApiAsync()
    {
        try
        {
            using var authClient = new RestClient(AuthorizationBaseUrl);

            // Step 1: Get device authorization code
            var deviceAuth = await RequestDeviceAuthorizationAsync(authClient);
            if (deviceAuth == null) return false;

            Console.WriteLine($"Open a browser and paste the URL: {deviceAuth.VerificationUri}, then enter code: {deviceAuth.UserCode}");
            Console.WriteLine("--------------------------------------------");

            // Step 2: Poll for token
            var accessToken = await PollForTokenAsync(authClient, deviceAuth);
            if (accessToken == null)
            {
                return false;
            }

            // Step 3: Call protected API
            return await CallApiWithTokenAsync(accessToken);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
            return false;
        }
    }

    private async Task<DeviceAuthorizationResponse?> RequestDeviceAuthorizationAsync(RestClient authClient)
    {
        var request = new RestRequest(DeviceAuthorizationEndpoint, Method.Post);
        request.AddHeader("content-type", "application/x-www-form-urlencoded");
        request.AddParameter("application/x-www-form-urlencoded",
            $"client_id={ClientId}&scope=offline_access+device.read",
            ParameterType.RequestBody);

        var response = await authClient.ExecuteAsync(request);
        if (!response.IsSuccessful)
        {
            Console.WriteLine($"Device authorization failed: {response.ErrorMessage}");
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<DeviceAuthorizationResponse>(response.Content!);
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"Failed to parse device authorization response: {ex.Message}");
            return null;
        }
    }

    private async Task<string?> PollForTokenAsync(RestClient authClient, DeviceAuthorizationResponse deviceAuth)
    {
        var interval = deviceAuth.Interval > 0 ? deviceAuth.Interval : 5;
        var expirationTime = DateTime.UtcNow.AddSeconds(deviceAuth.ExpiresIn);

        while (DateTime.UtcNow < expirationTime)
        {
            await Task.Delay(interval * 1000);

            var request = new RestRequest(TokenEndpoint, Method.Post);
            request.AddHeader("content-type", "application/x-www-form-urlencoded");
            request.AddParameter("application/x-www-form-urlencoded",
                $"grant_type=urn:ietf:params:oauth:grant-type:device_code&device_code={deviceAuth.DeviceCode}&client_id={ClientId}",
                ParameterType.RequestBody);

            var response = await authClient.ExecuteAsync(request);

            if (response.IsSuccessful)
            {
                try
                {
                    var tokenResponse = JsonSerializer.Deserialize<TokenSuccessResponse>(response.Content!);
                    Console.WriteLine("Successfully obtained access token");
                    return tokenResponse?.AccessToken;
                }
                catch (JsonException ex)
                {
                    Console.WriteLine($"Failed to parse token response: {ex.Message}");
                    return null;
                }
            }

            var errorResponse = JsonSerializer.Deserialize<TokenErrorResponse>(response.Content!);
            Console.WriteLine($"Authorization pending: {errorResponse?.Error} - {errorResponse?.ErrorDescription}");

            if (errorResponse?.Error != "authorization_pending")
            {
                break;
            }
        }

        Console.WriteLine("Failed to obtain access token within the allowed time");
        return null;
    }

    private async Task<bool> CallApiWithTokenAsync(string accessToken)
    {
        try
        {
            using var apiClient = new RestClient(ApiEndpoint);
            var request = new RestRequest();
            request.Method = Method.Get;
            request.AddHeader("content-type", "application/json");
            request.AddHeader("authorization", $"Bearer {accessToken}");

            var response = await apiClient.ExecuteAsync(request);

            Console.WriteLine("--------------------------------------------");
            Console.WriteLine("Response from API:");
            Console.WriteLine(response.IsSuccessful ? response.Content : $"API call failed: {response.ErrorMessage}");

            return response.IsSuccessful;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"API call failed: {ex.Message}");
            return false;
        }
    }

    // JSON response classes
    private class DeviceAuthorizationResponse
    {
        [JsonPropertyName("device_code")] public string DeviceCode { get; set; } = null!;
        [JsonPropertyName("user_code")] public string UserCode { get; set; } = null!;
        [JsonPropertyName("verification_uri")] public string VerificationUri { get; set; } = null!;
        [JsonPropertyName("expires_in")] public int ExpiresIn { get; set; }
        [JsonPropertyName("interval")] public int Interval { get; set; }
    }

    private class TokenSuccessResponse
    {
        [JsonPropertyName("access_token")] public string AccessToken { get; set; } = null!;
    }

    private class TokenErrorResponse
    {
        [JsonPropertyName("error")] public string Error { get; set; } = null!;
        [JsonPropertyName("error_description")] public string ErrorDescription { get; set; } = null!;
    }
}

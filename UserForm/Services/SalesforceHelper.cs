using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using UserForm.ViewModels.Account;

namespace UserForm.Services;

public static class SalesforceHelper
{
    private static readonly HttpClient _httpClient = new();

    public static async Task<string> GetAccessTokenAsync()
    {
        string passwordWithToken = "sk.147456369" + "1k9Y3bc2QHxQO6dVm3xTd78rC";

        var form = new Dictionary<string, string>
        {
            {"grant_type", "password"},
            {"client_id", "3MVG9rZjd7MXFdLga4IvrmO5baucvYrJgsWMXaGfG9y.XjBdU_rgr5iyaSICa10hOnWIA2F7CwAFZLeusImu6"},
            {"client_secret", "043DB510A2DC3389DD28F0D131884111585B7EDCE067FDB1F47ADCD1EDCDEABD"},
            {"username", "sifatshikdhar956@agentforce.com"},
            {"password", passwordWithToken}
        };

        var response = await _httpClient.PostAsync("https://login.salesforce.com/services/oauth2/token", new FormUrlEncodedContent(form));
        response.EnsureSuccessStatusCode();
        var result = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return result.RootElement.GetProperty("access_token").GetString();
    }

    public static async Task<string> CreateAccountAsync(string token, SalesforceAccountViewModel model)
    {
        var account = new
        {
            Name = model.Company,
        };

        var request = new HttpRequestMessage(HttpMethod.Post, "https://your-instance.salesforce.com/services/data/v59.0/sobjects/Account/")
        {
            Content = new StringContent(JsonSerializer.Serialize(account), Encoding.UTF8, "application/json")
        };

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var result = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return result.RootElement.GetProperty("id").GetString();
    }

    public static async Task<string> CreateContactAsync(string token, SalesforceAccountViewModel model, string accountId)
    {
        var contact = new
        {
            LastName = model.Name,
            Email = model.Email,
            Phone = model.Phone,
            AccountId = accountId
        };

        var request = new HttpRequestMessage(HttpMethod.Post, "https://your-instance.salesforce.com/services/data/v59.0/sobjects/Contact/")
        {
            Content = new StringContent(JsonSerializer.Serialize(contact), Encoding.UTF8, "application/json")
        };

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var result = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return result.RootElement.GetProperty("id").GetString();
    }
}

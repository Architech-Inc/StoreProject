using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        var handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
        };
        var client = new HttpClient(handler);
        
        var fullJson = @"{
            ""id"": ""testId"",
            ""rawId"": ""dGVzdFJhd0lk"",
            ""type"": ""public-key"",
            ""clientExtensionResults"": {},
            ""response"": {
                ""attestationObject"": ""dGVzdEF0dGVzdGF0aW9uT2JqZWN0"",
                ""clientDataJSON"": ""dGVzdENsaWVudERhdGFKU09O""
            }
        }";
        
        var content = new StringContent(fullJson, Encoding.UTF8, "application/json");
        
        try {
            var response = await client.PostAsync("https://localhost:7112/api/webauthn/makeCredential", content);
            var respStr = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Status Code: {response.StatusCode}");
            Console.WriteLine($"Response: {respStr}");
        } catch (Exception ex) {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}

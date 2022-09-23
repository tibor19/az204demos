using System.Threading.Tasks;
using Microsoft.Identity.Client;

internal class Program
{
    private static async Task Main(string[] args)
    {
        Console.WriteLine("Hello, World!");

        const string _clientId = "6c68a0b1-c748-4e78-a340-4e288520bd52";
        const string _tenantId = "5ef4e0bb-9bbd-44c0-b647-2c76a1fb1fec";

        var app = PublicClientApplicationBuilder
            .Create(_clientId)
            .WithAuthority(AzureCloudInstance.AzurePublic, _tenantId)
            .WithRedirectUri("http://localhost")
            .Build();

        string[] scopes = { "user.read" };
        AuthenticationResult result = await app.AcquireTokenInteractive(scopes).ExecuteAsync();

        Console.WriteLine($"Token:\t{result.AccessToken}");

        var client = new HttpClient();
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {result.AccessToken}");
        var content = await client.GetStringAsync("https://graph.microsoft.com/v1.0/me");
        Console.WriteLine(content);

    }
}
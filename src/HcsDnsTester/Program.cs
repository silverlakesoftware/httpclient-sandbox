using System.Net.Http;

namespace HcsDnsTester
{
    internal class Program
    {
        const string ApiEndpointUrl = "http://host.sandbox.example";

        static async Task Main(string[] args)
        {
            switch (args[0])
            {
                case "DisposeHttpClient":
                    await DisposeHttpClient();
                    break;
                case "SingletonHttpClient":
                    await SingletonHttpClient();
                    break;
                case "DnsTestHttpClient":
                    await DnsTestHttpClient();
                    break;
            }
        }

        static async Task SingletonHttpClient()
        {
            try
            {
                using var httpClient = new HttpClient() { BaseAddress = new Uri(ApiEndpointUrl) };
                for (int i = 0; i < 100; ++i)
                {
                    var result = await httpClient.GetStringAsync("weatherforecast").ConfigureAwait(false);
                    Console.WriteLine($"Request {i} Succeeded.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        static async Task DisposeHttpClient()
        {
            try
            {
                for (int i = 0; i < 100; ++i)
                {
                    using var httpClient = new HttpClient() { BaseAddress = new Uri(ApiEndpointUrl) };
                    var result = await httpClient.GetStringAsync("weatherforecast").ConfigureAwait(false);
                    Console.WriteLine($"Request {i} Succeeded.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        static async Task DnsTestHttpClient()
        {
            var handler = new SocketsHttpHandler
            {
                PooledConnectionLifetime = TimeSpan.FromSeconds(30)
            };
            var httpClient = new HttpClient(handler) { BaseAddress = new Uri(ApiEndpointUrl) };

            using var timer = new PeriodicTimer(TimeSpan.FromSeconds(5));
            while (true)
            {
                var result = await httpClient.GetStringAsync("weatherforecast").ConfigureAwait(false);
                Console.WriteLine(result);
                await timer.WaitForNextTickAsync();
            }
        }
    }
}
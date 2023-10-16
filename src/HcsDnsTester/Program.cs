namespace HcsDnsTester
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var handler = new SocketsHttpHandler
            {
                PooledConnectionLifetime = TimeSpan.FromMinutes(1)
            };
            var httpClient = new HttpClient(handler);
            httpClient.BaseAddress = new Uri("http://host.silverlake.dev");

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
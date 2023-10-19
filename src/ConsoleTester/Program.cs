using System.Text;
using HttpClientSandboxContainers;

// TODOS:
// - Pass reaper session in so intermediate images can be deleted
// - Pass cancellation token all the way through

namespace ConsoleTester
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            await Task.WhenAll(HcsResourceBuilder.CreateDnsServerImage(), HcsResourceBuilder.CreateBaseImage(), HcsResourceBuilder.CreateApiAppImage(), HcsResourceBuilder.CreateDnsTesterImage()).ConfigureAwait(false);

            await HcsNetworkBuilder.CreateNetwork().ConfigureAwait(false);

            var dnsServer = await HcsContainerBuilder.StartDnsServer().ConfigureAwait(false);
            await HcsContainerBuilder.StartApiApp1().ConfigureAwait(false);
            await HcsContainerBuilder.StartApiApp2().ConfigureAwait(false);
            var clientApp = await HcsContainerBuilder.StartClientApp().ConfigureAwait(false);

            var rawContents = await dnsServer.ReadFileAsync("/etc/bind/zones/db.sandbox.example");
            var contents = Encoding.ASCII.GetString(rawContents);
            contents = contents.Replace(
                "host.sandbox.example.         IN      A      172.20.1.3",
                "host.sandbox.example.         IN      A      172.20.1.4");
            rawContents = Encoding.ASCII.GetBytes(contents);
            await dnsServer.CopyAsync(rawContents, "/etc/bind/zones/db.sandbox.example");

            await dnsServer.ExecAsync(new[] { "rndc", "reload" });

            var result = await clientApp.ExecAsync(new[] { "/bin/bash", "-xc", "ss -t state time-wait | wc -l" });
        }
    }
}
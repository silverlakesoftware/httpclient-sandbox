using System.Text;
using Docker.DotNet.Models;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using DotNet.Testcontainers.Images;
using DotNet.Testcontainers.Networks;

// TODOS:
// - Pass reaper session in so intermediate images can be deleted
// - Pass cancellation token all the way through


namespace ConsoleTester
{
    internal class Program
    {
        const string HcsNetworkName = "hcs-demo-net";
        const string DnsHostIpAddress = "172.20.1.2";
        const string HcsProjectName = "hcs-demo";
        const string HcsApiAppImage = "hcs-api-app";

        static async Task Main(string[] args)
        {
            await Task.WhenAll(CreateBind9Image(), CreateBaseImage(), CreateHostAppImage(), CreateDnsTesterImage()).ConfigureAwait(false);

            await CreateNetwork().ConfigureAwait(false);

            var bind9 = await StartDnsHost().ConfigureAwait(false);
            await StartApiApp1().ConfigureAwait(false);
            await StartApiApp2().ConfigureAwait(false);
            await StartClientApp().ConfigureAwait(false);


            var rawContents = await bind9.ReadFileAsync("/etc/bind/zones/db.silverlake.dev");
            var contents = Encoding.ASCII.GetString(rawContents);
            contents = contents.Replace(
                "host.silverlake.dev.         IN      A      172.20.1.3",
                "host.silverlake.dev.         IN      A      172.20.1.4");
            rawContents = Encoding.ASCII.GetBytes(contents);
            await bind9.CopyAsync(rawContents, "/etc/bind/zones/db.silverlake.dev");

            await bind9.ExecAsync(new[] { "rndc", "reload" });

        }
        
        static async Task<INetwork> CreateNetwork()
        {
            var network = new NetworkBuilder()
                .WithName(HcsNetworkName)
                .WithCreateParameterModifier(pm => pm.IPAM = new IPAM
                {
                    Config = new List<IPAMConfig>
                    {
                        new() { Subnet = "172.20.1.0/24" }
                    }
                })
                .Build();

            await network.CreateAsync().ConfigureAwait(false);

            return network;
        }

        static async Task<IFutureDockerImage> CreateBind9Image()
        {
            var image = new ImageFromDockerfileBuilder()
                .WithDockerfileDirectory(CommonDirectoryPath.GetSolutionDirectory(), @"docker/hcs-dns-server")
                .WithDockerfile("dockerfile")
                .WithName("hcs-dns-server")
                .WithCleanUp(false)
                .Build();

            await image.CreateAsync().ConfigureAwait(false);

            return image;
        }

        static async Task<IFutureDockerImage> CreateBaseImage()
        {
            var image = new ImageFromDockerfileBuilder()
                .WithDockerfileDirectory(CommonDirectoryPath.GetSolutionDirectory(), @"docker/hcs-base")
                .WithDockerfile("dockerfile")
                .WithName("hcs-base")
                .WithCleanUp(false)
                .Build();

            await image.CreateAsync().ConfigureAwait(false);

            return image;
        }

        static async Task<IFutureDockerImage> CreateHostAppImage()
        {
            var image = new ImageFromDockerfileBuilder()
                .WithDockerfileDirectory(CommonDirectoryPath.GetSolutionDirectory(), ".")
                .WithDockerfile("src/MinimalService/Dockerfile")
                .WithName(HcsApiAppImage)
                .WithCleanUp(false)
                .Build();

            await image.CreateAsync().ConfigureAwait(false);

            return image;
        }

        static async Task<IFutureDockerImage> CreateDnsTesterImage()
        {
            var image = new ImageFromDockerfileBuilder()
                .WithDockerfileDirectory(CommonDirectoryPath.GetSolutionDirectory(), ".")
                .WithDockerfile("src/HcsDnsTester/Dockerfile")
                .WithName("hcs-dns-tester")
                .WithCleanUp(false)
                .Build();


            await image.CreateAsync().ConfigureAwait(false);

            return image;
        }

        static async Task<IContainer> StartDnsHost()
        {
            var container = new ContainerBuilder()
                .WithName("hcs-dns-server")
                .WithImage("hcs-dns-server")
                .WithProjectLabels(HcsProjectName)
                .WithNetwork(HcsNetworkName)
                .WithIpAddress(HcsNetworkName, DnsHostIpAddress)
                .Build();

            await container.StartAsync().ConfigureAwait(false);

            return container;
        }

        static async Task<IContainer> StartApiApp1()
        {
            var container = new ContainerBuilder()
                .WithName("hcs-api-1")
                .WithImage(HcsApiAppImage)
                .WithProjectLabels(HcsProjectName)
                .WithNetwork(HcsNetworkName)
                .WithDns(DnsHostIpAddress)
                .WithIpAddress(HcsNetworkName, "172.20.1.3")
                .Build();

            await container.StartAsync().ConfigureAwait(false);

            return container;
        }

        static async Task<IContainer> StartApiApp2()
        {
            var container = new ContainerBuilder()
                .WithName("hcs-api-2")
                .WithImage(HcsApiAppImage)
                .WithProjectLabels(HcsProjectName)
                .WithNetwork(HcsNetworkName)
                .WithDns(DnsHostIpAddress)
                .WithIpAddress(HcsNetworkName, "172.20.1.4")
                .Build();

            await container.StartAsync().ConfigureAwait(false);

            return container;
        }

        static async Task<IContainer> StartClientApp()
        {
            var container = new ContainerBuilder()
                .WithName("hcs-client")
                .WithImage("hcs-dns-tester")
                .WithProjectLabels(HcsProjectName)
                .WithNetwork(HcsNetworkName)
                .WithDns(DnsHostIpAddress)
                .WithIpAddress(HcsNetworkName, "172.20.1.5")
                .Build();

            await container.StartAsync().ConfigureAwait(false);

            return container;
        }

        static async Task<IContainer> StartMinimalServiceAsync()
        {
            var container = new ContainerBuilder()
                .WithName("TC-MinimalService")
                .WithImage("minimal-service:dev")
                .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Development")
                .WithEnvironment("ASPNETCORE_URLS", "http://+:80")
                .WithPortBinding(80, true)
                .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(80))
                .Build();

            await container.StartAsync().ConfigureAwait(false);

            return container;
        }
    }
}
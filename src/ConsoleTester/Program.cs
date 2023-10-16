using System.Text;
using Docker.DotNet.Models;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
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
        private const string HcsNetworkName = "hcs-demo-net";
        private const string HcsProjectName = "hcs-demo";

        static async Task Main(string[] args)
        {
            // var futureImage = new ImageFromDockerfileBuilder()
            //     .WithDockerfileDirectory(CommonDirectoryPath.GetSolutionDirectory(), @"src\MinimalService")
            //     .WithDockerfile("Dockerfile")
            //     .Build();
            //
            // await futureImage.CreateAsync().ConfigureAwait(false);


            // await new ImageFromDockerfileBuilder()
            //     .WithName("tc-minimalservice-image")
            //     .WithDockerfileDirectory(CommonDirectoryPath.GetSolutionDirectory(), @"src\MinimalService")
            //     .WithDockerfile("Dockerfile")
            //     .WithBuildArgument("RESOURCE_REAPER_SESSION_ID", ResourceReaper.DefaultSessionId.ToString("D")) // https://github.com/testcontainers/testcontainers-dotnet/issues/602.
            //     .WithDeleteIfExists(false)
            //     .Build()
            //     .CreateAsync()
            //     .ConfigureAwait(false);

            // await CreateNetwork().ConfigureAwait(false);
            //
            // await CreateBind9Image().ConfigureAwait(false);
            // await CreateBaseImage().ConfigureAwait(false);


            await Task.WhenAll(CreateBind9Image(), CreateBaseImage(), CreateHostAppImage(), CreateDnsTesterImage()).ConfigureAwait(false);

            await CreateNetwork().ConfigureAwait(false);

            var bind9 = await StartBind9().ConfigureAwait(false);
            await StartHost1().ConfigureAwait(false);
            await StartHost2().ConfigureAwait(false);
            await StartClient().ConfigureAwait(false);


            var rawContents = await bind9.ReadFileAsync("/etc/bind/zones/db.silverlake.dev");
            var contents = Encoding.ASCII.GetString(rawContents);
            contents = contents.Replace(
                "host.silverlake.dev.         IN      A      172.20.1.3",
                "host.silverlake.dev.         IN      A      172.20.1.4");
            rawContents = Encoding.ASCII.GetBytes(contents);
            await bind9.CopyAsync(rawContents, "/etc/bind/zones/db.silverlake.dev");

            await bind9.ExecAsync(new[] { "rndc", "reload" });

            // var container = await StartMinimalServiceAsync().ConfigureAwait(false);
            //

            // await container.DisposeAsync().ConfigureAwait(false);
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
                .WithName("hcs-host-app")
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

        static async Task<IContainer> StartBind9()
        {
            var container = new ContainerBuilder()
                .WithName("hcs-dns-server")
                .WithImage("hcs-dns-server")
                .WithLabel("com.docker.compose.project", HcsProjectName)
                .WithLabel("com.docker.compose.config-hash","1")
                .WithLabel("com.docker.compose.service", "hcs-dns-server")
                .WithLabel("com.docker.compose.container-number", "1")
                .WithLabel("com.docker.compose.version", "2.21.0")
                .WithLabel("com.docker.compose.oneoff", "False")
                .WithCreateParameterModifier(pm => pm.AttachStdout = true)
                .WithCreateParameterModifier(pm => pm.AttachStderr = true)
                .WithAutoRemove(true)
                .WithNetwork(HcsNetworkName)
                .WithIpAddress(HcsNetworkName, "172.20.1.2")
                //.WithBindMount(@"E:\Sandbox\HttpClientSandbox\files\dns-example1\zones\", "/etc/bind/zones")
                //.WithOutputConsumer()
                .Build();

            await container.StartAsync().ConfigureAwait(false);

            return container;
        }

        static async Task<IContainer> StartHost1()
        {
            var container = new ContainerBuilder()
                .WithName("hcs-host1")
                .WithImage("hcs-host-app")
                .WithLabel("com.docker.compose.project", HcsProjectName)
                .WithLabel("com.docker.compose.service", "hcs-host1")
                .WithLabel("com.docker.compose.config-hash", "2")
                .WithLabel("com.docker.compose.container-number", "2")
                .WithLabel("com.docker.compose.oneoff", "False")
                .WithCreateParameterModifier(pm => pm.AttachStdout = true)
                .WithCreateParameterModifier(pm => pm.AttachStderr = true)
                //.WithAutoRemove(true)
                .WithNetwork(HcsNetworkName)
                .WithDns("172.20.1.2")
                .WithIpAddress(HcsNetworkName, "172.20.1.3")
                .Build();

            await container.StartAsync().ConfigureAwait(false);

            return container;
        }

        static async Task<IContainer> StartHost2()
        {
            var container = new ContainerBuilder()
                .WithName("hcs-host2")
                .WithImage("hcs-host-app")
                .WithLabel("com.docker.compose.project", HcsProjectName)
                .WithLabel("com.docker.compose.service", "hcs-host2")
                .WithLabel("com.docker.compose.config-hash", "3")
                .WithLabel("com.docker.compose.container-number", "3")
                .WithLabel("com.docker.compose.oneoff", "False")
                //.WithAutoRemove(true)
                .WithNetwork(HcsNetworkName)
                .WithDns("172.20.1.2")
                .WithIpAddress(HcsNetworkName, "172.20.1.4")
                .Build();

            await container.StartAsync().ConfigureAwait(false);

            return container;
        }

        static async Task<IContainer> StartClient()
        {
            var container = new ContainerBuilder()
                .WithName("hcs-client")
                .WithImage("hcs-dns-tester")
                .WithLabel("com.docker.compose.project", HcsProjectName)
                .WithLabel("com.docker.compose.service", "hcs-client")
                .WithLabel("com.docker.compose.config-hash", "4")
                .WithLabel("com.docker.compose.container-number", "4")
                .WithLabel("com.docker.compose.oneoff", "False")
                //.WithAutoRemove(true)
                .WithNetwork(HcsNetworkName)
                .WithDns("172.20.1.2")
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
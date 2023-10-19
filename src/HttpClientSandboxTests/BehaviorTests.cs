using HttpClientSandboxContainers;

namespace HttpClientSandboxTests
{
    public class BehaviorTests : IClassFixture<CommonResourcesFixture>
    {
        public BehaviorTests(CommonResourcesFixture commonResourcesFixture) { }

        [Fact]
        public async Task DisposeExhaustsTcpPorts()
        {
            await using var dnsServer = await HcsContainerBuilder.StartDnsServer();

            await using var _1 = await HcsContainerBuilder.StartApiApp1();
            await using var clientApp = await HcsContainerBuilder.StartClientApp();

            var result = await clientApp.ExecAsync(new[] { "/bin/bash", "-xc", "dotnet HcsDnsTester.dll DisposeHttpClient | tee /proc/1/fd/1" });

            var lines = result.Stdout.Trim().Split('\n');
            Assert.Equal("Cannot assign requested address (host.sandbox.example:80)", lines[^1]);
        }

        [Fact]
        public async Task SingletonDoesNotExhaustTcpPorts()
        {
            await using var dnsServer = await HcsContainerBuilder.StartDnsServer();

            await using var _1 = await HcsContainerBuilder.StartApiApp1();
            await using var clientApp = await HcsContainerBuilder.StartClientApp();

            //var result = await clientApp.ExecAsync(new[] { "dotnet", "HcsDnsTester.dll" });
            var result = await clientApp.ExecAsync(new[] { "/bin/bash", "-xc", "dotnet HcsDnsTester.dll SingletonHttpClient | tee /proc/1/fd/1" });

            var lines = result.Stdout.Trim().Split('\n');
            Assert.Equal("Request 99 Succeeded.", lines[^1]);
        }
    }
}
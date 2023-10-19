using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;

namespace HttpClientSandboxContainers;

public class HcsContainerBuilder
{

    public const string DnsHostIpAddress = "172.20.1.2";
    public const string HcsProjectName = "httpclient-sandbox";

    public static async Task<IContainer> StartDnsServer()
    {
        var container = new ContainerBuilder()
            .WithName("hcs-dns-server")
            .WithImage("hcs-dns-server")
            .WithProjectLabels(HcsProjectName)
            .WithNetwork(HcsNetworkBuilder.NetworkName)
            .WithIpAddress(HcsNetworkBuilder.NetworkName, DnsHostIpAddress)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(53))
            .Build();

        await container.StartAsync().ConfigureAwait(false);

        return container;
    }

    public static async Task<IContainer> StartApiApp1()
    {
        var container = new ContainerBuilder()
            .WithName("hcs-api-1")
            .WithImage(HcsResourceBuilder.ApiAppImageName)
            .WithProjectLabels(HcsContainerBuilder.HcsProjectName)
            .WithNetwork(HcsNetworkBuilder.NetworkName)
            .WithDns(HcsContainerBuilder.DnsHostIpAddress)
            .WithIpAddress(HcsNetworkBuilder.NetworkName, "172.20.1.3")
            .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(80))
            .Build();

        await container.StartAsync().ConfigureAwait(false);

        return container;
    }

    public static async Task<IContainer> StartApiApp2()
    {
        var container = new ContainerBuilder()
            .WithName("hcs-api-2")
            .WithImage(HcsResourceBuilder.ApiAppImageName)
            .WithProjectLabels(HcsContainerBuilder.HcsProjectName)
            .WithNetwork(HcsNetworkBuilder.NetworkName)
            .WithDns(HcsContainerBuilder.DnsHostIpAddress)
            .WithIpAddress(HcsNetworkBuilder.NetworkName, "172.20.1.4")
            .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(80))
            .Build();

        await container.StartAsync().ConfigureAwait(false);

        return container;
    }

    public static async Task<IContainer> StartClientApp()
    {
        var container = new ContainerBuilder()
            .WithName("hcs-client")
            .WithImage("hcs-dns-tester")
            .WithProjectLabels(HcsContainerBuilder.HcsProjectName)
            .WithNetwork(HcsNetworkBuilder.NetworkName)
            .WithDns(HcsContainerBuilder.DnsHostIpAddress)
            .WithIpAddress(HcsNetworkBuilder.NetworkName, "172.20.1.5")
            .WithCreateParameterModifier(pm => pm.HostConfig.Sysctls = new Dictionary<string, string>
            {
                ["net.ipv4.ip_local_port_range"] = "33000\t33050"
            })
            .Build();

        await container.StartAsync().ConfigureAwait(false);

        return container;
    }
}
using Docker.DotNet.Models;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Networks;

namespace HttpClientSandboxContainers;

public static class HcsNetworkBuilder
{
    public const string NetworkName = "hcs-demo-net";

    public static async Task<INetwork> CreateNetwork()
    {
        var network = new NetworkBuilder()
            .WithName(NetworkName)
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
}
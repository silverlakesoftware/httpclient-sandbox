using DotNet.Testcontainers.Builders;

namespace ConsoleTester;

public static class ContainerBuilderExtensions
{
    public static ContainerBuilder WithIpAddress(this ContainerBuilder cb, string ipAddress)
        => cb.WithCreateParameterModifier(pm => pm.NetworkingConfig.EndpointsConfig.Single().Value.IPAddress = ipAddress);

    public static ContainerBuilder WithIpAddress(this ContainerBuilder cb, string network, string ipAddress)
        => cb.WithCreateParameterModifier(pm => pm.NetworkingConfig.EndpointsConfig[network].IPAddress = ipAddress);

    public static ContainerBuilder WithDns(this ContainerBuilder cb, string ipAddress)
        => cb.WithCreateParameterModifier(pm => (pm.HostConfig.DNS ??= new List<string>()).Add(ipAddress));

}
using DotNet.Testcontainers.Builders;

namespace ConsoleTester;

public static class ContainerBuilderExtensions
{
    // Used to create a project group of containers in Docker Desktop
    const string DockerComposeProjectLabel = "com.docker.compose.project";
    // Names the service for the project log
    const string DockerComposeServiceLabel = "com.docker.compose.service";
    // Required for project logs to appear, shouldn't matter unless your also trying to use compose with the same containers 
    // https://github.com/docker/compose-cli/issues/935
    const string DockerComposeConfigHashLabel = "com.docker.compose.config-hash";
    // Required for project logs to appear
    const string DockerComposeOneoffLabel = "com.docker.compose.oneoff";

    public static ContainerBuilder WithIpAddress(this ContainerBuilder cb, string ipAddress)
        => cb.WithCreateParameterModifier(pm => pm.NetworkingConfig.EndpointsConfig.Single().Value.IPAddress = ipAddress);

    public static ContainerBuilder WithIpAddress(this ContainerBuilder cb, string network, string ipAddress)
        => cb.WithCreateParameterModifier(pm => pm.NetworkingConfig.EndpointsConfig[network].IPAddress = ipAddress);

    public static ContainerBuilder WithDns(this ContainerBuilder cb, string ipAddress)
        => cb.WithCreateParameterModifier(pm => (pm.HostConfig.DNS ??= new List<string>()).Add(ipAddress));

    public static ContainerBuilder WithProjectLabels(this ContainerBuilder cb, string projectName, string? serviceName = default, string? configHash  = default)
    {
        return cb
            .WithLabel(DockerComposeProjectLabel, projectName)
            .WithCreateParameterModifier(pm => pm.Labels.Add(DockerComposeServiceLabel, serviceName ?? pm.Name))
            .WithLabel(DockerComposeConfigHashLabel, configHash ?? Guid.NewGuid().ToString())
            .WithLabel(DockerComposeOneoffLabel, "False");
    }

}
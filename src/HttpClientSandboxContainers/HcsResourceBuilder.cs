using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Images;

namespace HttpClientSandboxContainers;

public static class HcsResourceBuilder
{
    public const string ApiAppImageName = "hcs-api-app";

    public static async Task<IFutureDockerImage> CreateDnsServerImage()
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

    public static async Task<IFutureDockerImage> CreateBaseImage()
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

    public static async Task<IFutureDockerImage> CreateApiAppImage()
    {
        var image = new ImageFromDockerfileBuilder()
            .WithDockerfileDirectory(CommonDirectoryPath.GetSolutionDirectory(), ".")
            .WithDockerfile("src/MinimalService/Dockerfile")
            .WithName(ApiAppImageName)
            .WithCleanUp(false)
            .Build();

        await image.CreateAsync().ConfigureAwait(false);

        return image;
    }

    public static async Task<IFutureDockerImage> CreateDnsTesterImage()
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
}
using System.Diagnostics.CodeAnalysis;
using DotNet.Testcontainers.Images;
using DotNet.Testcontainers.Networks;
using HttpClientSandboxContainers;

namespace HttpClientSandboxTests;

public class CommonResourcesFixture : IAsyncLifetime
{
    INetwork _network = null!;
    IFutureDockerImage _dnsServerImage = null!;
    IFutureDockerImage _apiAppImage = null!;
    IFutureDockerImage _clientAppImage = null!;

    [MemberNotNull(nameof(_dnsServerImage))]
    public async Task InitializeAsync()
    {
        _network = await HcsNetworkBuilder.CreateNetwork();

        var dnsServerImageTask = HcsResourceBuilder.CreateDnsServerImage();
        var apiAppImageTask = HcsResourceBuilder.CreateApiAppImage();
        var clientAppImageTask = HcsResourceBuilder.CreateDnsTesterImage();
        await Task.WhenAll(
            dnsServerImageTask,
            apiAppImageTask,
            clientAppImageTask);
        _dnsServerImage = await dnsServerImageTask;
        _apiAppImage = await apiAppImageTask;
        _clientAppImage = await clientAppImageTask;
    }

    public async Task DisposeAsync()
    {
        await Task.WhenAll(
            _dnsServerImage.DisposeAsync().AsTask(), 
            _apiAppImage.DisposeAsync().AsTask(),
            _clientAppImage.DisposeAsync().AsTask());
        await _network.DisposeAsync();
    }
}
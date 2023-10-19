using DotNet.Testcontainers.Configurations;

namespace HttpClientSandboxContainers;

public class LogConsumer : IOutputConsumer
{
    public bool Enabled { get; }

    public Stream Stdout { get; }

    public Stream Stderr { get; }

    public void Dispose()
    {
        throw new NotImplementedException();
    }

}
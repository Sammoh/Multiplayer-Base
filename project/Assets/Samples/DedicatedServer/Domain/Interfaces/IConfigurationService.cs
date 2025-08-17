using DedicatedServer.Domain.ValueObjects;

namespace DedicatedServer.Domain.Interfaces
{
    /// <summary>
    /// Domain interface for configuration management.
    /// Implementation is provided by infrastructure layer.
    /// </summary>
    public interface IConfigurationService
    {
        ServerConfiguration GetServerConfiguration();
        void SetServerConfiguration(string ip, int port, int queryPort);
        bool IsHeadlessMode { get; }
        string[] GetCommandLineArguments();
    }
}
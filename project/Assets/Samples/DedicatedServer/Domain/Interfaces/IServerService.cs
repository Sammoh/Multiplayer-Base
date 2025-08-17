using System;
using System.Threading.Tasks;
using DedicatedServer.Domain.Entities;
using DedicatedServer.Domain.ValueObjects;

namespace DedicatedServer.Domain.Interfaces
{
    /// <summary>
    /// Domain interface for server operations.
    /// Implementation is provided by infrastructure layer.
    /// </summary>
    public interface IServerService
    {
        bool IsRunning { get; }
        ServerConfiguration Configuration { get; }
        int ConnectedPlayerCount { get; }

        event Action<ulong> ClientConnected;
        event Action<ulong> ClientDisconnected;

        Task StartAsync(GameSession gameSession);
        void Stop();
    }
}
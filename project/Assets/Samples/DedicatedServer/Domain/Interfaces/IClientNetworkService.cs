using System;
using System.Threading.Tasks;
using DedicatedServer.Domain.Entities;
using DedicatedServer.Domain.ValueObjects;

namespace DedicatedServer.Domain.Interfaces
{
    /// <summary>
    /// Domain interface for client networking operations.
    /// Implementation is provided by infrastructure layer.
    /// </summary>
    public interface IClientNetworkService
    {
        ConnectStatus Status { get; }
        bool IsConnected { get; }

        event Action<ConnectStatus> StatusChanged;

        Task<bool> ConnectAsync(string serverIP, int port);
        void Disconnect();
    }
}
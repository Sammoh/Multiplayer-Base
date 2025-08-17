using System;
using System.Threading.Tasks;
using DedicatedServer.Domain.Entities;
using DedicatedServer.Domain.Interfaces;
using DedicatedServer.Domain.ValueObjects;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

namespace DedicatedServer.Infrastructure.Unity
{
    /// <summary>
    /// Unity Netcode implementation of server service.
    /// Isolates Unity-specific networking concerns from domain.
    /// </summary>
    public class UnityServerService : IServerService
    {
        private readonly NetworkManager _networkManager;
        private ServerConfiguration _configuration;

        public bool IsRunning { get; private set; }
        public ServerConfiguration Configuration => _configuration;
        public int ConnectedPlayerCount => _networkManager?.ConnectedClientsList?.Count ?? 0;

        public event Action<ulong> ClientConnected;
        public event Action<ulong> ClientDisconnected;

        public UnityServerService(NetworkManager networkManager, ServerConfiguration configuration)
        {
            _networkManager = networkManager ?? throw new ArgumentNullException(nameof(networkManager));
            _configuration = configuration;
        }

        public async Task StartAsync(GameSession gameSession)
        {
            Debug.Log($"[Server] Starting server @ {_configuration.ConnectionString} | {gameSession}");

            ConfigureTransport(_configuration.ServerIP, _configuration.ServerPort);

            // Hook client connect/disconnect for basic visibility.
            _networkManager.OnClientConnectedCallback += HandleClientConnected;
            _networkManager.OnClientDisconnectCallback += HandleClientDisconnected;

            // Start the server
            if (!_networkManager.StartServer())
            {
                Debug.LogError("[Server] Failed to start NetworkManager server.");
                Unsubscribe();
                return;
            }

            IsRunning = true;

            // Yield once to mimic async start (e.g., waiting one frame for transport to bind).
            await Task.Yield();

            Debug.Log($"[Server] '{_configuration.ServerName}' is running. Players: {ConnectedPlayerCount}, Address: {_configuration.ConnectionString}");
        }

        public void Stop()
        {
            if (!IsRunning) return;

            Debug.Log("[Server] Stopping server...");
            _networkManager.Shutdown();
            Unsubscribe();
            IsRunning = false;
        }

        private void ConfigureTransport(string listenAddress, int port)
        {
            var utp = _networkManager.NetworkConfig.NetworkTransport as UnityTransport;
            if (utp == null)
            {
                Debug.LogWarning("[Server] UnityTransport not found on NetworkManager. " +
                                 "Ensure a transport is configured; falling back to defaults.");
                return;
            }

            try
            {
                utp.SetConnectionData(listenAddress, (ushort)port, listenAddress: listenAddress);
            }
            catch (MissingMethodException)
            {
                // Fallback for newer UTP API versions
                Debug.LogWarning("[Server] Using fallback transport configuration method.");
            }
        }

        private void HandleClientConnected(ulong clientId)
        {
            Debug.Log($"[Server] Client connected: {clientId}. Players: {ConnectedPlayerCount}");
            ClientConnected?.Invoke(clientId);
        }

        private void HandleClientDisconnected(ulong clientId)
        {
            Debug.Log($"[Server] Client disconnected: {clientId}. Players: {ConnectedPlayerCount - 1}");
            ClientDisconnected?.Invoke(clientId);
        }

        private void Unsubscribe()
        {
            if (_networkManager != null)
            {
                _networkManager.OnClientConnectedCallback -= HandleClientConnected;
                _networkManager.OnClientDisconnectCallback -= HandleClientDisconnected;
            }
        }
    }
}
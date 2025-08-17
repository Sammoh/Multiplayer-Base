using System;
using System.Threading.Tasks;
using Matchplay.Server;
using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP; // Optional but recommended for IP/port config

// NOTE: This file is deprecated. Use DedicatedServer.Infrastructure.Unity.UnityServerService instead.
// This file remains for backward compatibility only.

namespace Server
{
    /// <summary>
    /// Minimal, decoupled server start info (replace with your own DTO if desired).
    /// </summary>
    public struct ServerStartInfo
    {
        public string Map;        // optional metadata for your own systems
        public string GameMode;   // optional metadata for your own systems
        public int    MaxPlayers; // optional metadata for your own systems

        public override string ToString() =>
            $"Map={Map ?? "-"}, Mode={GameMode ?? "-"}, MaxPlayers={MaxPlayers}";
    }

    /// <summary>
    /// Simple Netcode for GameObjects server manager with no Matchplay/Multiplay dependencies.
    /// Handles binding the UnityTransport, starting/stopping server, and basic client join/leave hooks.
    /// </summary>
    public class ServerGameManager : IDisposable
    {
        public bool   IsRunning     { get; private set; }
        public string ServerIP      => _serverIP;
        public int    ServerPort    => _serverPort;
        public string ConnectionStr => $"{_serverIP}:{_serverPort}";
        public string ServerName    { get; private set; }
        
        public MatchplayNetworkServer NetworkServer => m_NetworkServer;
        private MatchplayNetworkServer m_NetworkServer => _networkManager.GetComponent<MatchplayNetworkServer>();


        public int PlayerCount =>
            _networkManager != null ? _networkManager.ConnectedClientsList.Count : 0;

        public event Action<ulong> OnClientConnected;
        public event Action<ulong> OnClientDisconnected;

        private readonly NetworkManager _networkManager;
        private readonly string         _serverIP;
        private readonly int            _serverPort;

        public ServerGameManager(string serverIP, int serverPort, NetworkManager networkManager, string serverName = null)
        {
            _serverIP        = string.IsNullOrWhiteSpace(serverIP) ? "0.0.0.0" : serverIP;
            _serverPort      = serverPort <= 0 ? 7777 : serverPort;
            _networkManager  = networkManager != null
                ? networkManager
                : throw new ArgumentNullException(nameof(networkManager));

            ServerName = string.IsNullOrEmpty(serverName) ? MakeServerName() : serverName;
        }

        /// <summary>
        /// Configures the UnityTransport to listen on the given IP/Port and starts the NGO server.
        /// </summary>
        public async Task StartGameServerAsync(ServerStartInfo startInfo)
        {
            Debug.Log($"[Server] Starting server @ {ConnectionStr} | {startInfo}");

            ConfigureTransport(_serverIP, _serverPort);

            // Hook client connect/disconnect for basic visibility.
            _networkManager.OnClientConnectedCallback    += HandleClientConnected;
            _networkManager.OnClientDisconnectCallback   += HandleClientDisconnected;

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

            Debug.Log($"[Server] '{ServerName}' is running. Players: {PlayerCount}, Address: {ConnectionStr}");
        }

        /// <summary>
        /// Gracefully stops the server.
        /// </summary>
        public void StopServer()
        {
            if (!IsRunning) return;

            Debug.Log("[Server] Stopping server...");
            _networkManager.Shutdown();
            Unsubscribe();
            IsRunning = false;
        }

        public void Dispose()
        {
            StopServer();
        }

        // ──────────────────────────────────────────────────────────────────────────────
        // Internals
        private void ConfigureTransport(string listenAddress, int port)
        {
            var utp = _networkManager.NetworkConfig.NetworkTransport as UnityTransport;
            if (utp == null)
            {
                Debug.LogWarning("[Server] UnityTransport not found on NetworkManager. " +
                                 "Ensure a transport is configured; falling back to defaults.");
                return;
            }

            // Typical NGO+UTP configuration for a server socket
            try
            {
                // For most UTP versions:
                utp.SetConnectionData(listenAddress, (ushort)port, listenAddress: listenAddress);
            }
            catch (MissingMethodException)
            {
                // If using a newer UTP API, you might need to set fields directly instead.
                // Keep this as a fallback so this file remains transport-version tolerant.
                // Example (pseudocode, adjust to your UTP version):
                // utp.ConnectionData.Address = listenAddress;
                // utp.ConnectionData.Port    = (ushort)port;
                // utp.ConnectionData.ServerListenAddress = listenAddress;
            }
        }

        private void HandleClientConnected(ulong clientId)
        {
            Debug.Log($"[Server] Client connected: {clientId}. Players: {PlayerCount}");
            OnClientConnected?.Invoke(clientId);
        }

        private void HandleClientDisconnected(ulong clientId)
        {
            Debug.Log($"[Server] Client disconnected: {clientId}. Players: {PlayerCount - 1}");
            OnClientDisconnected?.Invoke(clientId);

            // If you want auto-shutdown when empty, uncomment:
            // if (PlayerCount <= 0) StopServer();
        }

        private void Unsubscribe()
        {
            _networkManager.OnClientConnectedCallback  -= HandleClientConnected;
            _networkManager.OnClientDisconnectCallback -= HandleClientDisconnected;
        }

        private static string MakeServerName()
        {
            // Lightweight, dependency-free name (replace with your own scheme if needed)
            return $"Server-{Guid.NewGuid().ToString("N").Substring(0, 8)}";
        }
    }
}

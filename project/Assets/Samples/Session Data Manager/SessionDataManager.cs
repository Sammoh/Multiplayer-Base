using System.Collections.Generic;
using Unity.Netcode;
using Unity.Services.Multiplayer;
using UnityEngine;

public class SessionDataManager : NetworkBehaviour
{
    public static SessionDataManager Instance { get; private set; }

    // Server-authoritative session data
    public NetworkVariable<ServerSessionData> serverData;

    // Clients session data managed by server
    private readonly Dictionary<ulong, ClientSessionData> _clientSessionDataMap = new();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback += HandleClientDisconnect;
        }
        
        if (IsClient && !IsServer)
        {
            serverData.OnValueChanged += OnServerDataChanged;

            // Immediately check current value
            OnServerDataChanged(serverData.Value, serverData.Value);
        }
    }

    private void OnServerDataChanged(ServerSessionData previous, ServerSessionData current)
    {
        Debug.Log($"Map loaded for session: {current.MapName}");
        // You could now trigger a map loader or set UI label
    }

    public override void OnNetworkDespawn()
    {
        if (IsServer && NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback -= HandleClientDisconnect;
        }
    }

    public void OnSessionJoined(ISession session)
    {
        // check to make sure it's the server
        if (IsServer)
        {
            // write the session data to the ServerSessionData
            SetServerSessionData(new ServerSessionData()
            {
                SessionName = session.Name,
                MaxPlayers = session.MaxPlayers,
                MapName = "VIVOX CODE",
                AllowSpectators = true
            });
        }
    }
    
    public void SetServerSessionData(ServerSessionData data)
    {
        if (!IsServer) return;
        serverData.Value = data;
        SetServerSessionDataServerRpc(data);
    }

    // Server-side initialization of session settings
    [ServerRpc(RequireOwnership = false)]
    public void SetServerSessionDataServerRpc(ServerSessionData data)
    {
        if (!IsServer) return;
        serverData.Value = data;
        Debug.Log("Server session data updated.");
    }

    // Server-side method for registering clients
    [ServerRpc(RequireOwnership = false)]
    public void RegisterClientDataServerRpc(ulong clientId, ClientSessionData clientData)
    {
        if (!IsServer) return;

        _clientSessionDataMap[clientId] = clientData;
        Debug.Log($"Registered client {clientId} with display name {clientData.DisplayName}.");
    }

    private void HandleClientDisconnect(ulong clientId)
    {
        if (_clientSessionDataMap.ContainsKey(clientId))
        {
            _clientSessionDataMap.Remove(clientId);
            Debug.Log($"Client {clientId} disconnected and removed from session data.");
        }
    }

    // Server method to retrieve client data (server authoritative)
    public ClientSessionData GetClientData(ulong clientId)
    {
        _clientSessionDataMap.TryGetValue(clientId, out var data);
        return data;
    }

    // Client-side retrieval of their own data
    public ClientSessionData GetLocalClientData()
    {
        var clientId = NetworkManager.Singleton.LocalClientId;
        _clientSessionDataMap.TryGetValue(clientId, out var data);
        return data;
    }

    // Get all client data on the server
    public IReadOnlyDictionary<ulong, ClientSessionData> GetAllClientsData() => _clientSessionDataMap;
}

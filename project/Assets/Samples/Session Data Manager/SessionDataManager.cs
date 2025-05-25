using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class SessionManager : NetworkBehaviour
{
    public static SessionManager Instance { get; private set; }

    // Server-authoritative session data
    // public NetworkVariable<ServerSessionData> ServerData = new(
    //     new ServerSessionData(), NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    // Clients session data managed by server
    private readonly Dictionary<ulong, ClientSessionData> clientSessionDataMap = new();

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
    }

    public override void OnNetworkDespawn()
    {
        if (IsServer && NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback -= HandleClientDisconnect;
        }
    }
    
    // Todo - when the server realizes that a client has connected then the server tells the client that there is a vivox code
    
    public void SetServerSessionData(ServerSessionData data)
    {
        if (!IsServer) return;
        // ServerData.Value = data;
        SetServerSessionDataServerRpc(data);
    }

    // Server-side initialization of session settings
    [ServerRpc(RequireOwnership = false)]
    public void SetServerSessionDataServerRpc(ServerSessionData data)
    {
        if (!IsServer) return;
        // ServerData.Value = data;
        Debug.Log("Server session data updated.");
    }

    // Server-side method for registering clients
    [ServerRpc(RequireOwnership = false)]
    public void RegisterClientDataServerRpc(ulong clientId, ClientSessionData clientData)
    {
        if (!IsServer) return;

        clientSessionDataMap[clientId] = clientData;
        Debug.Log($"Registered client {clientId} with display name {clientData.DisplayName}.");
    }

    private void HandleClientDisconnect(ulong clientId)
    {
        if (clientSessionDataMap.ContainsKey(clientId))
        {
            clientSessionDataMap.Remove(clientId);
            Debug.Log($"Client {clientId} disconnected and removed from session data.");
        }
    }

    // Server method to retrieve client data (server authoritative)
    public ClientSessionData GetClientData(ulong clientId)
    {
        clientSessionDataMap.TryGetValue(clientId, out var data);
        return data;
    }

    // Client-side retrieval of their own data
    public ClientSessionData GetLocalClientData()
    {
        var clientId = NetworkManager.Singleton.LocalClientId;
        clientSessionDataMap.TryGetValue(clientId, out var data);
        return data;
    }

    // Get all client data on the server
    public IReadOnlyDictionary<ulong, ClientSessionData> GetAllClientsData() => clientSessionDataMap;
}

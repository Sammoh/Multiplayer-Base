using Unity.Netcode;
using UnityEngine;
using Unity.Services.Multiplayer;

public class NetworkSessionHandler : MonoBehaviour
{
    private void OnEnable()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
    }

    private void OnDisable()
    {
        if (NetworkManager.Singleton == null) return;
        NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
    }

    private void OnClientConnected(ulong clientId)
    {
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            var playerId = PlayerIdentifier.GetOrCreatePlayerId();
            // var displayName = "Player_" + UnityEngine.Random.Range(1000, 9999);
            // SessionManager.Instance.RegisterPlayer(playerId, displayName);
        }
    }

    private void OnClientDisconnected(ulong clientId)
    {
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            // var playerId = PlayerIdentifier.GetOrCreatePlayerId();
            // SessionManager.Instance.RemovePlayer(playerId);
        }
    }
}
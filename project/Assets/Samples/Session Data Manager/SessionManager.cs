using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using Unity.Multiplayer.Widgets;

namespace MysteriousGuests.Netcode
{
    /// <summary>
    /// Non-networked singleton that keeps persistent per-player state across disconnects.
    /// Stores mapping from stable GUID -> PlayerSessionData, and clientId -> GUID.
    /// </summary>
    public class SessionManager : MonoBehaviour
    {
        public static SessionManager Instance { get; private set; }

        [Serializable]
        public struct PlayerSessionData
        {
            public string Guid;          // Stable ID from client
            public int Score;
            // public List<CardId> Hand;    // Hidden hand
            public bool IsConnected;     // convenience
        }

        private readonly Dictionary<string, PlayerSessionData> _guidToData = new();
        private readonly Dictionary<ulong, string> _clientToGuid = new(); // transient mapping

        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        void Start()
        {
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
        }

        // TODO: Uncomment and implement if you want to register players
        // public void RegisterPlayer(ulong clientId, string guid)
        // {
        //     _clientToGuid[clientId] = guid;
        //
        //     if (!_guidToData.TryGetValue(guid, out var data))
        //     {
        //         data = new PlayerSessionData
        //         {
        //             Guid = guid,
        //             Score = 0,
        //             Hand = new List<CardId>(),
        //             IsConnected = true
        //         };
        //         _guidToData.Add(guid, data);
        //     }
        //     else
        //     {
        //         data.IsConnected = true;
        //         _guidToData[guid] = data;
        //     }
        // }

        // TODO: Uncomment and implement if you want to save player state
        // public void SavePlayerState(string guid, int score, List<CardId> hand)
        // {
        //     if (!_guidToData.TryGetValue(guid, out var data)) return;
        //     data.Score = score;
        //     data.Hand = new List<CardId>(hand);
        //     _guidToData[guid] = data;
        // }

        public bool TryGetSavedState(string guid, out PlayerSessionData data) => _guidToData.TryGetValue(guid, out data);

        public string GetGuid(ulong clientId)
        {
            return _clientToGuid.GetValueOrDefault(clientId);
        }

        public void MarkDisconnected(ulong clientId)
        {
            if (!_clientToGuid.TryGetValue(clientId, out var guid)) return;
            if (_guidToData.TryGetValue(guid, out var data))
            {
                data.IsConnected = false;
                _guidToData[guid] = data;
            }
            _clientToGuid.Remove(clientId);
        }

        private void OnClientDisconnected(ulong clientId) => MarkDisconnected(clientId);
    }
}

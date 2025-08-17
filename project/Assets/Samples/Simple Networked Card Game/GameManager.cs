using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace MysteriousGuests.Netcode
{
    /// <summary>
    /// Orchestrates the match: start, phases, win checks.
    /// Server-only logic with minimal NetworkVariables (phases via TurnManager).
    /// </summary>
    public class GameManager : NetworkBehaviour
    {
        public static GameManager Instance { get; private set; }

        [SerializeField] private int cardsPerPlayer = 3;
        
        private void Awake() => Instance ??= this;

        public override void OnNetworkSpawn()
        {
            if (IsServer)
            {
                NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
            }
        }

        #region Server API
        public void StartGame()
        {
            if (!IsServer) return;

            // Phase: Dealing
            TurnManager.Instance.Phase.Value = GamePhase.Dealing;

            // Build deck
            CardManager.Instance.BuildAndShuffle();

            // Gather players (in connect order)
            var players = new List<PlayerController>();
            foreach (var kvp in NetworkManager.Singleton.ConnectedClientsList)
            {
                var pc = kvp.PlayerObject.GetComponent<PlayerController>();
                players.Add(pc);
            }

            // Deal
            foreach (var p in players)
            {
                var cards = CardManager.Instance.Draw(cardsPerPlayer);
                p.ServerGiveCards(cards);
            }

            // Build turn order
            var order = new List<ulong>();
            foreach (var p in players) order.Add(p.OwnerClientId);
            TurnManager.Instance.BuildOrder(order);

            TurnManager.Instance.Phase.Value = GamePhase.Playing;
        }

        public void EndGame()
        {
            if (!IsServer) return;
            TurnManager.Instance.Phase.Value = GamePhase.GameOver;
            // TODO: scoring, cleanup, show results
        }
        #endregion

        private void OnClientDisconnected(ulong clientId)
        {
            if (!IsServer) return;

            // Remove from turn order
            TurnManager.Instance.RemoveFromTurnOrder(clientId);

            // TODO Save their state
            // var guid = SessionManager.Instance.GetGuid(clientId);
            // if (guid != null)
            // {
            //     // find player's current state
            //     var playerObj = NetworkManager.Singleton.ConnectedClients.ContainsKey(clientId)
            //         ? NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject
            //         : null;
            //
            //     int score = 0;
            //     List<CardId> hand = new();
            //     if (playerObj)
            //     {
            //         var pc = playerObj.GetComponent<PlayerController>();
            //         score = pc.Score.Value;
            //         hand = pc.ServerPeekHand();
            //     }
            //
            //     SessionManager.Instance.SavePlayerState(guid, score, hand);
            // }
        }
    }
}

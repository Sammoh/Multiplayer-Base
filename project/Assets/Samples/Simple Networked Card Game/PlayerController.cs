using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace MysteriousGuests.Netcode
{
    /// <summary>
    /// Per-player NetworkBehaviour. Handles player-owned data and sends input to server.
    /// Server owns authority over actual game state; this class mirrors it via NetworkVariables.
    /// </summary>
    public class PlayerController : NetworkBehaviour
    {
        [Header("Inspector")]
        [SerializeField] private string _debugName = "Player";
        // Synced fields visible to all
        public NetworkVariable<FixedString64Bytes> DisplayName = new(writePerm: NetworkVariableWritePermission.Server);
        public NetworkVariable<int> Score = new(writePerm: NetworkVariableWritePermission.Server);

        // Server-only hand; client gets via targeted RPC
        private readonly List<CardId> _hand = new();

        // Persistent ID for reconnect. Stored locally, sent to server on spawn.
        private string _guid;

        #region Lifecycle
        public override void OnNetworkSpawn()
        {
            #if UNITY_EDITOR
                        // In Editor, use a unique name for debugging
                        _debugName = $"Player_{NetworkManager.LocalClientId}";
            #endif
            
            if (IsOwner && IsClient)
            {
                // _guid = PersistentIdProvider.GetOrCreateGuid();

                if (IsServer) // host path
                {
                    ServerRegisterLocalPlayer(_guid, _debugName, OwnerClientId);
                }
                else          // pure client path
                {
                    HelloServerRpc(_guid, _debugName);
                }
            }

            if (IsServer)
                NetworkObject.DontDestroyWithOwner = true;
        }
        #endregion

        #region RPCs
        [ServerRpc(RequireOwnership = false)]
        private void HelloServerRpc(string guid, string displayName, ServerRpcParams p = default)
        {
            // // TODO Register to SessionManager
            // SessionManager.Instance.RegisterPlayer(p.Receive.SenderClientId, guid);
            // DisplayName.Value = displayName;
            //
            // // Restore saved state if any
            // if (SessionManager.Instance.TryGetSavedState(guid, out var saved))
            // {
            //     Score.Value = saved.Score;
            //     _hand.Clear();
            //     _hand.AddRange(saved.Hand);
            //     // send hand to client
            //     SendHandClientRpc(_hand.ToArray(), new ClientRpcParams
            //     {
            //         Send = new ClientRpcSendParams { TargetClientIds = new[] { p.Receive.SenderClientId } }
            //     });
            // }
            // else
            // {
            //     Score.Value = 0;
            //     _hand.Clear();
            // }
        }

        [ClientRpc]
        private void SendHandClientRpc(CardId[] cards, ClientRpcParams rpcParams = default)
        {
            if (!IsOwner) return; // only owner cares
            // Update local UI
            HandUI.Instance?.Refresh(cards);
        }

        /// <summary>Client → Server intent to play a card from their hand.</summary>
        [ServerRpc(RequireOwnership = false)]
        public void PlayCardServerRpc(CardId card)
        {
            // if (!IsServer) return;
            // // Validate: Is it this player's turn? Do they have this card?
            // if (!TurnManager.Instance.IsPlayersTurn(OwnerClientId)) return;
            // int idx = _hand.FindIndex(c => c.Equals(card));
            // if (idx < 0) return;
            //
            // _hand.RemoveAt(idx);
            // CardManager.Instance.PlayCard(card, OwnerClientId);
            //
            // // Update Session + client
            // SessionManager.Instance.SavePlayerState(SessionManager.Instance.GetGuid(OwnerClientId), Score.Value, _hand);
            // SendHandClientRpc(_hand.ToArray(), new ClientRpcParams
            // {
            //     Send = new ClientRpcSendParams { TargetClientIds = new[] { OwnerClientId } }
            // });
        }

        [ServerRpc(RequireOwnership = false)]
        public void EndTurnServerRpc()
        {
            if (!IsServer) return;
            if (!TurnManager.Instance.IsPlayersTurn(OwnerClientId)) return;
            TurnManager.Instance.AdvanceTurn();
        }
        #endregion

        #region Public Server-side helpers
        private void ServerRegisterLocalPlayer(string guid, string displayName, ulong clientId)
        {
            // SessionManager.Instance.RegisterPlayer(clientId, guid);
            // DisplayName.Value = displayName;
            //
            // if (SessionManager.Instance.TryGetSavedState(guid, out var saved))
            // {
            //     Score.Value = saved.Score;
            //     _hand.Clear();
            //     _hand.AddRange(saved.Hand);
            //
            //     SendHandClientRpc(_hand.ToArray(), new ClientRpcParams
            //     {
            //         Send = new ClientRpcSendParams { TargetClientIds = new[] { clientId } }
            //     });
            // }
            // else
            // {
            //     Score.Value = 0;
            //     _hand.Clear();
            // }
        }

        public void ServerGiveCards(List<CardId> cards)
        {
            // if (!IsServer) return;
            // _hand.AddRange(cards);
            // var guid = SessionManager.Instance.GetGuid(OwnerClientId);
            // SessionManager.Instance.SavePlayerState(guid, Score.Value, _hand);
            // SendHandClientRpc(cards.ToArray(), new ClientRpcParams
            // {
            //     Send = new ClientRpcSendParams { TargetClientIds = new[] { OwnerClientId } }
            // });
        }

        public List<CardId> ServerPeekHand() => new List<CardId>(_hand);
        #endregion
    }
}

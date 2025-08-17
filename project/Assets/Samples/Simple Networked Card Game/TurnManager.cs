using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace MysteriousGuests.Netcode
{
    public enum GamePhase : byte { Lobby, Dealing, Playing, Resolving, GameOver }

    /// <summary>Server-authoritative turn order control.</summary>
    public class TurnManager : NetworkBehaviour
    {
        public static TurnManager Instance { get; private set; }

        public NetworkVariable<ulong> CurrentTurnClientId = new(writePerm: NetworkVariableWritePermission.Server);
        public NetworkVariable<GamePhase> Phase = new(writePerm: NetworkVariableWritePermission.Server);

        // server-only
        private readonly List<ulong> _order = new();
        private int _turnIndex;

        private void Awake()
        {
            Instance ??= this;
        }

        #region Server API
        public void BuildOrder(IReadOnlyList<ulong> clients)
        {
            if (!IsServer) return;
            _order.Clear();
            _order.AddRange(clients);
            _turnIndex = 0;
            if (_order.Count > 0) CurrentTurnClientId.Value = _order[0];
        }

        public bool IsPlayersTurn(ulong clientId) => CurrentTurnClientId.Value == clientId;

        public void AdvanceTurn()
        {
            if (!IsServer || _order.Count == 0) return;
            _turnIndex = (_turnIndex + 1) % _order.Count;
            CurrentTurnClientId.Value = _order[_turnIndex];
        }

        public void RemoveFromTurnOrder(ulong clientId)
        {
            if (!_order.Contains(clientId)) return;
            int idx = _order.IndexOf(clientId);
            _order.RemoveAt(idx);
            if (_order.Count == 0)
            {
                Phase.Value = GamePhase.GameOver;
                return;
            }
            if (CurrentTurnClientId.Value == clientId)
            {
                // If current player left, don't skip next player
                if (idx <= _turnIndex && _turnIndex > 0) _turnIndex--;
                AdvanceTurn();
            }
        }
        #endregion
    }
}

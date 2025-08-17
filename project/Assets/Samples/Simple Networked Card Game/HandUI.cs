using UnityEngine;

namespace MysteriousGuests.Netcode
{
    /// <summary>Stub to prove the ClientRpc flow. Replace with your real UI.</summary>
    public class HandUI : MonoBehaviour
    {
        public static HandUI Instance { get; private set; }
        private void Awake() => Instance = this;

        public void Refresh(CardId[] cards)
        {
            // TODO: Build UI elements representing player hand
            Debug.Log($"[HandUI] Received {cards.Length} cards: {string.Join(",", cards)}");
        }
    }
}
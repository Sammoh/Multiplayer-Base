using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace MysteriousGuests.Netcode
{
    /// <summary>
    /// Server-side deck + visible card objects. Handles dealing & playing.
    /// </summary>
    public class CardManager : NetworkBehaviour
    {
        public static CardManager Instance { get; private set; }

        [SerializeField] private GameObject cardPrefab;   // Prefab with NetworkObject + Card script
        [SerializeField] private int startingDecks = 1;
        [SerializeField] private Transform tableRoot;     // where played cards go (server side)

        // server-only
        private readonly Stack<CardId> _drawPile = new();
        private readonly List<CardId> _discard = new();

        private void Awake() => Instance ??= this;

        #region Server API
        public void BuildAndShuffle()
        {
            if (!IsServer) return;
            _drawPile.Clear();
            _discard.Clear();

            var arr = CardUtils.BuildStandardDeck(startingDecks);
            Shuffle(arr);
            for (int i = 0; i < arr.Length; i++)
                _drawPile.Push(arr[i]);
        }

        public List<CardId> Draw(int count)
        {
            var drawn = new List<CardId>(count);
            for (int i = 0; i < count && _drawPile.Count > 0; i++)
                drawn.Add(_drawPile.Pop());
            return drawn;
        }

        public void PlayCard(CardId card, ulong fromClient)
        {
            if (!IsServer) return;

            // Spawn visible card on table
            var go = Instantiate(cardPrefab, tableRoot);
            var netObj = go.GetComponent<NetworkObject>();
            var cardComp = go.GetComponent<Card>();

            cardComp.Id.Value = card;
            cardComp.FaceUp.Value = true;

            netObj.Spawn(); // server-owned by default

            _discard.Add(card);
        }
        #endregion

        #region Utils
        private static void Shuffle<T>(IList<T> list)
        {
            var rng = new System.Random();
            for (int i = list.Count - 1; i > 0; i--)
            {
                int k = rng.Next(i + 1);
                (list[i], list[k]) = (list[k], list[i]);
            }
        }
        #endregion
    }
}

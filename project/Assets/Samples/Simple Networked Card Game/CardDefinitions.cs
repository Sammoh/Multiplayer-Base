using System;
using Unity.Netcode;
using UnityEngine;

namespace MysteriousGuests.Netcode
{
    public enum Suit : byte { Clubs, Diamonds, Hearts, Spades }
    public enum Rank : byte { Two = 2, Three, Four, Five, Six, Seven, Eight, Nine, Ten, Jack, Queen, King, Ace }

    /// <summary>Compact card identifier: 6 bits for rank, 2 bits for suit => fits in a byte.</summary>
    [Serializable]
    public struct CardId : INetworkSerializable, IEquatable<CardId>
    {
        public Rank Rank;
        public Suit Suit;

        public CardId(Rank r, Suit s) { Rank = r; Suit = s; }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            byte pack = (byte)(((int)Rank << 2) | ((int)Suit & 0x03));
            if (serializer.IsWriter) serializer.GetFastBufferWriter().WriteValueSafe(pack);
            else
            {
                serializer.GetFastBufferReader().ReadValueSafe(out pack);
                Suit = (Suit)(pack & 0x03);
                Rank = (Rank)(pack >> 2);
            }
        }

        public override int GetHashCode() => ((int)Rank << 2) ^ (int)Suit;
        public bool Equals(CardId other) => Rank == other.Rank && Suit == other.Suit;
        public override bool Equals(object obj) => obj is CardId other && Equals(other);
        public override string ToString() => $"{Rank}{Suit.ToString()[0]}"; // e.g. TenH -> "TenH" shorthand "10H"
    }

    public static class CardUtils
    {
        public static CardId[] BuildStandardDeck(int decks = 1)
        {
            var list = new CardId[52 * decks];
            int i = 0;
            for (int d = 0; d < decks; d++)
            {
                foreach (Suit s in Enum.GetValues(typeof(Suit)))
                foreach (Rank r in Enum.GetValues(typeof(Rank)))
                    if ((int)r >= 2) list[i++] = new CardId(r, s);
            }
            return list;
        }
    }
}
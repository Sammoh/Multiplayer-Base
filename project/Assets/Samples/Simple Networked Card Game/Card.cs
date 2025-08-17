using Unity.Netcode;
using UnityEngine;

namespace MysteriousGuests.Netcode
{
    /// <summary>Networked representation of a visible card (on table / discard etc.).</summary>
    public class Card : NetworkBehaviour
    {
        public NetworkVariable<CardId> Id = new(writePerm: NetworkVariableWritePermission.Server);
        public NetworkVariable<bool> FaceUp = new(writePerm: NetworkVariableWritePermission.Server);

        [SerializeField] private MeshRenderer _renderer; // or SpriteRenderer/UI

        public override void OnNetworkSpawn()
        {
            if (IsServer)
                NetworkObject.DontDestroyWithOwner = true;

            Id.OnValueChanged += (_, _) => ApplyVisual();
            FaceUp.OnValueChanged += (_, _) => ApplyVisual();

            ApplyVisual();
        }

        private void ApplyVisual()
        {
            if (_renderer == null) return;
            // TODO: map CardId to material/texture/sprite
            // For now just tint based on suit
            Color c = FaceUp.Value ? SuitColor(Id.Value.Suit) : Color.black;
            _renderer.material.color = c;
        }

        private static Color SuitColor(Suit s)
        {
            return s switch
            {
                Suit.Clubs => Color.green,
                Suit.Diamonds => Color.red,
                Suit.Hearts => new Color(1, .3f, .3f),
                Suit.Spades => Color.blue,
                _ => Color.white
            };
        }
    }
}
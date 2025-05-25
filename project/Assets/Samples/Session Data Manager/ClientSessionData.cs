using Unity.Netcode;
using UnityEngine;

public class ClientSessionData : INetworkSerializable
{
    public string PlayerId;
    public string DisplayName;
    public int    SelectedCharacter;
    public int    TeamId;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) 
        where T : IReaderWriter
    {
        PlayerId ??= string.Empty;
        DisplayName ??= string.Empty;
        SelectedCharacter = Mathf.Max(SelectedCharacter, 0); // Ensure valid character index
        TeamId = Mathf.Max(TeamId, 0); // Ensure valid team index

        serializer.SerializeValue(ref PlayerId);
        serializer.SerializeValue(ref DisplayName);
        serializer.SerializeValue(ref SelectedCharacter);
        serializer.SerializeValue(ref TeamId);
    }
}
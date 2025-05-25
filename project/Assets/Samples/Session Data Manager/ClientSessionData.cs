using Unity.Netcode;


public class ClientSessionData : INetworkSerializable
{
    public string PlayerId;
    public string DisplayName;
    public int    SelectedCharacter;
    public int    TeamId;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) 
        where T : IReaderWriter
    {
        serializer.SerializeValue(ref PlayerId);
        serializer.SerializeValue(ref DisplayName);
        serializer.SerializeValue(ref SelectedCharacter);
        serializer.SerializeValue(ref TeamId);
    }
}
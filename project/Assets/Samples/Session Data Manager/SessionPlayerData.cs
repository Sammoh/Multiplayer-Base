using System;
using Unity.Netcode;

[Serializable]
public class SessionPlayerData : INetworkSerializable
{
    public string PlayerId;
    public string DisplayName;
    public int SelectedOption;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref PlayerId);
        serializer.SerializeValue(ref DisplayName);
        serializer.SerializeValue(ref SelectedOption);
    }
}
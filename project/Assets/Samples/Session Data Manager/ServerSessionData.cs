using System;
using Unity.Netcode;

public class ServerSessionData : INetworkSerializable, IEquatable<ServerSessionData>
{
    public string SessionName;
    public int MaxPlayers;
    public string MapName;
    public bool AllowSpectators;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref SessionName);
        serializer.SerializeValue(ref MaxPlayers);
        serializer.SerializeValue(ref MapName);
        serializer.SerializeValue(ref AllowSpectators);
    }

    public bool Equals(ServerSessionData other)
    {
        if (other == null) return false;

        return SessionName == other.SessionName &&
               MaxPlayers == other.MaxPlayers &&
               MapName == other.MapName &&
               AllowSpectators == other.AllowSpectators;
    }

    public override bool Equals(object obj)
    {
        return Equals(obj as ServerSessionData);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(SessionName, MaxPlayers, MapName, AllowSpectators);
    }
}
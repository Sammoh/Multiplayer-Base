using System;
using Unity.Netcode;

public struct ServerSessionData : INetworkSerializable, IEquatable<ServerSessionData>
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
        return SessionName == other.SessionName &&
               MaxPlayers == other.MaxPlayers &&
               MapName == other.MapName &&
               AllowSpectators == other.AllowSpectators;
    }

    public override bool Equals(object obj) => obj is ServerSessionData other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(SessionName, MaxPlayers, MapName, AllowSpectators);

    public ServerSessionData(string sessionName = "", int maxPlayers = 0, string mapName = "", bool allowSpectators = false)
    {
        SessionName = sessionName ?? string.Empty;
        MaxPlayers = maxPlayers;
        MapName = mapName ?? string.Empty;
        AllowSpectators = allowSpectators;
    }
}

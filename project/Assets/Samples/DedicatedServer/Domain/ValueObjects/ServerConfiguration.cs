namespace DedicatedServer.Domain.ValueObjects
{
    /// <summary>
    /// Server configuration value object containing network parameters.
    /// Immutable and contains validation logic.
    /// </summary>
    public struct ServerConfiguration
    {
        public string ServerIP { get; }
        public int ServerPort { get; }
        public int QueryPort { get; }
        public int MaxPlayers { get; }
        public string ServerName { get; }

        public ServerConfiguration(string serverIP = "127.0.0.1", int serverPort = 7777, int queryPort = 7787, int maxPlayers = 10, string serverName = null)
        {
            ServerIP = string.IsNullOrWhiteSpace(serverIP) ? "127.0.0.1" : serverIP;
            ServerPort = serverPort <= 0 ? 7777 : serverPort;
            QueryPort = queryPort <= 0 ? 7787 : queryPort;
            MaxPlayers = maxPlayers <= 0 ? 10 : maxPlayers;
            ServerName = string.IsNullOrWhiteSpace(serverName) ? GenerateServerName() : serverName;
        }

        public string ConnectionString => $"{ServerIP}:{ServerPort}";

        private static string GenerateServerName()
        {
            return $"Server-{System.Guid.NewGuid().ToString("N").Substring(0, 8)}";
        }

        public override string ToString()
        {
            return $"Server '{ServerName}' @ {ConnectionString} (Query: {QueryPort}, Max: {MaxPlayers})";
        }
    }
}
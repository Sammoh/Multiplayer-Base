using System;
using System.Text;
using DedicatedServer.Domain.ValueObjects;

namespace DedicatedServer.Domain.Entities
{
    /// <summary>
    /// Core user domain entity containing user identification and preferences.
    /// Pure domain logic with no external dependencies.
    /// </summary>
    public class User
    {
        public string Name { get; set; }
        public string AuthenticationId { get; set; }
        public ulong NetworkId { get; set; }
        public GameSession GamePreferences { get; set; }

        public User(string name, string authenticationId, ulong networkId = 0)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            AuthenticationId = authenticationId ?? throw new ArgumentNullException(nameof(authenticationId));
            NetworkId = networkId;
            GamePreferences = new GameSession();
        }

        public void UpdateGamePreferences(Map map, GameMode gameMode, GameQueue gameQueue, string password = null)
        {
            GamePreferences.Map = map;
            GamePreferences.GameMode = gameMode;
            GamePreferences.GameQueue = gameQueue;
            GamePreferences.Password = password;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"User: {Name}");
            sb.AppendLine($"- Auth ID: {AuthenticationId}");
            sb.AppendLine($"- Network ID: {NetworkId}");
            sb.AppendLine($"- Preferences: {GamePreferences}");
            return sb.ToString();
        }
    }
}
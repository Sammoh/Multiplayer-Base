using System;
using DedicatedServer.Domain.Entities;
using DedicatedServer.Domain.ValueObjects;

namespace Matchplay.Shared
{
    /// <summary>
    /// Backward compatibility adapter for GameInfo.
    /// Delegates to the new GameSession entity.
    /// </summary>
    [Obsolete("Use DedicatedServer.Domain.Entities.GameSession instead", false)]
    public class GameInfoAdapter
    {
        private readonly GameSession _gameSession;

        public GameInfoAdapter()
        {
            _gameSession = new GameSession();
        }

        public GameInfoAdapter(GameSession gameSession)
        {
            _gameSession = gameSession ?? new GameSession();
        }

        // Maintain old property names for compatibility
        public Map map
        {
            get => _gameSession.Map;
            set => _gameSession.Map = value;
        }

        public GameMode gameMode
        {
            get => _gameSession.GameMode;
            set => _gameSession.GameMode = value;
        }

        public GameQueue gameQueue
        {
            get => _gameSession.GameQueue;
            set => _gameSession.GameQueue = value;
        }

        public string gamePassword
        {
            get => _gameSession.Password;
            set => _gameSession.Password = value;
        }

        public int MaxUsers
        {
            get => _gameSession.MaxUsers;
            set => _gameSession.MaxUsers = value;
        }

        public string ToSceneName => _gameSession.GetSceneName();

        public string ToMultiplayQueue() => _gameSession.GetMultiplayQueueName();

        public static GameQueue ToGameQueue(string multiplayQueue) => GameSession.ParseMultiplayQueue(multiplayQueue);

        // Implicit conversion to new type
        public static implicit operator GameSession(GameInfoAdapter adapter) => adapter._gameSession;
        public static implicit operator GameInfoAdapter(GameSession gameSession) => new GameInfoAdapter(gameSession);

        public override string ToString() => _gameSession.ToString();
    }
}
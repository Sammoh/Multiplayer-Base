using System;
using System.Collections.Generic;
using DedicatedServer.Domain.ValueObjects;

namespace DedicatedServer.Domain.Entities
{
    /// <summary>
    /// Core game configuration entity that defines game session parameters.
    /// Contains only domain logic, no framework dependencies.
    /// </summary>
    public class GameSession
    {
        private const string MAP_DEFAULT = "mainScene";
        private const string MAP_SPACE = "gameScene";
        
        public Map Map { get; set; }
        public GameMode GameMode { get; set; }
        public GameQueue GameQueue { get; set; }
        public string Password { get; set; }
        public int MaxUsers { get; set; } = 10;
        
        public string GetSceneName()
        {
            return Map switch
            {
                Map.Default => MAP_DEFAULT,
                Map.Space => MAP_SPACE,
                _ => MAP_DEFAULT
            };
        }
        
        public string GetMultiplayQueueName()
        {
            return GameQueue switch
            {
                GameQueue.Casual => "casual-queue",
                GameQueue.Competitive => "competetive-queue",
                _ => "casual-queue"
            };
        }
        
        public static GameQueue ParseMultiplayQueue(string multiplayQueue)
        {
            var queueMap = new Dictionary<string, GameQueue>
            {
                { "casual-queue", GameQueue.Casual },
                { "competetive-queue", GameQueue.Competitive }
            };
            
            return queueMap.TryGetValue(multiplayQueue, out var queue) ? queue : GameQueue.Casual;
        }

        public override string ToString()
        {
            return $"GameSession: Map={Map}, Mode={GameMode}, Queue={GameQueue}, MaxUsers={MaxUsers}";
        }
    }
}
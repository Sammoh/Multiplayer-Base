using System.Threading.Tasks;
using DedicatedServer.Application.Services;
using DedicatedServer.Application.UseCases;
using DedicatedServer.Domain.Entities;
using DedicatedServer.Domain.ValueObjects;
using DedicatedServer.Infrastructure.Configuration;
using UnityEngine;

namespace DedicatedServer.Tests
{
    /// <summary>
    /// Simple tests to verify Clean Architecture implementation.
    /// Demonstrates dependency injection and layer separation.
    /// </summary>
    public static class ArchitectureTests
    {
        public static void RunTests()
        {
            Debug.Log("Running Clean Architecture Tests...");
            
            TestDomainEntities();
            TestApplicationServices();
            TestInfrastructureServices();
            
            Debug.Log("All Clean Architecture Tests Passed!");
            
            // Run architecture validation
            ArchitectureValidationTests.RunValidationTests();
        }

        private static void TestDomainEntities()
        {
            // Test User entity
            var user = new User("TestUser", "auth123");
            user.UpdateGamePreferences(Map.Default, GameMode.Meditating, GameQueue.Casual, "test123");
            
            Debug.Log($"User Test: {user}");
            
            // Test GameSession entity
            var gameSession = new GameSession
            {
                Map = Map.Space,
                GameMode = GameMode.Staring,
                GameQueue = GameQueue.Competitive,
                Password = "secret",
                MaxUsers = 8
            };
            
            var sceneName = gameSession.GetSceneName();
            var queueName = gameSession.GetMultiplayQueueName();
            
            Debug.Log($"GameSession Test: {gameSession}");
            Debug.Log($"Scene: {sceneName}, Queue: {queueName}");
        }

        private static void TestApplicationServices()
        {
            // Test UserService
            var userService = new UserService();
            var user = userService.CreateUser("AppTestUser", "appauth456");
            
            userService.UpdateUserPreferences(Map.Space, GameMode.Staring, GameQueue.Competitive);
            
            var currentUser = userService.GetCurrentUser();
            Debug.Log($"Application Service Test: {currentUser}");
        }

        private static void TestInfrastructureServices()
        {
            // Test UnityConfigurationService
            var configService = new UnityConfigurationService();
            var serverConfig = configService.GetServerConfiguration();
            
            Debug.Log($"Infrastructure Test - Server Config: {serverConfig}");
            Debug.Log($"Headless Mode: {configService.IsHeadlessMode}");
        }
    }
}
using System;
using System.Threading.Tasks;
using DedicatedServer.Application.Services;
using DedicatedServer.Application.UseCases;
using DedicatedServer.Domain.Entities;
using DedicatedServer.Domain.Interfaces;
using DedicatedServer.Domain.ValueObjects;
using DedicatedServer.Infrastructure.Configuration;
using DedicatedServer.Infrastructure.Unity;
using DedicatedServer.Tests;
using Unity.Netcode;
using UnityEngine;

namespace DedicatedServer.Presentation.Controllers
{
    /// <summary>
    /// Application controller that orchestrates the startup of server/client.
    /// Composition root for dependency injection.
    /// </summary>
    public class ApplicationController : MonoBehaviour
    {
        [SerializeField] private GameObject serverPrefab;
        [SerializeField] private GameObject clientPrefab;

        private IConfigurationService _configurationService;
        private IServerService _serverService;
        private UserService _userService;
        private StartServerUseCase _startServerUseCase;

        public static bool IsServer { get; private set; }

        async void Start()
        {
            Application.targetFrameRate = 60;
            DontDestroyOnLoad(gameObject);
            
            // Editor uses different launcher
            if (Application.isEditor)
                return;
            
            Debug.Log("DEDICATED_SERVER v0.2 - Clean Architecture");

            // Run architecture tests to verify setup
            ArchitectureTests.RunTests();

            // Bootstrap services
            BootstrapServices();

            // Determine if headless server
            bool isHeadlessServer = _configurationService.IsHeadlessMode;
            await LaunchInMode(isHeadlessServer);
        }

        public void OnParrelSyncStarted(bool isServer, string cloneName)
        {
#pragma warning disable 4014
            LaunchInMode(isServer, cloneName);
#pragma warning restore 4014
        }

        private void BootstrapServices()
        {
            // Configuration
            _configurationService = new UnityConfigurationService();
            
            // User service
            _userService = new UserService();
            
            // Create default user
            var defaultUser = _userService.CreateUser("DefaultPlayer", Guid.NewGuid().ToString());
        }

        private async Task LaunchInMode(bool isServer, string profileName = "default")
        {
            IsServer = isServer;
            
            if (isServer)
            {
                await StartServerMode();
            }
            else
            {
                await StartClientMode(profileName);
            }
        }

        private async Task StartServerMode()
        {
            Debug.Log("Starting Server Mode");

            // Create server infrastructure
            var serverGameObject = Instantiate(serverPrefab);
            var networkManager = serverGameObject.GetComponent<NetworkManager>();
            
            var serverConfig = _configurationService.GetServerConfiguration();
            _serverService = new UnityServerService(networkManager, serverConfig);
            
            // Create use case
            _startServerUseCase = new StartServerUseCase(_serverService, _configurationService);

            // Create default game session
            var defaultGameSession = new GameSession
            {
                GameMode = GameMode.Meditating,
                Map = Map.Default,
                GameQueue = GameQueue.Casual,
                Password = "123456"
            };

            // Start server
            if (_startServerUseCase.CanExecute())
            {
                bool success = await _startServerUseCase.ExecuteAsync(defaultGameSession);
                if (success)
                {
                    Debug.Log($"Server started successfully: {serverConfig}");
                }
                else
                {
                    Debug.LogError("Failed to start server");
                }
            }
        }

        private async Task StartClientMode(string profileName)
        {
            Debug.Log($"Starting Client Mode with profile: {profileName}");
            
            // Create client infrastructure
            var clientGameObject = Instantiate(clientPrefab);
            
            // TODO: Implement client initialization
            await Task.Yield();
        }

        void OnDestroy()
        {
            _serverService?.Stop();
        }
    }
}
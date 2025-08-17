using System;
using System.Threading.Tasks;
using DedicatedServer.Domain.Interfaces;
using Unity.Services.Core;
using UnityEngine;

namespace DedicatedServer.Presentation.MonoBehaviours
{
    /// <summary>
    /// Unity MonoBehaviour wrapper for server management.
    /// Provides Unity lifecycle integration for server services.
    /// </summary>
    public class ServerManager : MonoBehaviour
    {
        private IServerService _serverService;
        
        public static ServerManager Instance { get; private set; }

        public IServerService ServerService
        {
            get
            {
                if (_serverService != null)
                    return _serverService;
                
                Debug.LogError("Server service is not initialized. Call InitializeAsync first.");
                return null;
            }
        }

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public async Task InitializeAsync()
        {
            try
            {
                await UnityServices.InitializeAsync();
                Debug.Log("Unity Services initialized for server.");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to initialize Unity Services: {ex.Message}");
            }
        }

        public void SetServerService(IServerService serverService)
        {
            _serverService = serverService ?? throw new ArgumentNullException(nameof(serverService));
        }

        void OnDestroy()
        {
            _serverService?.Stop();
            if (Instance == this)
            {
                Instance = null;
            }
        }
    }
}
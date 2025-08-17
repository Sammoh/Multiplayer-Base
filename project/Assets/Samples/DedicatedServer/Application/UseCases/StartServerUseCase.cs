using System;
using System.Threading.Tasks;
using DedicatedServer.Domain.Entities;
using DedicatedServer.Domain.Interfaces;
using DedicatedServer.Domain.ValueObjects;

namespace DedicatedServer.Application.UseCases
{
    /// <summary>
    /// Application use case for starting a game server.
    /// Orchestrates domain services without framework dependencies.
    /// </summary>
    public class StartServerUseCase
    {
        private readonly IServerService _serverService;
        private readonly IConfigurationService _configurationService;

        public StartServerUseCase(IServerService serverService, IConfigurationService configurationService)
        {
            _serverService = serverService ?? throw new ArgumentNullException(nameof(serverService));
            _configurationService = configurationService ?? throw new ArgumentNullException(nameof(configurationService));
        }

        public async Task<bool> ExecuteAsync(GameSession gameSession)
        {
            try
            {
                if (_serverService.IsRunning)
                {
                    return false; // Already running
                }

                await _serverService.StartAsync(gameSession);
                return _serverService.IsRunning;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool CanExecute()
        {
            return !_serverService.IsRunning && _configurationService.IsHeadlessMode;
        }
    }
}
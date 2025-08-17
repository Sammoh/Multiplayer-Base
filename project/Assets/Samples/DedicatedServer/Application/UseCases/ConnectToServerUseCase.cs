using System;
using System.Threading.Tasks;
using DedicatedServer.Domain.Entities;
using DedicatedServer.Domain.Interfaces;

namespace DedicatedServer.Application.UseCases
{
    /// <summary>
    /// Application use case for client connection to server.
    /// Handles connection logic without framework dependencies.
    /// </summary>
    public class ConnectToServerUseCase
    {
        private readonly IClientNetworkService _networkService;

        public ConnectToServerUseCase(IClientNetworkService networkService)
        {
            _networkService = networkService ?? throw new ArgumentNullException(nameof(networkService));
        }

        public async Task<bool> ExecuteAsync(string serverIP, int port, User user)
        {
            try
            {
                if (_networkService.IsConnected)
                {
                    _networkService.Disconnect();
                }

                return await _networkService.ConnectAsync(serverIP, port);
            }
            catch (Exception)
            {
                return false;
            }
        }

        public void Disconnect()
        {
            if (_networkService.IsConnected)
            {
                _networkService.Disconnect();
            }
        }

        public bool CanExecute()
        {
            return !_networkService.IsConnected;
        }
    }
}
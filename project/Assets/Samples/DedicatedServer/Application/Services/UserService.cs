using System;
using DedicatedServer.Domain.Entities;
using DedicatedServer.Domain.Interfaces;

namespace DedicatedServer.Application.Services
{
    /// <summary>
    /// Application service for user management.
    /// Provides business logic for user operations.
    /// </summary>
    public class UserService
    {
        private User _currentUser;

        public event Action<User> UserChanged;

        public User GetCurrentUser()
        {
            return _currentUser;
        }

        public void SetCurrentUser(User user)
        {
            _currentUser = user ?? throw new ArgumentNullException(nameof(user));
            UserChanged?.Invoke(_currentUser);
        }

        public User CreateUser(string name, string authId)
        {
            var user = new User(name, authId);
            SetCurrentUser(user);
            return user;
        }

        public void UpdateUserPreferences(Domain.ValueObjects.Map map, Domain.ValueObjects.GameMode gameMode, Domain.ValueObjects.GameQueue gameQueue, string password = null)
        {
            if (_currentUser != null)
            {
                _currentUser.UpdateGamePreferences(map, gameMode, gameQueue, password);
                UserChanged?.Invoke(_currentUser);
            }
        }
    }
}
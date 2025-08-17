using System;
using System.Threading.Tasks;
using LobbyRelaySample;
using Matchplay.Shared; // for PlayerStatus

namespace Matchplay.Client.Session
{
    public enum SessionState
    {
        Lobby   = 0,
        InGame  = 1
    }

    /// <summary>
    /// Shape of any session system (lobby-based or session service).
    /// Keep this tiny so ClientGameManager stays decoupled.
    /// </summary>
    public interface ISessionController : IDisposable
    {
        // Observability
        event Action<SessionState>    SessionStateChanged;
        event Action<bool>            LockedChanged;
        event Action<PlayerStatus>    LocalUserStatusChanged;

        // Snapshot getters
        SessionState CurrentState { get; }
        bool         Locked       { get; }
        bool         IsHost       { get; }
        string       Code         { get; }  // e.g., Lobby code or Session join code

        // Lifecycle
        Task InitializeAsync(string profileName);

        // Mutations (no-op for non-host when appropriate)
        void SetState(SessionState state);
        void SetLocked(bool locked);
        void SetLocalUserStatus(PlayerStatus status);
        void ResetLocalUser();  // clears ready/status/etc (host or client)

        // Network sync / housekeeping
        void Push();            // send local/session data upstream
        Task LeaveAsync();      // leave session
    }
}
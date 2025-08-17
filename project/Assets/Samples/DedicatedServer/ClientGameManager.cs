using System;
using System.Threading.Tasks;
// using LobbyRelaySample;  // REMOVED
using Matchplay.Server;
using Matchplay.Shared;
using Matchplay.Shared.Tools;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

[Flags]
public enum GameState
{
    Menu     = 1,
    Lobby    = 2,   // kept for compatibility with external code; unused here
    JoinMenu = 4,
}

public struct GameSettingsStruct
{
    public int CardsPerRound;
    public int RoundCount;
    public int GameCardPoolSize => CardsPerRound * 2 * RoundCount;
    public int MaxPlayers;
    public string DefaultDeck;

    public GameSettingsStruct(int cardsPerRound, int roundCount, int maxPlayers, string defaultDeck)
    {
        CardsPerRound = cardsPerRound;
        RoundCount    = roundCount;
        MaxPlayers    = maxPlayers;
        DefaultDeck   = defaultDeck;
    }
}

namespace Matchplay.Client
{
    /// <summary>
    /// Orchestrates authentication, matchmaking, networking, and scene flow
    /// for a Matchplay client instance. (Lobby removed)
    /// </summary>
    public class ClientGameManager : IDisposable
    {
        public Action<GameState> onGameStateChanged;
        public GameState LocalGameState { get; private set; }

        #region Events ──────────────────────────────────────────────────────
        public event Action<Matchplayer> MatchPlayerSpawned;
        public event Action<Matchplayer> MatchPlayerDespawned;
        #endregion

        #region Fields & Properties ─────────────────────────────────────────

        private GameSettingsStruct gamesettings = new GameSettingsStruct
        (
            cardsPerRound: 5,
            roundCount:    3,
            maxPlayers:    8,
            defaultDeck:   "DefaultDeck"
        );

        public int MaxPlayers        => gamesettings.MaxPlayers;
        public string DefaultDeck    => gamesettings.DefaultDeck;
        public int CardsPerRound     => gamesettings.CardsPerRound;

        public MatchplayUser           User          { get; private set; }
        public MatchplayNetworkClient  NetworkClient { get; private set; }
        public MatchplayMatchmaker     Matchmaker    { get; private set; }

        public string ProfileName { get; }

        // Internal state (non-lobby)
        private bool _useLocalServer;

        #endregion

        #region Constructor ────────────────────────────────────────────────

        public ClientGameManager(string profileName = "default")
        {
            ProfileName = profileName;
            User        = new MatchplayUser();

#if UNITY_EDITOR
            // Maintain MPPM profile override behavior; unrelated to lobbies.
            // var mppmTag = Unity.Multiplayer.Playmode.CurrentPlayer.ReadOnlyTags();
            // ProfileName = mppmTag[0];
#endif

            Debug.Log($"Beginning with new Profile: {ProfileName}");

            _ = InitAsync(() =>
            {
                // Initial game state to Menu and load main menu scene
                LocalGameState = GameState.Menu;
                ToMainMenu();
            });
        }

        #endregion

        #region Initialization ─────────────────────────────────────────────

        /// <summary>
        /// Bootstraps Unity Services, performs authentication, and sets up
        /// matchmaking / networking clients.
        /// </summary>
        public async Task InitAsync(Action onComplete)
        {
            var initOptions = new InitializationOptions()
                .SetProfile($"{ProfileName}{LocalProfileTool.LocalProfileSuffix}");

            await UnityServices.InitializeAsync(initOptions);
            Debug.Log("Unity Services initialized.");

            NetworkClient = new MatchplayNetworkClient();
            Matchmaker    = new MatchplayMatchmaker();

            var authResult = await AuthenticationWrapper.DoAuth();
            User.AuthId    = authResult == AuthState.Authenticated
                ? AuthenticationWrapper.PlayerID()
                : Guid.NewGuid().ToString(); // offline fallback

            Debug.Log($"Auth state: {authResult} | PlayerID: {User.AuthId}");
            onComplete?.Invoke();
        }

        #endregion

        #region Connection & Networking ────────────────────────────────────

        public void BeginConnection(string ip, int port)
        {
            Debug.Log($"Starting NetworkClient @ {ip}:{port}\nWith : {User}");
            NetworkClient.StartClient(ip, port);
        }

        public void Disconnect() => NetworkClient?.DisconnectClient();

        #endregion

        #region Matchmaking ────────────────────────────────────────────────

        public async Task MatchmakeAsync(Action<MatchmakerPollingResult> onMatchmakerResponse = null)
        {
            if (Matchmaker.IsMatchmaking)
            {
                Debug.LogWarning("Already matchmaking—please wait or cancel.");
                return;
            }

            var result = await GetMatchAsync();
            onMatchmakerResponse?.Invoke(result);
        }

        public async Task CancelMatchmaking() => await Matchmaker.CancelMatchmaking();

        private async Task<MatchmakerPollingResult> GetMatchAsync()
        {
            Debug.Log($"Beginning matchmaking with {User}");

            var poll = await Matchmaker.Matchmake(User.Data);

            if (poll.result == MatchmakerPollingResult.Success)
            {
                BeginConnection(poll.ip, poll.port);
            }
            else
            {
                Debug.LogWarning($"{poll.result} : {poll.resultMessage}");
            }

            Debug.LogError($"Matchmaking Result: {poll.resultMessage}");
            Debug.LogError($"Matchmaking Server: {poll.ip}:{poll.port}");

            return poll.result;
        }

        private void OnMatchMade(MatchmakerPollingResult result)
        {
            switch (result)
            {
                case MatchmakerPollingResult.Success:
                    // Start-of-game hooks can go here (non-lobby)
                    break;
                case MatchmakerPollingResult.TicketCreationError:
                case MatchmakerPollingResult.TicketCancellationError:
                case MatchmakerPollingResult.TicketRetrievalError:
                case MatchmakerPollingResult.MatchAssignmentError:
                    // Handle failure states/logging here
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(result), result, null);
            }
        }

        #endregion

        #region User Preference Mutators ───────────────────────────────────

        public void SetGameMode(GameMode gameMode)   => User.GameModePreferences = gameMode;
        public void SetGameMap(Map map)              => User.MapPreferences      = map;
        public void SetGameQueue(GameQueue queue)    => User.QueuePreference     = queue;
        public void SetGamePassword(string password) => User.QueuePassword       = password;

        #endregion

        #region Scene & Player Management ─────────────────────────────────

        public void ToMainMenu()
        {
            string sceneName = ClientSingleton.Instance.loadSceneName;
            SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
        }

        public void AddMatchPlayer(Matchplayer player)    => MatchPlayerSpawned?.Invoke(player);
        public void RemoveMatchPlayer(Matchplayer player) => MatchPlayerDespawned?.Invoke(player);

        #endregion

        #region IDisposable & App Lifecycle ───────────────────────────────

        public void Dispose()
        {
            NetworkClient?.Dispose();
            Matchmaker?.Dispose();
        }

        public void ExitGame()
        {
            Dispose();
            Application.Quit();
        }

        #endregion

        #region State Flow (Non-Lobby) ─────────────────────────────────────

        public void EndGame()
        {
            // No lobby state transitions; return to Menu by default.
            UIChangeMenuState(GameState.Menu);
        }

        public void UIChangeMenuState(GameState state)
        {
            SetGameState(state);
        }

        internal void SetGameState(GameState state)
        {
            LocalGameState = state;
            Debug.Log($"Switching Game State to : {LocalGameState}");
            onGameStateChanged?.Invoke(LocalGameState);
        }

        public void BeginGame()
        {
            // Hook your non-lobby "game start" flow here if needed.
        }

        public void ClientQuitGame()
        {
            EndGame();
            Debug.LogError("Client Quit Game");
            // m_setupInGame?.OnGameEnd(); // if you keep a separate in-game setup, call it there.
        }

        /// <summary>
        /// Called when any client-side countdown (e.g., ready-up) finishes.
        /// Kicks off connection to local server or matchmaking—no lobby involved.
        /// </summary>
        public async void FinishedCountDown()
        {
            var clientManager = ClientSingleton.Instance.Manager;

            if (_useLocalServer)
            {
                var localIP = ApplicationData.IP();
                var port    = ApplicationData.Port();
                clientManager.BeginConnection(localIP, port);
                Debug.LogError($"Starting local server @ {localIP}:{port} with user: {User}");
                return;
            }

            await clientManager.MatchmakeAsync(OnMatchMade);
        }

        #endregion
    }
}

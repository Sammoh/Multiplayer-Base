using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.Services.Multiplayer;
#if AUTH_PACKAGE_PRESENT
using Unity.Services.Authentication;
#endif
using Unity.Services.Vivox;
using UnityEngine;
using UnityEngine.UI;

public class SignInUI : MonoBehaviour
{
    public Button LoginButton;
    public InputField DisplayNameInput;
    public GameObject LoginUIObject;

    private ISession _sessionIdKey;

    void Start()
    {
        // DisplayNameInput.onValueChanged.AddListener(InputAccountName);
        // LoginButton.onClick.AddListener(async () => { await LoginToVivoxService(); });
        
        // Move the sign in logic to use the lobby service
        // AuthenticationService.Instance.SignedIn += async () =>
        // {
        //     var info = await AuthenticationService.Instance.GetPlayerInfoAsync();
        //     var displayName = info.Username;
        //     await VivoxManager.Instance.InitializeAsync(displayName);
        //     await VivoxService.Instance.LoginAsync(new LoginOptions()
        //     {
        //         DisplayName = displayName,
        //         ParticipantUpdateFrequency = ParticipantPropertyUpdateFrequency.TenPerSecond
        //     });
        //     TextSampleUIManager.Instance.SignedInPlayerDisplayName = displayName;
        //     TextSampleUIManager.Instance.ShowConversationsUI();
        // };
        
        VivoxService.Instance.LoggedIn += async () =>
        {
            Debug.Log("Logged in Vivox");
            var info = await AuthenticationService.Instance.GetPlayerInfoAsync();
            TextSampleUIManager.Instance.SignedInPlayerDisplayName = info.Username;
            TextSampleUIManager.Instance.ShowConversationsUI();
        };
    }
    
    // todo create new properties that include vivox session id
    public void Evt_OnLobbySessionCreated(ISession sessionId)
    {
        _sessionIdKey = sessionId;
        
        // USE THE SESSION ID TO GET THE LOBBY CODE
        var lobbyCode = sessionId.Code;

        var msg = $"New lobby session created: {lobbyCode}";
        var sessions = MultiplayerService.Instance.Sessions;

        // log all sessions
        // foreach (var session in sessions)
        // {
        //     var sessionIdKey = session.Key;
        //     var sessionIdValue = session.Value;
        //     msg += $"\nSession ID: {sessionIdKey}";
        //
        // }
        
        // get the first session in dictionary
        var session = sessions.FirstOrDefault().Value;
        
        Debug.Log(msg);
        
        // GET THE LOBBY ID
        var lobbyId = session.Id;
        // GET THE LOBBY NAME
        var lobbyName = session.Name;
        // GET THE LOBBY TYPE
        var lobbyType = session.Type;
            
        msg += $"\nLobby ID: {lobbyId}\nLobby Name: {lobbyName}\nLobby Type: {lobbyType}\n";
            
        // GET THE LOBBY PROPERTIES
        // var lobbyProperties = session.Properties;
        // log all properties   
        // foreach (var property in lobbyProperties)
        // {
        //     msg += $"Key: {property.Key} / Value: {property.Value}\n";
        // }
        
        Debug.Log(msg);
        
        CreateNetworkedHostData(lobbyId, lobbyName, lobbyType, lobbyCode);
    }

    private void CreateNetworkedHostData(string lobbyId, string lobbyName, string lobbyType, string lobbyCode)
    {
        Debug.LogError("TODO: UPDATE LOBBY DATA");
    }

    void OnEnable()
    {
        LoginUIObject.SetActive(true);
        DisplayNameInput.ActivateInputField();
        DisplayNameInput.Select();
        LoginButton.interactable = !string.IsNullOrEmpty(DisplayNameInput.text);
    }

    void OnDisable()
    {
        LoginUIObject.SetActive(false);
    }

    void InputAccountName(string arg0)
    {
        LoginButton.interactable = !string.IsNullOrEmpty(arg0);
    }

    async Task LoginToVivoxService()
    {
        var userDisplayName = DisplayNameInput.text;
        LoginButton.interactable = false;
        var loginOptions = new LoginOptions()
        {
            DisplayName = DisplayNameInput.text,
            ParticipantUpdateFrequency = ParticipantPropertyUpdateFrequency.TenPerSecond
        };

        try
        {
            await TextSampleUIManager.Instance.RunTaskWithLoadingUI(VivoxManager.Instance.InitializeAsync(DisplayNameInput.text));
            await TextSampleUIManager.Instance.RunTaskWithLoadingUI(VivoxService.Instance.LoginAsync(loginOptions));
            TextSampleUIManager.Instance.SignedInPlayerDisplayName = userDisplayName;
            TextSampleUIManager.Instance.ShowConversationsUI();
        }
        catch (Exception e)
        {
#if AUTH_PACKAGE_PRESENT
            AuthenticationService.Instance.SignOut();
#endif
            throw e;
        }
    }
}

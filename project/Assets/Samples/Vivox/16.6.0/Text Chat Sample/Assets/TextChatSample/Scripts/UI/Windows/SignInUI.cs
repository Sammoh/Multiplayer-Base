using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
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
            Debug.Log("Logged in vivox");
            var info = await AuthenticationService.Instance.GetPlayerInfoAsync();
            
            // Set a value in the lobby service to store the vivox code
            // await AuthenticationService.Instance.SetPlayerInfoAsync(new PlayerInfo()
            
            TextSampleUIManager.Instance.SignedInPlayerDisplayName = info.Username;
            TextSampleUIManager.Instance.ShowConversationsUI();
        };
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

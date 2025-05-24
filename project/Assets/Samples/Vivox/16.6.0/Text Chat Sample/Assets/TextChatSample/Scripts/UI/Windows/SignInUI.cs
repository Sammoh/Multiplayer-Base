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
        VivoxService.Instance.LoggedIn += async () =>
        {
            Debug.Log("Logged in Vivox");
            var info = await AuthenticationService.Instance.GetPlayerInfoAsync();
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

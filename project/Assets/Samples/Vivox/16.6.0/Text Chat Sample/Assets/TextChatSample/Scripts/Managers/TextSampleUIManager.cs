using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Vivox;
using UnityEngine;

/// <summary>
/// Manages Vivox Text Chat sample UI, including authentication screens,
/// conversation selection, and message sending.
/// </summary>
public class TextSampleUIManager : MonoBehaviour
{
    public static TextSampleUIManager Instance;

    /// <summary>
    /// Event triggered when the local user sends a direct message.
    /// </summary>
    public Action<string> OnDMSentFromLocalUser;

    public const string CachedConversationTypeKey = "ActiveConversationType";

    public GameObject SignInScreenUI;
    public GameObject ActiveConversationUI;
    public GameObject ConversationListUI;
    public GameObject ChannelRosterUI;
    public GameObject ProfilePaneUI;
    public GameObject LoadingSpinnerUI;

    [HideInInspector] public ConversationType CurrentConversationParadigm;
    [HideInInspector] public string SignedInPlayerDisplayName;
    [HideInInspector] public string ActiveChannelConversationId = string.Empty;
    [HideInInspector] public string ActiveDMConversationId = string.Empty;

    void Awake()
    {
        Instance = this;
        LoadPrefs();
        CloseAllMenus();
    }

    void Start()
    {
        VivoxService.Instance.LoggedOut += ShowSignInScreenUI;
        // SignInScreenUI.SetActive(true);
    }

    private void OnDestroy()
    {
    }

    /// <summary>Displays the sign-in UI panel and hides all others.</summary>
    public void ShowSignInScreenUI()
    {
        CloseAllMenus();
        SignInScreenUI.SetActive(true);
    }

    /// <summary>Displays the active conversation panel.</summary>
    public void ShowActiveConversationUI()
    {
        CloseAllMenus();
        ActiveConversationUI.SetActive(true);
    }

    /// <summary>Displays the conversation list panel.</summary>
    public void ShowConversationsUI()
    {
        CloseAllMenus();
        ConversationListUI.SetActive(true);
    }

    /// <summary>Displays the channel roster panel.</summary>
    public void ShowChannelRosterUI()
    {
        CloseAllMenus();
        ChannelRosterUI.SetActive(true);
    }

    /// <summary>
    /// Shows or hides the profile panel.
    /// </summary>
    /// <param name="doShow">True to show the profile panel; false to hide it.</param>
    public void ShowProfilePaneUI(bool doShow)
    {
        ProfilePaneUI.SetActive(doShow);
    }

    /// <summary>Disables all major UI panels.</summary>
    void CloseAllMenus()
    {
        SignInScreenUI.SetActive(false);
        ActiveConversationUI.SetActive(false);
        ConversationListUI.SetActive(false);
        ChannelRosterUI.SetActive(false);
        ProfilePaneUI.SetActive(false);
    }

    /// <summary>Forces refresh of the conversation list panel.</summary>
    void RefreshConversationUIView()
    {
        ConversationListUI.SetActive(false);
        ConversationListUI.SetActive(true);
        ProfilePaneUI.SetActive(false);
    }

    /// <summary>
    /// Switches the current conversation context and refreshes the view.
    /// </summary>
    /// <param name="conversationContext">The new conversation type (Channel or DM).</param>
    public void SwitchConversationContext(ConversationType conversationContext)
    {
        if (CurrentConversationParadigm == conversationContext)
            return;

        CurrentConversationParadigm = conversationContext;
        SaveConversationContextPreference(conversationContext);
        RefreshConversationUIView();
    }

    /// <summary>
    /// Saves the preferred conversation type to PlayerPrefs.
    /// </summary>
    /// <param name="conversationType">Conversation type to save (Channel or DM).</param>
    public void SaveConversationContextPreference(ConversationType conversationType)
    {
        switch (conversationType)
        {
            case ConversationType.ChannelConversation:
                PlayerPrefs.SetString(CachedConversationTypeKey, "Channel");
                break;
            case ConversationType.DirectedMessageConversation:
                PlayerPrefs.SetString(CachedConversationTypeKey, "DM");
                break;
        }

        PlayerPrefs.Save();
    }

    /// <summary>
    /// Joins a conversation based on the current conversation context.
    /// </summary>
    /// <param name="conversationId">The ID of the channel or player to join.</param>
    public async Task JoinConversation(string conversationId)
    {
        ConversationListUI.SetActive(false);
        ActiveConversationUI.SetActive(true);

        switch (CurrentConversationParadigm)
        {
            case ConversationType.ChannelConversation:
                await RunTaskWithLoadingUI(JoinChannelConversation(conversationId));
                break;

            case ConversationType.DirectedMessageConversation:
                await JoinDMConversation(conversationId);
                break;
        }
    }

    /// <summary>
    /// Leaves all active conversations and returns to the conversation list.
    /// </summary>
    public async Task CloseConversation()
    {
        await RunTaskWithLoadingUI(VivoxService.Instance.LeaveAllChannelsAsync());
        ConversationListUI.SetActive(true);
        ActiveConversationUI.SetActive(false);
    }

    /// <summary>
    /// Executes a task while showing a loading spinner.
    /// </summary>
    /// <param name="task">The task to run with loading UI.</param>
    public async Task RunTaskWithLoadingUI(Task task)
    {
        LoadingSpinnerUI.SetActive(true);

        try
        {
            await task;
        }
        catch (Exception e)
        {
            throw e;
        }
        finally
        {
            LoadingSpinnerUI.SetActive(false);
        }
    }

    /// <summary>
    /// Copies a string to the user's system clipboard.
    /// </summary>
    /// <param name="text">Text to copy.</param>
    public void CopyToClipboard(string text)
    {
        GUIUtility.systemCopyBuffer = text;
    }

    /// <summary>
    /// Joins a Vivox group channel and initializes the UI.
    /// </summary>
    /// <param name="channelName">The name of the channel to join.</param>
    async Task JoinChannelConversation(string channelName)
    {
        try
        {
            await VivoxService.Instance.JoinGroupChannelAsync(channelName, ChatCapability.TextOnly);
            ActiveChannelConversationId = channelName;

            await ActiveConversationUI
                .GetComponent<ConversationUI>()
                .SetupConversation(channelName, CurrentConversationParadigm);
        }
        catch (Exception)
        {
            throw;
        }
    }

    /// <summary>
    /// Joins a direct message conversation and initializes the UI.
    /// </summary>
    /// <param name="playerId">The ID of the player to DM.</param>
    async Task JoinDMConversation(string playerId)
    {
        ActiveDMConversationId = playerId;

        await ActiveConversationUI
            .GetComponent<ConversationUI>()
            .SetupConversation(playerId, CurrentConversationParadigm);
    }

    /// <summary>
    /// Sends a text message through the current conversation context.
    /// </summary>
    /// <param name="message">The message content to send.</param>
    public async Task SendVivoxMessage(string message)
    {
        switch (CurrentConversationParadigm)
        {
            case ConversationType.ChannelConversation:
                await VivoxService.Instance.SendChannelTextMessageAsync(ActiveChannelConversationId, message);
                break;

            case ConversationType.DirectedMessageConversation:
                await VivoxService.Instance.SendDirectTextMessageAsync(ActiveDMConversationId, message);
                OnDMSentFromLocalUser?.Invoke(message);
                break;
        }
    }

    /// <summary>
    /// Loads the last saved conversation type from PlayerPrefs.
    /// </summary>
    public void LoadPrefs()
    {
        var cachedConversationType = PlayerPrefs.GetString(CachedConversationTypeKey, "Channel");

        switch (cachedConversationType)
        {
            case "DM":
                CurrentConversationParadigm = ConversationType.DirectedMessageConversation;
                break;

            case "Channel":
                CurrentConversationParadigm = ConversationType.ChannelConversation;
                break;
        }
    }
}

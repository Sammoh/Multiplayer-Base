using MysteriousGuests.Netcode;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class StartGame : MonoBehaviour
{
    // This script is a placeholder for the StartGame functionality.
    // You can add your game start logic here, such as initializing game state,
    // loading scenes, or setting up player data.
    [SerializeField] private Button button;

    void Start()
    {
        // get the button component from the GameObject this script is attached to
        button.interactable = false;

        button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(OnStartGameClicked);
        }
        else
        {
            Debug.LogError("Button component not found on this GameObject.");
        }
    }

    private void OnStartGameClicked()
    {
        Debug.Log("Game is starting...");
        button.interactable = false;
        // TODO This needs to go through a presentation layer, not directly to the GameManager.
        // GameManager.Instance.StartGame();
    }
}

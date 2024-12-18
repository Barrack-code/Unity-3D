using UnityEngine;
using UnityEngine.UI;
using System;

public class GameModeSelector : MonoBehaviour
{
    [Header("UI References")]
    public GameObject mainCanvas;         // Main menu canvas
    public GameObject carSelectCanvas;    // Separate car selection canvas
    public Button singlePlayerButton;
    public Button multiplayerButton;
    public Button backButton;             // Optional back button to return to main menu

    void Start()
    {
        // Add listeners to buttons
        if (singlePlayerButton != null)
        {
            singlePlayerButton.onClick.AddListener(() => SelectGameMode(GameMode.Mode.SinglePlayer));
        }
        else
        {
            Debug.LogError("Single Player button not assigned in GameModeSelector!");
        }

        if (multiplayerButton != null)
        {
            multiplayerButton.onClick.AddListener(() => SelectGameMode(GameMode.Mode.Multiplayer));
        }
        else
        {
            Debug.LogError("Multiplayer button not assigned in GameModeSelector!");
        }

        if (backButton != null)
        {
            backButton.onClick.AddListener(BackToMainMenu);
        }

        // Make sure car select canvas starts inactive
        if (carSelectCanvas != null)
        {
            carSelectCanvas.SetActive(false);
        }
        else
        {
            Debug.LogError("Car Select Canvas not assigned in GameModeSelector!");
        }
    }

    public void SelectGameMode(GameMode.Mode mode)
    {
        Debug.Log($"Selecting game mode: {mode}");
        
        // Set the game mode
        GameMode.CurrentMode = mode;
        
        // Hide main canvas
        if (mainCanvas != null)
        {
            mainCanvas.SetActive(false);
        }
        
        // Show car selection canvas
        if (carSelectCanvas != null)
        {
            carSelectCanvas.SetActive(true);
            
            // Find and update the CarRotator
            CarRotator carRotator = carSelectCanvas.GetComponentInChildren<CarRotator>();
            if (carRotator != null)
            {
                // This will trigger OnEnable and set up the cars
                carRotator.gameObject.SetActive(true);
            }
            else
            {
                Debug.LogError("No CarRotator found in car select canvas!");
            }
        }
        else
        {
            Debug.LogError("Car Select Canvas not assigned!");
        }
    }

    public void BackToMainMenu()
    {
        // Show main canvas, hide car select
        if (mainCanvas != null) mainCanvas.SetActive(true);
        if (carSelectCanvas != null) carSelectCanvas.SetActive(false);
    }
}

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class OptionsMenu : MonoBehaviour
{
    [Header("Track Selection")]
    public GameObject circuitPreview;  // First track preview
    public GameObject tunnelPreview;   // Second track preview
    public string[] trackNames;        // Array of track names
    public string[] sceneNames;        // Array of scene names
    public TextMeshProUGUI trackNameText;

    [Header("UI Elements")]
    public GameObject optionsPanel;
    public Button previousTrackButton;
    public Button nextTrackButton;
    public Button selectButton;        // New select button
    public Button backButton;

    private bool isCircuitSelected = true;

    private void Start()
    {
        // Hide options panel by default
        if (optionsPanel != null)
        {
            optionsPanel.SetActive(false);
        }

        // Set up button listeners
        if (previousTrackButton != null)
        {
            previousTrackButton.onClick.AddListener(ToggleTrack);
            Debug.Log("Previous button listener added");
        }
        if (nextTrackButton != null)
        {
            nextTrackButton.onClick.AddListener(ToggleTrack);
            Debug.Log("Next button listener added");
        }
        if (selectButton != null)
        {
            selectButton.onClick.AddListener(SelectTrack);
            Debug.Log("Select button listener added");
        }
        if (backButton != null)
        {
            backButton.onClick.AddListener(BackToMainMenu);
            Debug.Log("Back button listener added");
        }

        // Set initial track state
        UpdateTrackDisplay();

        // Load previously selected track
        LoadSavedTrack();
    }

    private void LoadSavedTrack()
    {
        int savedTrack = PlayerPrefs.GetInt("SelectedTrack", 0);
        isCircuitSelected = (savedTrack == 0);
        UpdateTrackDisplay();
    }

    public void ShowOptions()
    {
        Debug.Log("ShowOptions called");
        if (optionsPanel != null)
        {
            optionsPanel.SetActive(true);
            UpdateTrackDisplay();
        }
    }

    public void BackToMainMenu()
    {
        Debug.Log("BackToMainMenu called");
        if (optionsPanel != null)
        {
            optionsPanel.SetActive(false);
        }
    }

    public void SelectTrack()
    {
        Debug.Log("SelectTrack called");
        SaveTrackSelection();
        
        // Show a confirmation message (you can add a UI text element for this)
        Debug.Log($"Selected track: {trackNames[isCircuitSelected ? 0 : 1]}");
        
        // Return to main menu
        BackToMainMenu();
    }

    public void ToggleTrack()
    {
        Debug.Log("Toggling track selection");
        isCircuitSelected = !isCircuitSelected;
        UpdateTrackDisplay();
    }

    private void UpdateTrackDisplay()
    {
        // Update track name
        if (trackNameText != null && trackNames != null)
        {
            trackNameText.text = trackNames[isCircuitSelected ? 0 : 1];
            Debug.Log($"Updated track name to: {trackNameText.text}");
        }

        // Update track previews
        if (circuitPreview != null)
        {
            circuitPreview.SetActive(isCircuitSelected);
            Debug.Log($"Circuit preview active: {isCircuitSelected}");
        }
        
        if (tunnelPreview != null)
        {
            tunnelPreview.SetActive(!isCircuitSelected);
            Debug.Log($"Tunnel preview active: {!isCircuitSelected}");
        }
    }

    private void SaveTrackSelection()
    {
        // Save both the track index and scene name
        int trackIndex = isCircuitSelected ? 0 : 1;
        PlayerPrefs.SetInt("SelectedTrack", trackIndex);
        
        if (sceneNames != null && trackIndex < sceneNames.Length)
        {
            PlayerPrefs.SetString("SelectedTrackScene", sceneNames[trackIndex]);
            Debug.Log($"Saved track scene: {sceneNames[trackIndex]}");
        }
        
        PlayerPrefs.Save();
    }
}

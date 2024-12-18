using UnityEngine;
using UnityEngine.UI;

public class TrackSelector : MonoBehaviour
{
    [System.Serializable]
    public class TrackInfo
    {
        public string trackName;
        public string sceneName;
        public Sprite trackPreview;  // Preview image of the track
    }

    [Header("Track Settings")]
    public TrackInfo[] availableTracks;
    public Image previewImage;      // UI Image to show track preview
    public Text trackNameText;      // UI Text to show track name
    
    [Header("UI Elements")]
    public GameObject trackSelectionPanel;  // The panel containing track selection UI
    public Button previousButton;
    public Button nextButton;

    private int currentTrackIndex = 0;

    private void Start()
    {
        // Hide track selection by default
        if (trackSelectionPanel != null)
            trackSelectionPanel.SetActive(false);

        // Show the first track when selection is opened
        if (availableTracks.Length > 0)
        {
            // Load previously selected track if any
            string savedTrack = PlayerPrefs.GetString("SelectedTrack", "");
            if (!string.IsNullOrEmpty(savedTrack))
            {
                for (int i = 0; i < availableTracks.Length; i++)
                {
                    if (availableTracks[i].sceneName == savedTrack)
                    {
                        currentTrackIndex = i;
                        break;
                    }
                }
            }
            UpdateTrackDisplay();
        }

        // Set up button listeners
        if (previousButton != null)
            previousButton.onClick.AddListener(PreviousTrack);
        if (nextButton != null)
            nextButton.onClick.AddListener(NextTrack);
    }

    public void ShowTrackSelection()
    {
        if (trackSelectionPanel != null)
        {
            trackSelectionPanel.SetActive(true);
            UpdateTrackDisplay();
        }
    }

    public void HideTrackSelection()
    {
        if (trackSelectionPanel != null)
        {
            trackSelectionPanel.SetActive(false);
        }
    }

    public void NextTrack()
    {
        currentTrackIndex = (currentTrackIndex + 1) % availableTracks.Length;
        UpdateTrackDisplay();
    }

    public void PreviousTrack()
    {
        currentTrackIndex--;
        if (currentTrackIndex < 0) currentTrackIndex = availableTracks.Length - 1;
        UpdateTrackDisplay();
    }

    private void UpdateTrackDisplay()
    {
        TrackInfo currentTrack = availableTracks[currentTrackIndex];
        
        // Update preview image
        if (previewImage != null && currentTrack.trackPreview != null)
        {
            previewImage.sprite = currentTrack.trackPreview;
        }

        // Update track name
        if (trackNameText != null)
        {
            trackNameText.text = currentTrack.trackName;
        }

        // Save selected track
        PlayerPrefs.SetString("SelectedTrack", currentTrack.sceneName);
        PlayerPrefs.Save();
    }

    public void ApplyTrackSelection()
    {
        // Save and hide the panel
        UpdateTrackDisplay();
        HideTrackSelection();
    }

    public void CancelTrackSelection()
    {
        // Revert to previously saved track
        string savedTrack = PlayerPrefs.GetString("SelectedTrack", "");
        if (!string.IsNullOrEmpty(savedTrack))
        {
            for (int i = 0; i < availableTracks.Length; i++)
            {
                if (availableTracks[i].sceneName == savedTrack)
                {
                    currentTrackIndex = i;
                    UpdateTrackDisplay();
                    break;
                }
            }
        }
        HideTrackSelection();
    }
}

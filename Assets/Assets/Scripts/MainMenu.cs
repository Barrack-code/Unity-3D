using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class MainMenu : MonoBehaviour
{
    [Header("Buttons")]
    public Button playButton;
    public Button optionsButton;
    public Button settingsButton;
    public Button quitButton;

    [Header("UI")]
    public GameObject mainCanvas;         // Main menu canvas with all UI
    public GameObject mainMenuPanel;      // Main menu panel (containing all main menu buttons)
    public GameObject modeSelectPanel;    // Panel for mode selection within main canvas
    public GameObject settingsPanel;      // Panel for settings
    public Dropdown qualityDropdown;      // Dropdown for quality settings
    public Button settingsBackButton;     // Back button in settings panel

    private readonly string[] qualityNames = { "Very Low", "Low", "Medium", "High", "Very High", "Ultra" };

    void Start()
    {
        Debug.Log("[MainMenu] Starting initialization...");

        // Check UI elements
        if (!mainCanvas) Debug.LogError("[MainMenu] Main Canvas is not assigned!");
        if (!mainMenuPanel) Debug.LogError("[MainMenu] Main Menu Panel is not assigned!");
        if (!modeSelectPanel) Debug.LogError("[MainMenu] Mode Select Panel is not assigned!");
        if (!settingsPanel) Debug.LogError("[MainMenu] Settings Panel is not assigned!");

        // Add click listeners to buttons and verify they exist
        if (playButton) 
        {
            playButton.onClick.AddListener(PlayGame);
            Debug.Log("[MainMenu] Play button initialized");
        }
        else Debug.LogError("[MainMenu] Play Button is not assigned!");

        if (optionsButton)
        {
            optionsButton.onClick.AddListener(OpenOptions);
            Debug.Log("[MainMenu] Options button initialized");
        }
        else Debug.LogError("[MainMenu] Options Button is not assigned!");

        if (settingsButton)
        {
            settingsButton.onClick.AddListener(OpenSettings);
            Debug.Log("[MainMenu] Settings button initialized");
        }
        else Debug.LogError("[MainMenu] Settings Button is not assigned!");

        if (quitButton)
        {
            quitButton.onClick.AddListener(QuitGame);
            Debug.Log("[MainMenu] Quit button initialized");
        }
        else Debug.LogError("[MainMenu] Quit Button is not assigned!");

        if (settingsBackButton)
        {
            settingsBackButton.onClick.AddListener(CloseSettings);
            Debug.Log("[MainMenu] Settings Back button initialized");
        }
        else Debug.LogError("[MainMenu] Settings Back Button is not assigned!");

        // Initialize quality dropdown
        if (qualityDropdown)
        {
            InitializeQualityDropdown();
            Debug.Log("[MainMenu] Quality dropdown initialized");
        }
        else Debug.LogError("[MainMenu] Quality Dropdown is not assigned!");

        // Show main menu panel, hide others
        if (modeSelectPanel) modeSelectPanel.SetActive(false);
        if (settingsPanel) settingsPanel.SetActive(false);
        if (mainMenuPanel) mainMenuPanel.SetActive(true);

        Debug.Log("[MainMenu] Initialization complete");
    }

    private void InitializeQualityDropdown()
    {
        if (qualityDropdown == null) return;

        // Style the dropdown
        RectTransform dropdownRect = qualityDropdown.GetComponent<RectTransform>();
        if (dropdownRect != null)
        {
            dropdownRect.sizeDelta = new Vector2(400f, 60f); // Width and height of the main dropdown
        }

        // Style the dropdown background
        Image dropdownImage = qualityDropdown.GetComponent<Image>();
        if (dropdownImage != null)
        {
            dropdownImage.color = new Color(0.565f, 0.933f, 0.565f); // Light green
        }

        // Style the main text
        Text labelText = qualityDropdown.captionText;
        if (labelText != null)
        {
            labelText.fontSize = 24; // Slightly smaller font
            labelText.color = Color.black;
            labelText.alignment = TextAnchor.MiddleLeft; // Align left
        }

        // Style the template (dropdown list)
        RectTransform template = qualityDropdown.template;
        if (template != null)
        {
            // Make the dropdown list taller
            template.sizeDelta = new Vector2(400f, 400f);
            
            // Style template background
            Image templateImage = template.GetComponent<Image>();
            if (templateImage != null)
            {
                templateImage.color = new Color(0.565f, 0.933f, 0.565f);
            }

            // Find and adjust the Viewport's Content
            Transform content = template.Find("Viewport/Content");
            if (content != null)
            {
                // Adjust the height of each item in the dropdown
                VerticalLayoutGroup layoutGroup = content.GetComponent<VerticalLayoutGroup>();
                if (layoutGroup != null)
                {
                    layoutGroup.spacing = 5; // Space between items
                    layoutGroup.padding = new RectOffset(10, 10, 10, 10); // Padding around items
                }

                // Adjust each item's size
                foreach (Transform item in content)
                {
                    RectTransform itemRect = item.GetComponent<RectTransform>();
                    if (itemRect != null)
                    {
                        itemRect.sizeDelta = new Vector2(0, 50f); // Make each item taller
                    }

                    // Style the item's text
                    Toggle toggle = item.GetComponent<Toggle>();
                    if (toggle != null && toggle.graphic != null)
                    {
                        Text itemText = toggle.GetComponentInChildren<Text>();
                        if (itemText != null)
                        {
                            itemText.fontSize = 24;
                            itemText.color = Color.black;
                            itemText.alignment = TextAnchor.MiddleLeft;
                        }
                    }
                }
            }
        }

        // Clear existing options and add new ones
        qualityDropdown.ClearOptions();
        qualityDropdown.AddOptions(new List<string>(qualityNames));

        // Set current quality level
        int currentQuality = PlayerPrefs.GetInt("QualityLevel", QualitySettings.GetQualityLevel());
        qualityDropdown.value = currentQuality;
        qualityDropdown.onValueChanged.AddListener(OnQualityChanged);

        // Apply current quality settings
        QualitySettings.SetQualityLevel(currentQuality, true);
        Debug.Log($"Initialized quality to: {qualityNames[currentQuality]}");

        if (currentQuality == 5) // Ultra
        {
            QualitySettings.shadows = ShadowQuality.All;
            QualitySettings.shadowResolution = ShadowResolution.VeryHigh;
            QualitySettings.shadowDistance = 150f;
            QualitySettings.antiAliasing = 8;
            QualitySettings.softParticles = true;
            QualitySettings.realtimeReflectionProbes = true;
        }
    }

    public void PlayGame()
    {
        // Only show mode select panel, hide all others
        mainMenuPanel.SetActive(false);
        modeSelectPanel.SetActive(true);
        settingsPanel.SetActive(false);
    }

    public void OpenOptions()
    {
        // Find the OptionsMenu component and call ShowOptions directly
        OptionsMenu optionsMenu = FindFirstObjectByType<OptionsMenu>();
        if (optionsMenu != null)
        {
            optionsMenu.ShowOptions();
        }
    }

    public void OpenSettings()
    {
        if (mainMenuPanel != null && settingsPanel != null)
        {
            mainMenuPanel.SetActive(false);
            settingsPanel.SetActive(true);
        }
    }

    public void CloseSettings()
    {
        if (settingsPanel)
        {
            settingsPanel.SetActive(false);
            mainMenuPanel.SetActive(true);
        }
    }

    public void StartRace()
    {
        LoadingSystem.LoadScene("Race");
    }

    private void OnQualityChanged(int index)
    {
        if (index >= 0 && index < qualityNames.Length)
        {
            QualitySettings.SetQualityLevel(index, true);
            PlayerPrefs.SetInt("QualityLevel", index);
            PlayerPrefs.Save();
            Debug.Log($"Quality changed to: {qualityNames[index]}");

            // Apply additional settings for Ultra quality
            if (index == 5) // Ultra
            {
                QualitySettings.shadows = ShadowQuality.All;
                QualitySettings.shadowResolution = ShadowResolution.VeryHigh;
                QualitySettings.shadowDistance = 150f;
                QualitySettings.antiAliasing = 8;
                QualitySettings.softParticles = true;
                QualitySettings.realtimeReflectionProbes = true;
            }
        }
    }

    public void QuitGame()
    {
        Debug.Log("[MainMenu] Quit button clicked");
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

    public void BackToMainMenu()
    {
        if (modeSelectPanel) modeSelectPanel.SetActive(false);
    }
}

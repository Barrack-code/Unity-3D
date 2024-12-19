using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class LoadingSystem : MonoBehaviour
{
    public static LoadingSystem Instance { get; private set; }

    [Header("UI References")]
    public Slider loadingSlider;
    public Image loadingBarFill;  // Reference to the Fill image of the Slider
    public TextMeshProUGUI percentageText;
    public TextMeshProUGUI statusText;
    public Image backgroundImage;

    [Header("Loading Settings")]
    public string[] loadingTips = new string[] {
        "Tip: Press 'R' to respawn your car if you get stuck!",
        "Tip: Use the brake to take tight corners more effectively",
        "Tip: Watch out for track boundaries - staying on track is faster!",
        "First time loading may take a bit longer while we set things up...",
        "Loading assets for the first time - please be patient..."
    };
    public float minimumLoadTime = 3f;  // Minimum time to show loading screen
    public float progressSmoothing = 0.5f;  // How smooth the progress bar moves
    public float firstLoadProgressScale = 0.3f;  // Slower progress for first load

    [Header("Loading Bar Colors")]
    public Color loadingBarStartColor = new Color(0.8f, 0.2f, 0.2f, 1f);    // Red
    public Color loadingBarMidColor1 = new Color(1f, 0.6f, 0.0f, 1f);       // Orange
    public Color loadingBarMidColor2 = new Color(0.2f, 0.8f, 0.2f, 1f);     // Green
    public Color loadingBarEndColor = new Color(0.2f, 0.4f, 1f, 1f);        // Blue

    private bool isFirstLoad = true;  // Track if this is the first load

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            // Ensure all required components are assigned
            if (loadingSlider == null)
            {
                Debug.LogError("[LoadingSystem] Loading Slider not assigned!");
                return;
            }
            if (loadingBarFill == null)
            {
                Debug.LogError("[LoadingSystem] Loading Bar Fill not assigned!");
                return;
            }
            if (percentageText == null)
            {
                Debug.LogError("[LoadingSystem] Percentage Text not assigned!");
                return;
            }
            if (statusText == null)
            {
                Debug.LogError("[LoadingSystem] Status Text not assigned!");
                return;
            }

            // Hide initially
            gameObject.SetActive(false);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public static void LoadScene(string sceneName)
    {
        if (Instance == null)
        {
            Debug.LogError("LoadingSystem Instance is null! Make sure there is a LoadingSystem prefab in your scene.");
            // Fallback to direct scene loading
            SceneManager.LoadScene(sceneName);
            return;
        }

        Instance.gameObject.SetActive(true);
        Instance.StartCoroutine(Instance.LoadSceneAsync(sceneName));
    }

    private IEnumerator LoadSceneAsync(string sceneName)
    {
        // Show a random tip, prioritizing first-load tips for first time
        if (loadingTips != null && loadingTips.Length > 0)
        {
            int tipIndex;
            if (isFirstLoad)
            {
                // Show one of the last two tips (first-time loading tips)
                tipIndex = Random.Range(loadingTips.Length - 2, loadingTips.Length);
            }
            else
            {
                // Show any tip
                tipIndex = Random.Range(0, loadingTips.Length);
            }
            statusText.text = loadingTips[tipIndex];
            
            // Start a coroutine to cycle through tips
            StartCoroutine(CycleTips(tipIndex));
        }

        // Reset loading bar
        loadingSlider.value = 0;
        loadingBarFill.color = loadingBarStartColor;
        percentageText.text = "0%";

        // Start loading the scene
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false;

        float startTime = Time.time;
        float artificialProgress = 0f;
        
        // Use slower progress for first load
        float progressRate = isFirstLoad ? progressSmoothing * firstLoadProgressScale : progressSmoothing;
        
        while (!asyncLoad.isDone)
        {
            // Calculate real progress
            float realProgress = Mathf.Clamp01(asyncLoad.progress / 0.9f);
            
            // Smoothly interpolate artificial progress
            artificialProgress = Mathf.MoveTowards(artificialProgress, realProgress, Time.deltaTime * progressRate);
            
            // Update progress UI
            loadingSlider.value = artificialProgress;
            percentageText.text = $"{(artificialProgress * 100):0}%";

            // Update loading bar color based on progress
            Color targetColor;
            if (artificialProgress < 0.33f)
            {
                // Lerp from start color to first mid color (red to orange)
                float t = artificialProgress / 0.33f;
                targetColor = Color.Lerp(loadingBarStartColor, loadingBarMidColor1, t);
            }
            else if (artificialProgress < 0.66f)
            {
                // Lerp from first mid color to second mid color (orange to green)
                float t = (artificialProgress - 0.33f) / 0.33f;
                targetColor = Color.Lerp(loadingBarMidColor1, loadingBarMidColor2, t);
            }
            else
            {
                // Lerp from second mid color to end color (green to blue)
                float t = (artificialProgress - 0.66f) / 0.34f;
                targetColor = Color.Lerp(loadingBarMidColor2, loadingBarEndColor, t);
            }

            // Smoothly transition the loading bar fill color
            loadingBarFill.color = Color.Lerp(loadingBarFill.color, targetColor, Time.deltaTime * 3f);

            // Wait for minimum load time and scene to be ready
            if (asyncLoad.progress >= 0.9f && Time.time - startTime >= minimumLoadTime)
            {
                asyncLoad.allowSceneActivation = true;
            }

            yield return null;
        }

        // No longer first load
        isFirstLoad = false;

        // Hide loading screen
        gameObject.SetActive(false);
    }

    private IEnumerator CycleTips(int startIndex)
    {
        float tipDisplayTime = 2f;  // Time each tip is shown
        int currentTipIndex = startIndex;

        while (gameObject.activeSelf)
        {
            yield return new WaitForSeconds(tipDisplayTime);
            
            // Move to next tip
            currentTipIndex = (currentTipIndex + 1) % loadingTips.Length;
            statusText.text = loadingTips[currentTipIndex];
        }
    }
}

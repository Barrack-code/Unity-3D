using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class LoadingScreen : MonoBehaviour
{
    [Header("UI Elements")]
    public Image progressBar;
    public TextMeshProUGUI progressText;
    public TextMeshProUGUI tipText;
    public CanvasGroup fadeGroup;

    [Header("Loading Settings")]
    public float minimumLoadTime = 2f;
    public string[] loadingTips;

    private void Start()
    {
        // Ensure the loading screen is visible
        fadeGroup.alpha = 1;
        
        // Start with empty progress
        if (progressBar != null)
            progressBar.fillAmount = 0f;
            
        if (progressText != null)
            progressText.text = "Loading... 0%";

        // Show random tip
        if (tipText != null && loadingTips != null && loadingTips.Length > 0)
        {
            tipText.text = loadingTips[Random.Range(0, loadingTips.Length)];
        }

        // Start the loading process
        StartCoroutine(LoadGameScene());
    }

    private IEnumerator LoadGameScene()
    {
        // Get the scene to load from PlayerPrefs (set by MainMenu)
        string sceneToLoad = PlayerPrefs.GetString("NextScene", "Race");
        
        // Start loading the scene asynchronously
        AsyncOperation loadOperation = SceneManager.LoadSceneAsync(sceneToLoad);
        loadOperation.allowSceneActivation = false;
        
        float startTime = Time.time;
        
        while (!loadOperation.isDone)
        {
            // Calculate progress (0 to 0.9 from AsyncOperation)
            float progress = Mathf.Clamp01(loadOperation.progress / 0.9f);
            
            // Update UI
            if (progressBar != null)
                progressBar.fillAmount = progress;
                
            if (progressText != null)
                progressText.text = $"Loading... {(progress * 100):0}%";

            // Check if we've met minimum load time and scene is ready
            if (loadOperation.progress >= 0.9f && Time.time - startTime >= minimumLoadTime)
            {
                // Fade out loading screen
                float fadeTime = 0.5f;
                float elapsedTime = 0;
                
                while (elapsedTime < fadeTime)
                {
                    elapsedTime += Time.deltaTime;
                    fadeGroup.alpha = Mathf.Lerp(1, 0, elapsedTime / fadeTime);
                    yield return null;
                }

                // Activate the scene
                loadOperation.allowSceneActivation = true;
            }

            yield return null;
        }
    }
}

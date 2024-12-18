using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class LoadingManager : MonoBehaviour
{
    [Header("UI References")]
    public Image loadingBar;
    public TextMeshProUGUI percentageText;
    public TextMeshProUGUI statusText;
    public CanvasGroup fadeGroup;

    [Header("Loading Settings")]
    public float minimumLoadTime = 2f;
    public string[] loadingTips;

    private void Start()
    {
        // Initialize UI
        fadeGroup.alpha = 1;
        loadingBar.fillAmount = 0f;
        percentageText.text = "0%";
        
        // Show random tip if available
        if (loadingTips != null && loadingTips.Length > 0)
        {
            statusText.text = loadingTips[Random.Range(0, loadingTips.Length)];
        }

        // Start loading
        StartCoroutine(LoadGameScene());
    }

    private IEnumerator LoadGameScene()
    {
        // Get the scene to load
        string sceneToLoad = PlayerPrefs.GetString("NextScene", "Race");
        float startTime = Time.time;

        // Start async loading
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneToLoad);
        asyncLoad.allowSceneActivation = false;

        while (!asyncLoad.isDone)
        {
            // Update progress
            float progress = Mathf.Clamp01(asyncLoad.progress / 0.9f);
            loadingBar.fillAmount = progress;
            percentageText.text = $"{(progress * 100):0}%";

            // Check if loading is complete and minimum time has passed
            if (asyncLoad.progress >= 0.9f && Time.time - startTime >= minimumLoadTime)
            {
                // Fade out
                float fadeTime = 0.5f;
                float elapsedTime = 0;

                while (elapsedTime < fadeTime)
                {
                    elapsedTime += Time.deltaTime;
                    fadeGroup.alpha = Mathf.Lerp(1, 0, elapsedTime / fadeTime);
                    yield return null;
                }

                asyncLoad.allowSceneActivation = true;
            }

            yield return null;
        }
    }
}

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class GameInitializer : MonoBehaviour
{
    [Header("UI References")]
    public Image loadingBar;
    public TextMeshProUGUI percentageText;
    public TextMeshProUGUI statusText;

    [Header("Asset Loading")]
    public List<string> assetPaths = new List<string>(); // Fill this in Unity Inspector
    private int totalAssets;
    private int loadedAssets;

    private void Start()
    {
        // Initialize loading UI
        loadingBar.fillAmount = 0f;
        percentageText.text = "0%";
        statusText.text = "Initializing...";

        // Start loading process
        StartCoroutine(LoadGame());
    }

    private IEnumerator LoadGame()
    {
        // Simulate loading different types of assets
        yield return StartCoroutine(LoadAssets());
        
        // Load Main Menu scene
        statusText.text = "Loading Main Menu...";
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("MainMenu");
        asyncLoad.allowSceneActivation = false;

        while (asyncLoad.progress < 0.9f)
        {
            loadingBar.fillAmount = asyncLoad.progress;
            percentageText.text = $"{(asyncLoad.progress * 100):0}%";
            yield return null;
        }

        // Ensure loading bar reaches 100%
        loadingBar.fillAmount = 1f;
        percentageText.text = "100%";
        statusText.text = "Press Any Key to Continue";

        // Wait for input
        yield return new WaitUntil(() => Input.anyKeyDown);
        asyncLoad.allowSceneActivation = true;
    }

    private IEnumerator LoadAssets()
    {
        totalAssets = assetPaths.Count;
        loadedAssets = 0;

        foreach (string assetPath in assetPaths)
        {
            // Simulate loading each asset
            statusText.text = $"Loading {System.IO.Path.GetFileName(assetPath)}...";
            
            // Simulate asset loading time
            float randomLoadTime = Random.Range(0.1f, 0.5f);
            yield return new WaitForSeconds(randomLoadTime);
            
            loadedAssets++;
            float progress = (float)loadedAssets / totalAssets;
            loadingBar.fillAmount = progress;
            percentageText.text = $"{(progress * 100):0}%";
        }
    }
}

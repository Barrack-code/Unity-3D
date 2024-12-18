using UnityEngine;

public class GameStarter : MonoBehaviour
{
    [SerializeField] private LoadingSystem loadingSystemPrefab;

    void Start()
    {
        // Ensure LoadingSystem exists
        if (LoadingSystem.Instance == null && loadingSystemPrefab != null)
        {
            Instantiate(loadingSystemPrefab);
        }
        else if (LoadingSystem.Instance == null)
        {
            Debug.LogError("LoadingSystem prefab not assigned in GameStarter!");
            return;
        }

        // Load main menu immediately
        LoadingSystem.LoadScene("MainMenu");
    }
}

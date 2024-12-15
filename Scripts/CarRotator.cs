using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;
using System.Linq;

public class CarRotator : MonoBehaviour
{
    [Header("Cars")]
    public GameObject[] displayCars;  // Array of display car prefabs
    public string[] carNames;  // Array to hold car names
    public TextMeshProUGUI carNameText;  // Reference to the UI text
    public TextMeshProUGUI playerText;   // To show which player is selecting
    private int currentCarIndex = 0;

    [Header("Selection")]
    private bool isPlayer1Selecting = true;  // True for Player 1, False for Player 2
    private string player1Selection = "";
    private string player2Selection = "";

    [Header("Rotation")]
    public float autoRotationSpeed = 30f;  // Degrees per second
    public float manualRotationSpeed = 100f;  // Increased for better control
    private bool isDragging = false;
    private float lastMouseX;

    void OnEnable()
    {
        // When the panel is enabled, make sure cars are properly set up
        ShowCurrentCar();
    }

    void Start()
    {
        // Log display cars setup
        Debug.Log("Car Display Setup:");
        for (int i = 0; i < displayCars.Length; i++)
        {
            if (displayCars[i] != null)
            {
                Debug.Log($"Display Car {i}: {displayCars[i].name} (Prefab: {displayCars[i].name.Replace("(Clone)", "").Trim()})");
            }
            else
            {
                Debug.Log($"Display Car {i}: NULL");
            }
        }

        // Disable wheel colliders on display cars to prevent errors
        foreach (GameObject car in displayCars)
        {
            if (car != null)
            {
                // Disable wheel colliders on display cars
                WheelCollider[] wheelColliders = car.GetComponentsInChildren<WheelCollider>();
                foreach (WheelCollider collider in wheelColliders)
                {
                    collider.enabled = false;
                }

                // Disable car controller components
                CarController controller = car.GetComponent<CarController>();
                if (controller != null)
                {
                    controller.enabled = false;
                }

                // Ensure the car is a prefab instance, not a scene reference
                GameObject[] sceneObjects = SceneManager.GetActiveScene().GetRootGameObjects();
                if (sceneObjects.Contains(car))
                {
                    Debug.LogWarning($"Car {car.name} is a scene object, not a prefab instance. This may cause issues.");
                }
            }
        }

        ShowCurrentCar();
        UpdateUI();
    }

    void ShowCurrentCar()
    {
        if (displayCars == null || displayCars.Length == 0)
        {
            Debug.LogError("No display cars assigned to CarRotator!");
            return;
        }

        // Hide all cars except the current one
        for (int i = 0; i < displayCars.Length; i++)
        {
            if (displayCars[i] != null)
            {
                bool shouldBeActive = (i == currentCarIndex);
                displayCars[i].SetActive(shouldBeActive);
                
                if (shouldBeActive)
                {
                    // Make sure the car is properly positioned
                    displayCars[i].transform.localPosition = Vector3.zero;
                    displayCars[i].transform.localRotation = Quaternion.identity;
                }
            }
        }
    }

    void UpdateUI()
    {
        // Update car name
        if (carNameText != null && carNames != null && currentCarIndex < carNames.Length)
        {
            carNameText.text = carNames[currentCarIndex];
        }

        // Update player text
        if (playerText != null)
        {
            if (GameMode.IsSinglePlayer())
            {
                playerText.text = "Select Your Car";
                playerText.color = Color.white;
            }
            else
            {
                string newText = isPlayer1Selecting ? "Player 1 Select" : "Player 2 Select";
                playerText.text = newText;
                
                Color lightYellow = new Color(1f, 1f, 0.7f);
                Color lightGreen = new Color(0.7f, 1f, 0.7f);
                playerText.color = isPlayer1Selecting ? lightYellow : lightGreen;
            }
        }
    }

    public void NextCar()
    {
        if (displayCars.Length == 0) return;

        // Hide current car
        if (displayCars[currentCarIndex] != null)
        {
            displayCars[currentCarIndex].SetActive(false);
        }

        // Move to next car
        currentCarIndex = (currentCarIndex + 1) % displayCars.Length;

        ShowCurrentCar();
        UpdateUI();
    }

    public void PreviousCar()
    {
        if (displayCars.Length == 0) return;

        // Hide current car
        if (displayCars[currentCarIndex] != null)
        {
            displayCars[currentCarIndex].SetActive(false);
        }

        // Move to previous car
        currentCarIndex--;
        if (currentCarIndex < 0) currentCarIndex = displayCars.Length - 1;

        ShowCurrentCar();
        UpdateUI();
    }

    public void SelectCar()
    {
        if (displayCars[currentCarIndex] == null) return;

        // Get the base name (remove "_Display" suffix and any Unity-added markers)
        string selectedCar = displayCars[currentCarIndex].name
            .Replace("_Display", "")
            .Replace("(Clone)", "")
            .Replace("Variant", "")
            .Trim();
        
        Debug.Log($"Selected car: {selectedCar} from display car: {displayCars[currentCarIndex].name}");
        
        if (GameMode.IsSinglePlayer())
        {
            // In single player, select car and start race immediately
            Debug.Log($"Player selected: {selectedCar}");
            player1Selection = selectedCar;
            player2Selection = selectedCar; // AI uses same car
            LoadRaceScene();
        }
        else if (isPlayer1Selecting)
        {
            Debug.Log($"Player 1 selected: {selectedCar}");
            player1Selection = selectedCar;
            isPlayer1Selecting = false;
            
            // Reset car selection for Player 2
            currentCarIndex = 0;
            ShowCurrentCar();
            UpdateUI();
        }
        else
        {
            Debug.Log($"Player 2 selected: {selectedCar}");
            player2Selection = selectedCar;
            LoadRaceScene();
        }
    }

    void LoadRaceScene()
    {
        // Save the car selections and indices
        PlayerPrefs.SetString("Player1Car", player1Selection);
        PlayerPrefs.SetString("Player2Car", player2Selection);
        
        // For multiplayer, save each player's selection index
        if (!GameMode.IsSinglePlayer())
        {
            // For Player 1, save their selection index
            if (isPlayer1Selecting)
            {
                PlayerPrefs.SetInt("Player1CarIndex", currentCarIndex);
            }
            // For Player 2, save their selection index
            else
            {
                PlayerPrefs.SetInt("Player2CarIndex", currentCarIndex);
            }
        }
        else
        {
            // In single player, both indices should be the same
            PlayerPrefs.SetInt("Player1CarIndex", currentCarIndex);
            PlayerPrefs.SetInt("Player2CarIndex", currentCarIndex);
        }
        
        PlayerPrefs.Save();
        Debug.Log($"Saved car selections - P1: {player1Selection} (Index: {PlayerPrefs.GetInt("Player1CarIndex")}), P2: {player2Selection} (Index: {PlayerPrefs.GetInt("Player2CarIndex")})");
        
        StartCoroutine(LoadRaceSceneAsync());
    }

    IEnumerator LoadRaceSceneAsync()
    {
        // Load the selected track scene instead of a fixed scene index
        string selectedScene = PlayerPrefs.GetString("SelectedTrackScene", "Circuit"); // Default to Circuit if nothing selected
        Debug.Log($"Loading selected track: {selectedScene}");
        
        // Load the new scene in Single mode (replacing current scene)
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(selectedScene, LoadSceneMode.Single);
        asyncLoad.allowSceneActivation = true;
        
        // Wait until the scene is fully loaded
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
        
        Debug.Log("Track scene loaded - RaceManager will spawn cars");
    }

    void Update()
    {
        // Make sure current car is visible
        if (displayCars[currentCarIndex] != null && !displayCars[currentCarIndex].activeSelf)
        {
            displayCars[currentCarIndex].SetActive(true);
        }

        // Check for mouse input
        if (Input.GetMouseButtonDown(0))
        {
            isDragging = true;
            lastMouseX = Input.mousePosition.x;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
        }

        // Handle rotation for the current car
        if (displayCars[currentCarIndex] != null)
        {
            if (isDragging)
            {
                // Manual rotation when dragging
                float deltaX = Input.mousePosition.x - lastMouseX;
                displayCars[currentCarIndex].transform.Rotate(Vector3.up, -deltaX * manualRotationSpeed * Time.deltaTime);
                lastMouseX = Input.mousePosition.x;
            }
            else
            {
                // Auto rotation when not touching
                displayCars[currentCarIndex].transform.Rotate(Vector3.up, autoRotationSpeed * Time.deltaTime);
            }
        }
    }
}

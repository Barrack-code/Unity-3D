using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class CarRotator : MonoBehaviour
{
    [Header("Cars")]
    [SerializeField] private GameObject[] carPrefabs;  // Array of car prefabs
    private GameObject[] displayCars;  // Array to hold display versions
    public string[] carNames;  // Array to hold car names
    public TextMeshProUGUI carNameText;  // Reference to the UI text
    public TextMeshProUGUI playerText;   // To show which player is selecting
    private int currentCarIndex = 0;

    [Header("Selection")]
    private bool isPlayer1Selecting = true;  // True for Player 1, False for Player 2

    [Header("Rotation")]
    public float rotationSpeed = 30f;  // Degrees per second
    public float manualRotationSpeed = 100f;  // Increased for better control
    private float lastMouseX;

    void Start()
    {
        Debug.Log("[CarRotator] Starting car display initialization...");
        currentCarIndex = 0;  // Ensure we start with the first car
        
        // Set up camera and canvas first
        SetupDisplayCamera();
        SetupCanvas();
        
        // Create display car holders
        displayCars = new GameObject[carPrefabs.Length];
        for (int i = 0; i < carPrefabs.Length; i++)
        {
            if (carPrefabs[i] != null)
            {
                // Create a holder for each car
                GameObject holder = new GameObject($"CarHolder_{i}");
                holder.transform.SetParent(transform);
                holder.transform.localPosition = Vector3.zero;
                holder.transform.localRotation = Quaternion.identity;
                holder.transform.localScale = Vector3.one;  // Use normal scale for holder

                // Instantiate the car as a child of the holder
                displayCars[i] = Instantiate(carPrefabs[i], holder.transform);
                displayCars[i].transform.localPosition = new Vector3(0f, -4.549353e-05f, 8.510913e-05f);
                displayCars[i].transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
                displayCars[i].transform.localScale = new Vector3(100f, 100f, 100f);  // Scale the car instead
                
                // Disable physics and audio
                DisablePhysicsAndAudio(displayCars[i]);
                DisableGameplayComponents(displayCars[i]);
                
                // Initially hide holder but keep car active
                holder.SetActive(false);
                displayCars[i].SetActive(true);
            }
            else
            {
                Debug.LogError($"[CarRotator] Car prefab at index {i} is null!");
            }
        }

        // Show first car and update UI
        ShowCurrentCar();
        UpdateUI();
        
        Debug.Log("[CarRotator] Initialization complete");
    }

    void OnEnable()
    {
        // When panel is activated, make sure current car is shown
        if (displayCars != null && displayCars.Length > 0)
        {
            ShowCurrentCar();
            UpdateUI();
        }
    }

    private void DisablePhysicsAndAudio(GameObject car)
    {
        // Make Rigidbodies kinematic instead of destroying them
        Rigidbody[] rigidbodies = car.GetComponentsInChildren<Rigidbody>();
        foreach (Rigidbody rb in rigidbodies)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
            rb.constraints = RigidbodyConstraints.FreezeAll;  // Freeze all movement
        }

        // Disable all Colliders
        Collider[] colliders = car.GetComponentsInChildren<Collider>();
        foreach (Collider collider in colliders)
        {
            collider.enabled = false;
        }

        // Disable all AudioSources
        AudioSource[] audioSources = car.GetComponentsInChildren<AudioSource>();
        foreach (AudioSource audio in audioSources)
        {
            audio.enabled = false;
        }

        // Disable wheel colliders
        WheelCollider[] wheelColliders = car.GetComponentsInChildren<WheelCollider>();
        foreach (WheelCollider wheel in wheelColliders)
        {
            wheel.enabled = false;
        }
    }

    private void SetupDisplayCamera()
    {
        Debug.Log("[CarRotator] Setting up display camera");

        // Find and disable any split screen cameras
        var splitScreenCameras = FindObjectsByType<SplitScreenCamera>(FindObjectsSortMode.None);
        foreach (var cam in splitScreenCameras)
        {
            cam.gameObject.SetActive(false);
        }

        // Find the main camera
        Camera mainCam = Camera.main;
        if (mainCam == null)
        {
            Debug.LogError("[CarRotator] No main camera found! Creating one...");
            GameObject camObj = new GameObject("CarDisplayCamera");
            mainCam = camObj.AddComponent<Camera>();
        }

        // Set up the camera for proper canvas rendering
        mainCam.gameObject.SetActive(true);
        mainCam.clearFlags = CameraClearFlags.SolidColor;
        mainCam.backgroundColor = Color.black;
        mainCam.rect = new Rect(0, 0, 1, 1);
        mainCam.orthographic = false;
        mainCam.fieldOfView = 60f;
        mainCam.nearClipPlane = 0.1f;
        mainCam.farClipPlane = 1000f;
        
        // Position camera for a good view of the car
        mainCam.transform.position = new Vector3(0, 2, -5);
        mainCam.transform.rotation = Quaternion.Euler(15, 0, 0);

        // Make sure the camera culling mask includes the car layer and UI
        mainCam.cullingMask = -1;
        
        Debug.Log($"[CarRotator] Camera setup complete - Position: {mainCam.transform.position}, Rotation: {mainCam.transform.rotation.eulerAngles}");
    }

    private void SetupCanvas()
    {
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas != null && canvas.renderMode == RenderMode.ScreenSpaceCamera)
        {
            canvas.planeDistance = 1f;
            Debug.Log("[CarRotator] Set canvas plane distance to 1");
        }
    }

    void OnDestroy()
    {
        // Clean up display cars when the script is destroyed
        if (displayCars != null)
        {
            foreach (GameObject car in displayCars)
            {
                if (car != null)
                {
                    Destroy(car);
                }
            }
        }
    }

    private void DisableGameplayComponents(GameObject car)
    {
        Debug.Log($"[CarRotator] Disabling gameplay components for {car.name}");

        // Disable physics components
        Rigidbody rb = car.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            Debug.Log("[CarRotator] Disabled Rigidbody");
        }

        // Disable all wheel colliders
        WheelCollider[] wheelColliders = car.GetComponentsInChildren<WheelCollider>();
        foreach (var collider in wheelColliders)
        {
            collider.enabled = false;
        }
        Debug.Log($"[CarRotator] Disabled {wheelColliders.Length} wheel colliders");

        // Disable all car-related scripts
        var scriptsToDisable = new System.Type[] {
            typeof(Backup),              // Main car controller
            typeof(AICarController),     // AI car controller
            typeof(CarInputManager),     // Input management
            typeof(InputController),     // Alternative input controller
            typeof(PlayerInput),         // Player input
            typeof(CarAudio),            // Car audio
            typeof(CheckpointSystem),    // Checkpoint handling
            typeof(RaceProgressTracker)  // Race progress
        };

        foreach (var scriptType in scriptsToDisable)
        {
            var component = car.GetComponent(scriptType);
            if (component != null)
            {
                var behaviour = component as MonoBehaviour;
                if (behaviour != null)
                {
                    behaviour.enabled = false;
                    Debug.Log($"[CarRotator] Disabled {scriptType.Name}");
                }
            }
        }

        // Disable all colliders except mesh colliders
        Collider[] colliders = car.GetComponentsInChildren<Collider>();
        int disabledCount = 0;
        foreach (var collider in colliders)
        {
            if (!(collider is MeshCollider))
            {
                collider.enabled = false;
                disabledCount++;
            }
        }
        Debug.Log($"[CarRotator] Disabled {disabledCount} colliders (kept mesh colliders)");
    }

    void DisableAllAudio(GameObject car)
    {
        AudioSource[] audioSources = car.GetComponentsInChildren<AudioSource>();
        foreach (var audio in audioSources)
        {
            audio.enabled = false;
        }
    }

    void ShowCurrentCar()
    {
        Debug.Log($"[CarRotator] Showing car index: {currentCarIndex}");
        
        if (displayCars == null)
        {
            Debug.LogError("[CarRotator] Display cars array is null!");
            return;
        }

        // First ensure all cars are in correct position and hidden
        for (int i = 0; i < displayCars.Length; i++)
        {
            if (displayCars[i] != null)
            {
                Transform holderTransform = displayCars[i].transform.parent;
                if (holderTransform != null)
                {
                    // Set position and rotation for all cars
                    displayCars[i].transform.localPosition = new Vector3(0f, -4.549353e-05f, 8.510913e-05f);
                    displayCars[i].transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
                    
                    bool shouldShow = (i == currentCarIndex);
                    holderTransform.gameObject.SetActive(shouldShow);
                    displayCars[i].SetActive(true); // Make sure car is always active
                    
                    if (shouldShow)
                    {
                        Debug.Log($"[CarRotator] Showing car {i} at position: {displayCars[i].transform.position}");
                    }
                }
            }
            else
            {
                Debug.LogError($"[CarRotator] Display car at index {i} is null!");
            }
        }
    }

    public void NextCar()
    {
        currentCarIndex = (currentCarIndex + 1) % displayCars.Length;
        ShowCurrentCar();
        UpdateUI();
    }

    public void PreviousCar()
    {
        currentCarIndex--;
        if (currentCarIndex < 0) currentCarIndex = displayCars.Length - 1;
        ShowCurrentCar();
        UpdateUI();
    }

    void Update()
    {
        if (currentCarIndex >= 0 && currentCarIndex < displayCars.Length && displayCars[currentCarIndex] != null)
        {
            GameObject currentCar = displayCars[currentCarIndex];
            
            // Always enforce correct position
            currentCar.transform.localPosition = new Vector3(0f, -4.549353e-05f, 8.510913e-05f);
            
            // Manual rotation with left mouse button
            if (Input.GetMouseButton(0))
            {
                float mouseDelta = Input.mousePosition.x - lastMouseX;
                // Only rotate around Y axis
                currentCar.transform.Rotate(0f, mouseDelta * manualRotationSpeed * Time.deltaTime, 0f, Space.World);
            }
            // Auto rotation when not being manually controlled
            else
            {
                // Only rotate around Y axis
                currentCar.transform.Rotate(0f, rotationSpeed * Time.deltaTime, 0f, Space.World);
            }

            // Ensure X and Z rotation stay at 0
            Vector3 currentRotation = currentCar.transform.localRotation.eulerAngles;
            if (currentRotation.x != 0f || currentRotation.z != 0f)
            {
                currentCar.transform.localRotation = Quaternion.Euler(0f, currentRotation.y, 0f);
            }

            lastMouseX = Input.mousePosition.x;
        }
    }

    void UpdateUI()
    {
        if (carNameText != null && carNames != null && currentCarIndex < carNames.Length)
        {
            carNameText.text = carNames[currentCarIndex];
        }

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

    public void SelectCar()
    {
        if (displayCars[currentCarIndex] == null) return;

        string selectedCar = carPrefabs[currentCarIndex].name;
        Debug.Log($"Selected car: {selectedCar}");
        
        if (GameMode.IsSinglePlayer())
        {
            PlayerPrefs.SetString("Player1Car", selectedCar);
            PlayerPrefs.SetString("Player2Car", selectedCar);
            PlayerPrefs.SetInt("Player1CarIndex", currentCarIndex);
            PlayerPrefs.SetInt("Player2CarIndex", currentCarIndex);
        }
        else if (isPlayer1Selecting)
        {
            PlayerPrefs.SetString("Player1Car", selectedCar);
            PlayerPrefs.SetInt("Player1CarIndex", currentCarIndex);
            isPlayer1Selecting = false;
            currentCarIndex = 0;
            ShowCurrentCar();
            UpdateUI();
            return;
        }
        else
        {
            PlayerPrefs.SetString("Player2Car", selectedCar);
            PlayerPrefs.SetInt("Player2CarIndex", currentCarIndex);
        }
        
        PlayerPrefs.Save();
        
        // Use LoadingSystem to load the selected track scene
        string selectedScene = PlayerPrefs.GetString("SelectedTrackScene", "Circuit");
        LoadingSystem.LoadScene(selectedScene);
    }
}
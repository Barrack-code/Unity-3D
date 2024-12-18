using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class RaceManager : MonoBehaviour
{
    [Header("Race Setup")]
    public Transform[] spawnPoints;
    public GameObject[] carPrefabs;
    public TextMeshProUGUI countdownText;
    
    [Header("Track Settings")]
    public LayerMask trackLayer;  // Layer for track detection
    public float maxDistanceFromTrack = 5f;  // Maximum distance allowed from track

    [Header("Split-Screen Camera Settings")]
    [Tooltip("Height of the camera above the car")]
    public float cameraHeight = 3f;
    [Tooltip("Distance of the camera behind the car")]
    public float cameraDistance = 8f;

    // Race state
    public bool HasRaceStarted { get; private set; }
    
    // References to spawned cars (marked as SerializeField to maintain Unity serialization)
    [SerializeField] private GameObject player1Car;
    [SerializeField] private GameObject player2Car;

    private CheckpointManager checkpointManager;
    private bool isAIMode = false;

    private void Start()
    {
        // Check if we came from track selection
        string selectedTrack = PlayerPrefs.GetString("SelectedTrackScene");
        Debug.Log($"Selected track scene: {selectedTrack}");

        HasRaceStarted = false;
        if (countdownText != null)
        {
            countdownText.gameObject.SetActive(true);
        }

        // Get the checkpoint manager
        checkpointManager = Object.FindAnyObjectByType<CheckpointManager>();
        if (checkpointManager == null)
        {
            Debug.LogError("No CheckpointManager found in scene!");
        }

        // Check game mode
        isAIMode = GameMode.IsSinglePlayer();
        SpawnCars();
        StartCoroutine(CountdownAndStartRace());
    }

    void SpawnCars()
    {
        // Clean up any existing cars
        if (player1Car != null) Destroy(player1Car);
        if (player2Car != null) Destroy(player2Car);

        // Get car indices and names from PlayerPrefs
        int player1CarIndex = PlayerPrefs.GetInt("Player1CarIndex", 0);
        int player2CarIndex = PlayerPrefs.GetInt("Player2CarIndex", 0);
        string player1CarName = PlayerPrefs.GetString("Player1Car", "");
        string player2CarName = PlayerPrefs.GetString("Player2Car", "");
        
        // Debug all available car prefabs
        Debug.Log("Available car prefabs:");
        for (int i = 0; i < carPrefabs.Length; i++)
        {
            if (carPrefabs[i] != null)
            {
                Debug.Log($"Index {i}: {carPrefabs[i].name}");
            }
            else
            {
                Debug.Log($"Index {i}: NULL");
            }
        }
        
        Debug.Log($"Spawning cars - P1: {player1CarName} (Index: {player1CarIndex}), P2: {player2CarName} (Index: {player2CarIndex})");

        if (carPrefabs == null || carPrefabs.Length == 0)
        {
            Debug.LogError("No car prefabs assigned in RaceManager!");
            return;
        }

        // Ensure indices are within bounds
        player1CarIndex = Mathf.Clamp(player1CarIndex, 0, carPrefabs.Length - 1);
        player2CarIndex = Mathf.Clamp(player2CarIndex, 0, carPrefabs.Length - 1);

        // Log the actual prefabs being used
        Debug.Log($"Using prefab for P1: {carPrefabs[player1CarIndex].name}");
        Debug.Log($"Using prefab for P2: {carPrefabs[player2CarIndex].name}");

        // First spawn point for Player 1, second for Player 2/AI
        Vector3 p1SpawnPos = spawnPoints[0].position;
        Vector3 p2SpawnPos = spawnPoints[1].position;
        Quaternion p1SpawnRot = spawnPoints[0].rotation;
        Quaternion p2SpawnRot = spawnPoints[1].rotation;

        // Spawn Player 1's car at first spawn point
        if (carPrefabs[player1CarIndex] != null)
        {
            player1Car = Instantiate(carPrefabs[player1CarIndex], p1SpawnPos, p1SpawnRot);
            player1Car.name = "Player1Car";
            SetupPlayerCar(player1Car, true);
            Debug.Log($"Spawned Player 1's car: {player1Car.name} (Prefab: {carPrefabs[player1CarIndex].name}) at position {p1SpawnPos}");
        }
        else
        {
            Debug.LogError("Player 1 car prefab is null!");
        }

        // Spawn Player 2's car or AI car at second spawn point
        if (carPrefabs[player2CarIndex] != null)
        {
            player2Car = Instantiate(carPrefabs[player2CarIndex], p2SpawnPos, p2SpawnRot);
            player2Car.name = "Player2Car";
            
            if (isAIMode)
            {
                SetupAICar(player2Car);
                Debug.Log($"Spawned AI car: {player2Car.name} (Prefab: {carPrefabs[player2CarIndex].name}) at position {p2SpawnPos}");
            }
            else
            {
                SetupPlayerCar(player2Car, false);
                Debug.Log($"Spawned Player 2's car: {player2Car.name} (Prefab: {carPrefabs[player2CarIndex].name}) at position {p2SpawnPos}");
            }
        }
        else
        {
            Debug.LogError("Player 2 car prefab is null!");
        }
    }

    private void SetupPlayerCar(GameObject car, bool isPlayer1)
    {
        // Setup camera
        var cameraObj = new GameObject(isPlayer1 ? "Player1Camera" : "Player2Camera");
        var camera = cameraObj.AddComponent<Camera>();
        
        // Add audio listener only to Player 1's camera
        if (isPlayer1)
        {
            cameraObj.AddComponent<AudioListener>();
            camera.rect = new Rect(0, 0.5f, 1, 0.5f);  // Top half
        }
        else
        {
            camera.rect = new Rect(0, 0, 1, 0.5f);     // Bottom half
        }

        // Position camera relative to car
        cameraObj.transform.position = car.transform.position - car.transform.forward * cameraDistance + Vector3.up * cameraHeight;
        cameraObj.transform.LookAt(car.transform);
        cameraObj.transform.parent = car.transform;
    }

    private void SetupAICar(GameObject car)
    {
        // Remove any existing backup controller
        var existingBackup = car.GetComponent<Backup>();
        if (existingBackup != null) Destroy(existingBackup);

        // Add AI controller
        var aiController = car.AddComponent<AICarController>();
        
        // Setup camera for AI car (bottom half)
        var cameraObj = new GameObject("AICamera");
        var camera = cameraObj.AddComponent<Camera>();
        camera.rect = new Rect(0, 0, 1, 0.5f);  // Bottom half
        cameraObj.transform.position = car.transform.position - car.transform.forward * cameraDistance + Vector3.up * cameraHeight;
        cameraObj.transform.LookAt(car.transform);
        cameraObj.transform.parent = car.transform;
    }

    private IEnumerator CountdownAndStartRace()
    {
        // Disable controllers at start
        if (player1Car != null)
        {
            var backupController = player1Car.GetComponent<Backup>();
            if (backupController != null) backupController.enabled = false;
        }
        
        if (player2Car != null)
        {
            if (isAIMode)
            {
                var aiController = player2Car.GetComponent<AICarController>();
                if (aiController != null) aiController.enabled = false;
            }
            else
            {
                var backupController = player2Car.GetComponent<Backup>();
                if (backupController != null) backupController.enabled = false;
            }
        }

        if (countdownText != null)
        {
            countdownText.text = "3";
            yield return new WaitForSeconds(1f);
            countdownText.text = "2";
            yield return new WaitForSeconds(1f);
            countdownText.text = "1";
            yield return new WaitForSeconds(1f);
            countdownText.text = "GO!";
            yield return new WaitForSeconds(0.5f);
            countdownText.gameObject.SetActive(false);
        }

        // Enable controllers when race begins
        if (player1Car != null)
        {
            var backupController = player1Car.GetComponent<Backup>();
            if (backupController != null) backupController.enabled = true;
        }
        
        if (player2Car != null)
        {
            if (isAIMode)
            {
                var aiController = player2Car.GetComponent<AICarController>();
                if (aiController != null)
                {
                    aiController.enabled = true;
                    aiController.StartRacing();
                }
            }
            else
            {
                var backupController = player2Car.GetComponent<Backup>();
                if (backupController != null) backupController.enabled = true;
            }
        }

        HasRaceStarted = true;
    }

    public void CleanupForSceneChange()
    {
        if (player1Car != null)
        {
            var backupController = player1Car.GetComponent<Backup>();
            if (backupController != null) backupController.enabled = false;
        }

        if (player2Car != null)
        {
            if (isAIMode)
            {
                var aiController = player2Car.GetComponent<AICarController>();
                if (aiController != null)
                {
                    aiController.enabled = false;
                    aiController.StopRacing();
                }
            }
            else
            {
                var backupController = player2Car.GetComponent<Backup>();
                if (backupController != null) backupController.enabled = false;
            }
        }

        player1Car = null;
        player2Car = null;
    }

    private void ResetCarInstant(GameObject car)
    {
        if (car == null) return;

        // Get the last checkpoint position and rotation from CheckpointManager
        Vector3 respawnPosition = checkpointManager != null 
            ? checkpointManager.GetLastCheckpointPosition(car.name)
            : car.transform.position;
        
        // Set position
        car.transform.position = respawnPosition + Vector3.up * 0.5f; // Lift slightly to prevent ground collision
    }

    private IEnumerator ReenablePhysics(Rigidbody rb)
    {
        yield return new WaitForSeconds(0.1f);
        if (rb != null)
        {
            rb.isKinematic = false;
        }
    }

    public void ResetCar(string playerName)
    {
        GameObject car = null;
        if (playerName == player1Car?.name)
        {
            car = player1Car;
        }
        else if (playerName == player2Car?.name)
        {
            car = player2Car;
        }

        if (car == null)
        {
            Debug.LogWarning($"Could not find car for player: {playerName}");
            return;
        }

        ResetCarInstant(car);
    }

    void ConfigurePlayer1Input(GameObject car)
    {
        // Configure WASD controls
        PlayerPrefs.SetString("Player1_Forward", KeyCode.W.ToString());
        PlayerPrefs.SetString("Player1_Back", KeyCode.S.ToString());
        PlayerPrefs.SetString("Player1_Left", KeyCode.A.ToString());
        PlayerPrefs.SetString("Player1_Right", KeyCode.D.ToString());
        PlayerPrefs.SetString("Player1_Brake", KeyCode.Space.ToString());
    }

    void ConfigurePlayer2Input(GameObject car)
    {
        // Configure Arrow key controls
        PlayerPrefs.SetString("Player2_Forward", KeyCode.UpArrow.ToString());
        PlayerPrefs.SetString("Player2_Back", KeyCode.DownArrow.ToString());
        PlayerPrefs.SetString("Player2_Left", KeyCode.LeftArrow.ToString());
        PlayerPrefs.SetString("Player2_Right", KeyCode.RightArrow.ToString());
        PlayerPrefs.SetString("Player2_Brake", KeyCode.RightControl.ToString());
    }

    void CreateCamera(GameObject car, bool isTopScreen)
    {
        GameObject cameraObj = new GameObject(isTopScreen ? "TopCamera" : "BottomCamera");
        Camera camera = cameraObj.AddComponent<Camera>();

        // Only add AudioListener to the top camera
        if (isTopScreen)
        {
            cameraObj.AddComponent<AudioListener>();
        }

        Vector3 cameraPosition = car.transform.position - car.transform.forward * cameraDistance + Vector3.up * cameraHeight;
        cameraObj.transform.position = cameraPosition;
        cameraObj.transform.LookAt(car.transform.position + car.transform.forward * 4f);

        camera.rect = isTopScreen ? new Rect(0, 0.5f, 1, 0.5f) : new Rect(0, 0, 1, 0.5f);
        cameraObj.transform.parent = car.transform;
    }

    public void UpdateCheckpoint(string playerName, Transform checkpoint)
    {
        if (checkpoint != null)
        {
            // Removed code here
            Debug.Log($"{playerName} passed checkpoint: {checkpoint.name}");
        }
    }

    private void Update()
    {
        // Handle manual resets
        if (Input.GetKeyDown(KeyCode.R) && player1Car != null)
        {
            ResetCar("Player1Car");
        }
        if (Input.GetKeyDown(KeyCode.P) && player2Car != null && !isAIMode)
        {
            ResetCar("Player2Car");
        }
    }

    private void OnDestroy()
    {
        // Clean up car references when scene is unloaded
        player1Car = null;
        player2Car = null;
    }

    private void OnDisable()
    {
        // Additional cleanup when component is disabled
        player1Car = null;
        player2Car = null;
    }

    // Helper class to handle input for both players
    public static class InputManager
    {
        public static float GetAxis(string axisName)
        {
            string carName = axisName.StartsWith("Player1") ? "Player1" : "Player2";
            
            if (axisName.Contains("Horizontal"))
            {
                bool right = GetKey($"{carName}_Right");
                bool left = GetKey($"{carName}_Left");
                return right ? 1f : (left ? -1f : 0f);
            }
            else if (axisName.Contains("Vertical"))
            {
                bool forward = GetKey($"{carName}_Forward");
                bool back = GetKey($"{carName}_Back");
                return forward ? 1f : (back ? -1f : 0f);
            }
            
            return 0f;
        }

        public static bool GetKey(string keyName)
        {
            string keyString = PlayerPrefs.GetString(keyName, "None");
            if (keyString == "None") return false;
            
            KeyCode key = (KeyCode)System.Enum.Parse(typeof(KeyCode), keyString);
            return Input.GetKey(key);
        }
    }
}

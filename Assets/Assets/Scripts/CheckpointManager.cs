using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class CheckpointManager : MonoBehaviour
{
    // Dictionary to store the last checkpoint for each car
    private Dictionary<string, Transform> lastCheckpoints = new Dictionary<string, Transform>();
    
    // List of all checkpoints in order
    public List<Transform> checkpoints = new List<Transform>();

    private void Awake()
    {
        // Find all checkpoints in the scene
        GameObject[] checkpointObjects = GameObject.FindGameObjectsWithTag("Checkpoint");
        
        // Sort checkpoints by their name (assuming names are like "Checkpoint0", "Checkpoint1", etc.)
        checkpoints = checkpointObjects
            .OrderBy(cp => {
                string numberStr = new string(cp.name.Where(char.IsDigit).ToArray());
                if (int.TryParse(numberStr, out int number))
                {
                    return number;
                }
                Debug.LogError($"Invalid checkpoint name format: {cp.name}. Expected format: Checkpoint0, Checkpoint1, etc.");
                return int.MaxValue; // Put invalid names at the end
            })
            .Select(cp => cp.transform)
            .ToList();

        if (checkpoints.Count == 0)
        {
            Debug.LogError("No checkpoints found in scene! Make sure checkpoints are tagged with 'Checkpoint' tag.");
        }
        else
        {
            Debug.Log($"Found {checkpoints.Count} checkpoints in scene.");
            // Log the order of checkpoints
            for (int i = 0; i < checkpoints.Count; i++)
            {
                Debug.Log($"Checkpoint {i}: {checkpoints[i].name}");
            }
        }
    }

    private void Start()
    {
        if (checkpoints.Count > 0)
        {
            // Find all cars in the scene
            Backup[] backupControllers = Object.FindObjectsByType<Backup>(FindObjectsSortMode.None);
            AICarController[] aiControllers = Object.FindObjectsByType<AICarController>(FindObjectsSortMode.None);

            // Set initial checkpoint for Backup cars
            foreach (Backup car in backupControllers)
            {
                if (!lastCheckpoints.ContainsKey(car.gameObject.name))
                {
                    lastCheckpoints.Add(car.gameObject.name, checkpoints[0]);
                }
            }

            // Set initial checkpoint for AI cars
            foreach (AICarController car in aiControllers)
            {
                if (!lastCheckpoints.ContainsKey(car.gameObject.name))
                {
                    lastCheckpoints.Add(car.gameObject.name, checkpoints[0]);
                }
            }
        }
    }

    // Called by Checkpoint script when a car passes through
    public void CarPassedCheckpoint(string carName, Transform checkpoint)
    {
        lastCheckpoints[carName] = checkpoint;
        Debug.Log($"{carName} passed checkpoint!");
    }

    // Get the last checkpoint position for a car
    public Vector3 GetLastCheckpointPosition(string carName)
    {
        if (lastCheckpoints.ContainsKey(carName) && lastCheckpoints[carName] != null)
        {
            return lastCheckpoints[carName].position;
        }
        
        // If no checkpoint found, return the first checkpoint position or zero
        if (checkpoints.Count > 0 && checkpoints[0] != null)
        {
            Debug.LogWarning($"No last checkpoint found for {carName}, using first checkpoint");
            return checkpoints[0].position;
        }
        
        Debug.LogError($"No checkpoints available for {carName}!");
        return Vector3.zero;
    }

    // Get the last checkpoint rotation for a car
    public Quaternion GetLastCheckpointRotation(string carName)
    {
        if (lastCheckpoints.ContainsKey(carName) && lastCheckpoints[carName] != null)
        {
            return lastCheckpoints[carName].rotation;
        }
        
        // If no checkpoint found, return the first checkpoint rotation or identity
        if (checkpoints.Count > 0 && checkpoints[0] != null)
        {
            Debug.LogWarning($"No last checkpoint found for {carName}, using first checkpoint");
            return checkpoints[0].rotation;
        }
        
        Debug.LogError($"No checkpoints available for {carName}!");
        return Quaternion.identity;
    }

    // Get the next checkpoint after a given checkpoint
    public Transform GetNextCheckpoint(Transform currentCheckpoint)
    {
        if (checkpoints.Count == 0) return null;

        int currentIndex = checkpoints.IndexOf(currentCheckpoint);
        if (currentIndex == -1) return checkpoints[0];

        // Return next checkpoint, or first checkpoint if at the end
        return currentIndex < checkpoints.Count - 1 ? checkpoints[currentIndex + 1] : checkpoints[0];
    }
}

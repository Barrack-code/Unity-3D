using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    public int checkpointNumber;  // Order in the race track
    private CheckpointManager checkpointManager;

    void Start()
    {
        // Find the checkpoint manager in the scene
        checkpointManager = Object.FindAnyObjectByType<CheckpointManager>();
        
        // Add this checkpoint to the manager's list
        if (checkpointManager != null && !checkpointManager.checkpoints.Contains(transform))
        {
            checkpointManager.checkpoints.Add(transform);
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        // Check for both types of car controllers
        CarController carController = other.GetComponent<CarController>();
        Backup backupController = other.GetComponent<Backup>();

        if (carController != null || backupController != null)
        {
            if (checkpointManager != null)
            {
                checkpointManager.CarPassedCheckpoint(other.gameObject.name, transform);
            }

            // Keep the existing race progress tracking if needed
            RaceProgressTracker progressTracker = other.GetComponent<RaceProgressTracker>();
            if (progressTracker != null)
            {
                progressTracker.PassCheckpoint(checkpointNumber);
            }
        }
    }
}

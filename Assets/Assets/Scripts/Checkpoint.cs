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
        // Check for car components
        var backupController = other.GetComponent<Backup>();
        var aiController = other.GetComponent<AICarController>();

        // Process checkpoint if we found a car
        if (backupController != null || aiController != null)
        {
            if (checkpointManager != null)
            {
                checkpointManager.CarPassedCheckpoint(other.gameObject.name, transform);
            }
            else
            {
                Debug.LogWarning("No checkpoint manager found!");
            }
        }
    }
}

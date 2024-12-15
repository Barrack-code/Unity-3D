using UnityEngine;

public class RaceProgressTracker : MonoBehaviour
{
    [Header("Race Progress")]
    public int currentLap = 0;
    public int currentCheckpoint = -1;
    public int totalLaps = 3;
    
    private CheckpointSystem checkpointSystem;
    private bool raceFinished = false;

    void Start()
    {
        // Try to find the checkpoint system
        checkpointSystem = FindFirstObjectByType<CheckpointSystem>();
        
        if (checkpointSystem == null)
        {
            Debug.LogError("No CheckpointSystem found in the scene!");
            return;
        }
    }

    public void PassCheckpoint(int checkpointNumber)
    {
        if (raceFinished || checkpointSystem == null) return;

        // Get total number of checkpoints
        int totalCheckpoints = checkpointSystem.transform.childCount;
        if (totalCheckpoints == 0) return;

        // Check if this is the next checkpoint in sequence
        if (checkpointNumber == (currentCheckpoint + 1) % totalCheckpoints)
        {
            currentCheckpoint = checkpointNumber;
            
            // If we've hit the first checkpoint again, increment lap
            if (checkpointNumber == 0 && currentLap > 0)
            {
                currentLap++;
                if (currentLap >= totalLaps)
                {
                    raceFinished = true;
                    Debug.Log(gameObject.name + " finished the race!");
                }
            }
            // If this is our first checkpoint ever, start counting laps
            else if (checkpointNumber == 0)
            {
                currentLap = 1;
            }

            Debug.Log($"{gameObject.name} passed checkpoint {checkpointNumber}, Lap {currentLap}");
        }
    }

    public float GetProgress()
    {
        if (checkpointSystem == null) return 0f;

        int totalCheckpoints = checkpointSystem.transform.childCount;
        if (totalCheckpoints == 0) return 0f;

        float checkpointProgress = (float)currentCheckpoint / totalCheckpoints;
        float lapProgress = (float)currentLap / totalLaps;
        
        return (lapProgress + checkpointProgress / totalLaps) * 100f;
    }
}

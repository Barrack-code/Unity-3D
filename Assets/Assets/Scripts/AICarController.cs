using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody))]
public class AICarController : MonoBehaviour
{
    [Header("Vehicle Settings")]
    [SerializeField] private float moveSpeed = 50f;     // Direct movement speed
    [SerializeField] private float rotationSpeed = 360f; // Faster rotation
    [SerializeField] private float waypointThreshold = 5f;
    [SerializeField] private float maxDistanceFromTrack = 5f;
    [SerializeField] private float heightOffset = 1f;
    [SerializeField] private float accelerationTime = 2f; // Time to reach full speed
    [SerializeField] private float initialSpeedMultiplier = 0.2f; // Start at 20% speed
    [SerializeField] private float finalSpeedMultiplier = 1.1f;   // End at 110% speed

    [Header("Track-Specific Settings")]
    [SerializeField] private float circuitSpeedMultiplier = 0.4f; // Slower on Circuit track
    [SerializeField] private float tunnelSpeedMultiplier = 1.0f;  // Slightly slower on Tunnel track for better control

    [Header("Audio Settings")]
    [SerializeField] private AudioSource engineAudio;
    [SerializeField] private float minPitch = 0.5f;
    [SerializeField] private float maxPitch = 1.5f;
    [SerializeField] private float pitchModifier = 0.1f;

    private Rigidbody rb;
    private bool isRacing = false;
    private bool isRespawning = false;
    private List<Transform> checkpoints;
    private int currentWaypointIndex = 0;
    private int lastPassedCheckpoint = 0;
    private Transform currentWaypoint;
    private float currentPitch;
    private bool hasPassedFirstCheckpoint = false;
    private float currentSpeed;
    private float accelerationProgress = 0f;
    private float trackSpeedMultiplier = 1f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError($"[AI] No Rigidbody found on {gameObject.name}");
            return;
        }

        // Configure rigidbody for kinematic movement
        rb.isKinematic = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        // Ensure we have a collider for finish line detection
        BoxCollider boxCollider = GetComponent<BoxCollider>();
        if (boxCollider == null)
        {
            boxCollider = gameObject.AddComponent<BoxCollider>();
            boxCollider.size = new Vector3(2f, 1f, 4f); // Typical car size
            boxCollider.center = new Vector3(0f, 0.5f, 0f);
            boxCollider.isTrigger = true;
        }

        // Check current track
        int selectedTrackIndex = PlayerPrefs.GetInt("SelectedTrack", 0);
        string selectedTrackScene = PlayerPrefs.GetString("SelectedTrackScene", "");
        Debug.Log($"[AI] Selected track index: {selectedTrackIndex}, Scene: {selectedTrackScene}");

        // Set speed based on track index (0 = Circuit, 1 = Tunnel)
        if (selectedTrackIndex == 0)  // Circuit track
        {
            trackSpeedMultiplier = circuitSpeedMultiplier;
            Debug.Log($"[AI] Using Circuit track speed multiplier: {circuitSpeedMultiplier}");
        }
        else  // Tunnel track
        {
            trackSpeedMultiplier = tunnelSpeedMultiplier;
            Debug.Log($"[AI] Using Tunnel track speed multiplier: {tunnelSpeedMultiplier}");
        }

        // Get checkpoints
        var checkpointManager = Object.FindAnyObjectByType<CheckpointManager>();
        if (checkpointManager != null)
        {
            checkpoints = checkpointManager.checkpoints;
            if (checkpoints.Count > 0)
            {
                currentWaypoint = checkpoints[0];
            }
        }

        Debug.Log($"[AI] Initialized for {gameObject.name}");
    }

    private void FixedUpdate()
    {
        if (!isRacing || isRespawning || checkpoints == null || checkpoints.Count == 0) return;

        currentWaypoint = checkpoints[currentWaypointIndex];
        if (currentWaypoint == null) return;

        // Update acceleration
        if (hasPassedFirstCheckpoint)
        {
            accelerationProgress = Mathf.Min(1f, accelerationProgress + (Time.fixedDeltaTime / accelerationTime));
        }
        else
        {
            accelerationProgress = Mathf.Min(0.3f, accelerationProgress + (Time.fixedDeltaTime / accelerationTime));
        }

        // Calculate speed with smooth acceleration and track multiplier
        float speedMultiplier;
        if (hasPassedFirstCheckpoint)
        {
            speedMultiplier = Mathf.Lerp(initialSpeedMultiplier, finalSpeedMultiplier, accelerationProgress);
        }
        else
        {
            speedMultiplier = Mathf.Lerp(0f, initialSpeedMultiplier, accelerationProgress);
        }

        // Apply track multiplier
        float targetSpeed = moveSpeed * speedMultiplier * trackSpeedMultiplier;
        Debug.Log($"[AI] Current speed: {targetSpeed} (Base: {moveSpeed}, Multiplier: {speedMultiplier}, Track: {trackSpeedMultiplier})");
        
        // Direct movement towards waypoint
        Vector3 directionToWaypoint = (currentWaypoint.position - transform.position).normalized;
        
        // Rotate towards waypoint instantly
        Quaternion targetRotation = Quaternion.LookRotation(directionToWaypoint);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);

        // Move directly towards waypoint with current speed
        Vector3 movement = directionToWaypoint * targetSpeed * Time.fixedDeltaTime;
        transform.position += movement;
        
        // Update current speed for audio
        currentSpeed = movement.magnitude / Time.fixedDeltaTime;

        // Check if reached waypoint
        if (Vector3.Distance(transform.position, currentWaypoint.position) < waypointThreshold)
        {
            lastPassedCheckpoint = currentWaypointIndex;
            currentWaypointIndex = (currentWaypointIndex + 1) % checkpoints.Count;
            
            // Check if we've completed a lap (reached checkpoint 0 again)
            if (currentWaypointIndex == 0 && hasPassedFirstCheckpoint)
            {
                Debug.Log($"[AI] {gameObject.name} completed the race!");
                StopRacing(); // Stop the AI car when it completes a lap
                return;
            }
            
            if (!hasPassedFirstCheckpoint && currentWaypointIndex > 0)
            {
                hasPassedFirstCheckpoint = true;
                Debug.Log($"[AI] {gameObject.name} passed first checkpoint - Increasing speed!");
                // Reset acceleration progress to create a new acceleration curve
                accelerationProgress = 0f;
            }
        }

        // Check if off track
        CheckTrackDistance();

        // Update engine sound
        UpdateEngineSound();
    }

    private void CheckTrackDistance()
    {
        RaycastHit hit;
        Vector3 rayStart = transform.position + Vector3.up * heightOffset;
        
        // Cast multiple rays to better detect track
        bool isOnTrack = false;
        float raySpacing = 1f;
        
        for (int x = -1; x <= 1; x++)
        {
            for (int z = -1; z <= 1; z++)
            {
                Vector3 offset = new Vector3(x * raySpacing, 0, z * raySpacing);
                if (Physics.Raycast(rayStart + offset, Vector3.down, out hit, maxDistanceFromTrack))
                {
                    // Check if we hit any ground/road object
                    if (hit.collider != null)
                    {
                        isOnTrack = true;
                        break;
                    }
                }
            }
            if (isOnTrack) break;
        }

        if (!isOnTrack)
        {
            Debug.Log($"[AI] {gameObject.name} off track - Moving to checkpoint {lastPassedCheckpoint}");
            MoveToLastCheckpoint();
        }
    }

    private void MoveToLastCheckpoint()
    {
        if (isRespawning) return;
        if (lastPassedCheckpoint >= checkpoints.Count) return;

        isRespawning = true;
        Transform respawnPoint = checkpoints[lastPassedCheckpoint];
        
        // Stop all movement
        transform.position = respawnPoint.position + Vector3.up * 0.5f;
        transform.rotation = respawnPoint.rotation;
        
        // Set next target
        currentWaypointIndex = (lastPassedCheckpoint + 1) % checkpoints.Count;
        
        Debug.Log($"[AI] Moved to checkpoint {lastPassedCheckpoint}, targeting {currentWaypointIndex}");
        isRespawning = false;
    }

    private void UpdateEngineSound()
    {
        if (engineAudio != null && engineAudio.clip != null)
        {
            float speedRatio = currentSpeed / moveSpeed; // Normalized speed ratio
            float targetPitch = minPitch + (maxPitch - minPitch) * speedRatio;
            currentPitch = Mathf.Lerp(currentPitch, targetPitch, Time.fixedDeltaTime * pitchModifier);
            engineAudio.pitch = currentPitch;
        }
    }

    public void StartRacing()
    {
        isRacing = true;
        accelerationProgress = 0f;
        if (engineAudio != null && engineAudio.clip != null)
        {
            engineAudio.Play();
        }
        Debug.Log($"[AI] Started racing for {gameObject.name}");
    }

    public void StopRacing()
    {
        isRacing = false;
        if (engineAudio != null)
        {
            engineAudio.Stop();
        }
        Debug.Log($"[AI] {gameObject.name} stopped racing");
    }

    public void StopEngine()
    {
        if (engineAudio != null)
        {
            engineAudio.Stop();
        }
    }

    public void MuteEngine(bool mute)
    {
        if (engineAudio != null)
        {
            engineAudio.mute = mute;
        }
    }
}

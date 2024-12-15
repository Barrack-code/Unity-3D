using UnityEngine;

public class SpawnPointManager : MonoBehaviour
{
    [Header("Spawn Point Settings")]
    public float spawnHeight = 0.5f;    // Height above track
    public float lateralSpacing = 4f;    // Space between spawn points
    public float forwardOffset = 2f;     // Distance from start line
    public Color gizmoColor = Color.green;
    public float gizmoSize = 1f;

    void OnDrawGizmos()
    {
        // Draw spawn point indicators
        Gizmos.color = gizmoColor;
        
        // For each child (spawn point)
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform spawnPoint = transform.GetChild(i);
            
            // Draw sphere at spawn position
            Gizmos.DrawWireSphere(spawnPoint.position, gizmoSize);
            
            // Draw forward direction
            Gizmos.DrawLine(spawnPoint.position, 
                           spawnPoint.position + spawnPoint.forward * 2f);
            
            // Draw ground projection
            Gizmos.color = new Color(gizmoColor.r, gizmoColor.g, gizmoColor.b, 0.3f);
            Gizmos.DrawLine(spawnPoint.position, 
                           spawnPoint.position + Vector3.down * spawnHeight);
        }
    }

    // Helper method to automatically position spawn points
    public void AutoPositionSpawnPoints()
    {
        Vector3 basePosition = transform.position;
        
        // Calculate start position for first spawn point
        float startX = -(lateralSpacing * 0.5f) * (transform.childCount - 1);
        
        // Position each spawn point
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform spawnPoint = transform.GetChild(i);
            Vector3 newPos = basePosition + new Vector3(
                startX + (lateralSpacing * i),
                spawnHeight,
                forwardOffset
            );
            spawnPoint.position = newPos;
            spawnPoint.rotation = transform.rotation;
        }
    }

#if UNITY_EDITOR
    // Add menu item to auto-position spawn points
    [UnityEditor.MenuItem("Racing Game/Auto Position Spawn Points")]
    static void AutoPositionSpawnPointsMenuItem()
    {
        SpawnPointManager manager = Object.FindFirstObjectByType<SpawnPointManager>();
        if (manager != null)
        {
            manager.AutoPositionSpawnPoints();
            UnityEditor.SceneView.RepaintAll();
        }
    }
#endif
}

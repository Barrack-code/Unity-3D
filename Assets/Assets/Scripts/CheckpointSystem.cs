using UnityEngine;
using System.Collections.Generic;

public class CheckpointSystem : MonoBehaviour
{
    [Header("Checkpoint Settings")]
    public float checkpointWidth = 10f;      // Width of checkpoint trigger
    public float checkpointHeight = 5f;      // Height of checkpoint trigger
    public Color checkpointColor = new Color(0, 1, 0, 0.3f);
    public Color startLineColor = new Color(1, 0, 0, 0.3f);
    public Color finishLineColor = new Color(0, 0, 1, 0.3f);

    // List of all checkpoints in order
    private List<Checkpoint> checkpoints = new List<Checkpoint>();
    
    void Start()
    {
        // Get all checkpoints and sort them by their number
        Checkpoint[] points = GetComponentsInChildren<Checkpoint>();
        foreach (Checkpoint cp in points)
        {
            checkpoints.Add(cp);
        }
        checkpoints.Sort((a, b) => a.checkpointNumber.CompareTo(b.checkpointNumber));
    }

    public Checkpoint GetNextCheckpoint(int currentCheckpoint)
    {
        int nextIndex = (currentCheckpoint + 1) % checkpoints.Count;
        return checkpoints[nextIndex];
    }

    // Draw checkpoint visualization in editor
    void OnDrawGizmos()
    {
        foreach (Transform child in transform)
        {
            Checkpoint cp = child.GetComponent<Checkpoint>();
            if (cp != null)
            {
                // Choose color based on checkpoint type
                if (cp.checkpointNumber == 0)
                    Gizmos.color = startLineColor;
                else if (cp.checkpointNumber == transform.childCount - 1)
                    Gizmos.color = finishLineColor;
                else
                    Gizmos.color = checkpointColor;

                // Draw checkpoint trigger area
                Vector3 size = new Vector3(checkpointWidth, checkpointHeight, 0.5f);
                Gizmos.matrix = Matrix4x4.TRS(child.position, child.rotation, Vector3.one);
                Gizmos.DrawCube(Vector3.zero, size);
                
                // Draw checkpoint number
                UnityEditor.Handles.Label(child.position, "CP " + cp.checkpointNumber);
            }
        }
    }
}

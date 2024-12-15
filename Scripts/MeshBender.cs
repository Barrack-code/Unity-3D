using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class MeshBender : MonoBehaviour
{
    [Header("Bend Settings")]
    [Range(0f, 360f)]
    public float bendAngle = 90f;
    [Range(0.1f, 10f)]
    public float bendRadius = 2f;

    private Mesh originalMesh;
    private Mesh clonedMesh;
    private Vector3[] originalVertices;
    private Vector3[] bentVertices;

    void Start()
    {
        // Get the original mesh
        originalMesh = GetComponent<MeshFilter>().mesh;
        
        // Create a clone of the mesh to modify
        clonedMesh = new Mesh();
        clonedMesh.name = "Bent Mesh";
        
        // Copy the original mesh data
        clonedMesh.vertices = originalMesh.vertices;
        clonedMesh.triangles = originalMesh.triangles;
        clonedMesh.normals = originalMesh.normals;
        clonedMesh.uv = originalMesh.uv;
        
        // Store original vertices
        originalVertices = originalMesh.vertices;
        bentVertices = new Vector3[originalVertices.Length];
        
        // Assign the cloned mesh
        GetComponent<MeshFilter>().mesh = clonedMesh;
        
        // Initial bend
        BendMesh();
    }

    void Update()
    {
        BendMesh();
    }

    void BendMesh()
    {
        float angleInRad = bendAngle * Mathf.Deg2Rad;
        
        for (int i = 0; i < originalVertices.Length; i++)
        {
            Vector3 vertex = originalVertices[i];
            
            // Calculate the bend based on the vertex position
            float zCoord = vertex.z;
            float bendPercentage = zCoord / bendRadius;
            float rotation = bendPercentage * angleInRad;
            
            // Calculate the new position
            float radius = bendRadius + vertex.x;
            Vector3 newPos = new Vector3(
                Mathf.Sin(rotation) * radius,
                vertex.y,
                Mathf.Cos(rotation) * radius - bendRadius
            );
            
            bentVertices[i] = newPos;
        }
        
        // Update the mesh
        clonedMesh.vertices = bentVertices;
        clonedMesh.RecalculateNormals();
        clonedMesh.RecalculateBounds();
    }
}

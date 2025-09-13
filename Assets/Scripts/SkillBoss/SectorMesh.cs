using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class SectorMesh : MonoBehaviour
{
    public float radius = 5f;
    public float angle = 90f; // góc quạt (độ)
    public int segments = 30; // càng nhiều càng mượt

    void Start()
    {
        MeshFilter mf = GetComponent<MeshFilter>();
        mf.mesh = CreateSectorMesh();
    }

    Mesh CreateSectorMesh()
    {
        Mesh mesh = new Mesh();

        int vertCount = segments + 2;
        Vector3[] vertices = new Vector3[vertCount];
        int[] triangles = new int[segments * 3];

        vertices[0] = Vector3.zero;
        float angleStep = angle / segments;

        for (int i = 0; i <= segments; i++)
        {
            float currentAngle = -angle / 2 + angleStep * i;
            float rad = currentAngle * Mathf.Deg2Rad;
            vertices[i + 1] = new Vector3(Mathf.Sin(rad), 0, Mathf.Cos(rad)) * radius;
        }

        for (int i = 0; i < segments; i++)
        {
            triangles[i * 3] = 0;
            triangles[i * 3 + 1] = i + 1;
            triangles[i * 3 + 2] = i + 2;
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        return mesh;
    }
}

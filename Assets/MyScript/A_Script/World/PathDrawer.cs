using UnityEngine;
using System.Collections.Generic;

public class PathDrawer : MonoBehaviour
{
    public static PathDrawer Instance;

    Mesh pathMesh;
    [SerializeField] Material pathMaterial;

    void Awake()
    {
        Instance = this;

        pathMesh = new Mesh();
    }

    public void DrawPath(Vector2 pawnPos, IEnumerable<Vector2Int> path)
    {
        if (path == null) return;

        List<Vector3> verts = new();
        List<int> tris = new();
        List<Vector2> uvs = new();

        float width = 0.05f;
        int index = 0;

        // START FROM PAWN POSITION
        Vector3? last = new Vector3(pawnPos.x, pawnPos.y - 0.5f, 0);

        foreach (var node in path)
        {
            Vector3 p = new Vector3(node.x, node.y - 0.5f, 0);

            Vector3 dir = (p - last.Value).normalized;
            Vector3 normal = new Vector2(-dir.y, dir.x) * width;

            Vector3 v0 = last.Value + normal;
            Vector3 v1 = last.Value - normal;
            Vector3 v2 = p + normal;
            Vector3 v3 = p - normal;

            verts.Add(v0);
            verts.Add(v1);
            verts.Add(v2);
            verts.Add(v3);

            tris.Add(index + 0);
            tris.Add(index + 2);
            tris.Add(index + 1);

            tris.Add(index + 2);
            tris.Add(index + 3);
            tris.Add(index + 1);

            uvs.Add(Vector2.zero);
            uvs.Add(Vector2.right);
            uvs.Add(Vector2.up);
            uvs.Add(Vector2.one);

            index += 4;

            last = p;
        }

        pathMesh.Clear();
        pathMesh.SetVertices(verts);
        pathMesh.SetTriangles(tris, 0);
        pathMesh.SetUVs(0, uvs);

        Graphics.DrawMesh(
            pathMesh,
            Matrix4x4.identity,
            pathMaterial,
            0
        );
    }
}
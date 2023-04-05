using System.Collections.Generic;
using UnityEngine;

public static class Icosahedron
{
    /// <summary>
    /// This is a special version of an icosahedron with unwelded vertices with barycentric uv coordinates
    /// </summary>
    /// <param name="subdivideLevel">Number of times to subdivide the icosahedron</param>
    /// <returns>Mesh</returns>
    public static Mesh CreateIcosahedronWithBarycentricUVs(int subdivideLevel)
    {
        // Create initial low poly icosahedron
        var vertices = new List<Vector3>();
        CreateIcosahedronVertices(vertices);

        // Subdivide the icosahedron to add more polygons.
        // This will also add the barycentric coordinates and tangents.
        var barycentric = new List<Vector2>();
        var tangents = new List<Vector4>();
        Subdivide(subdivideLevel, ref vertices, ref barycentric, ref tangents);

        // Calculating the normal could also be done in the shader,
        // given that we're using a unit sphere.
        var normals = new List<Vector3>();
        CreateNormals(vertices, normals);

        int[] triangles = new int[vertices.Count];
        CreateTriangles(vertices.Count, triangles);

        Mesh mesh = new Mesh();

        mesh.vertices = vertices.ToArray();
        mesh.uv = barycentric.ToArray();
        mesh.normals = normals.ToArray();
        mesh.tangents = tangents.ToArray();
        mesh.triangles = triangles;

        return mesh;
    }

    private static void AddVertex(List<Vector3> vertices, Vector3 v)
    {
        v.Normalize();
        vertices.Add(v);
    }

    private static void CalculateTangent(List<Vector4> tangents, ref Vector3 a, ref Vector3 b)
    {
        Vector4 t = b - a;
        t.Normalize();
        tangents.Add(t);
        tangents.Add(t);
        tangents.Add(t);
    }

    private static void CreateIcosahedronVertices(List<Vector3> vertices)
    {
        float t = (1.0f + Mathf.Sqrt(5.0f)) / 2.0f;

        AddVertex(vertices, new Vector3(-1.0f, t, 0.0f));
        AddVertex(vertices, new Vector3(-t, 0.0f, 1.0f));
        AddVertex(vertices, new Vector3(0.0f, 1.0f, t));

        AddVertex(vertices, new Vector3(-1.0f, t, 0.0f));
        AddVertex(vertices, new Vector3(0.0f, 1.0f, t));
        AddVertex(vertices, new Vector3(1.0f, t, 0.0f));

        AddVertex(vertices, new Vector3(-1.0f, t, 0.0f));
        AddVertex(vertices, new Vector3(1.0f, t, 0.0f));
        AddVertex(vertices, new Vector3(0.0f, 1.0f, -t));

        AddVertex(vertices, new Vector3(-1.0f, t, 0.0f));
        AddVertex(vertices, new Vector3(0.0f, 1.0f, -t));
        AddVertex(vertices, new Vector3(-t, 0.0f, -1.0f));

        AddVertex(vertices, new Vector3(-1.0f, t, 0.0f));
        AddVertex(vertices, new Vector3(-t, 0.0f, -1.0f));
        AddVertex(vertices, new Vector3(-t, 0.0f, 1.0f));

        AddVertex(vertices, new Vector3(1.0f, t, 0.0f));
        AddVertex(vertices, new Vector3(0.0f, 1.0f, t));
        AddVertex(vertices, new Vector3(t, 0.0f, 1.0f));

        AddVertex(vertices, new Vector3(0.0f, 1.0f, t));
        AddVertex(vertices, new Vector3(-t, 0.0f, 1.0f));
        AddVertex(vertices, new Vector3(0.0f, -1.0f, t));

        AddVertex(vertices, new Vector3(-t, 0.0f, 1.0f));
        AddVertex(vertices, new Vector3(-t, 0.0f, -1.0f));
        AddVertex(vertices, new Vector3(-1.0f, -t, 0.0f));

        AddVertex(vertices, new Vector3(-t, 0.0f, -1.0f));
        AddVertex(vertices, new Vector3(0.0f, 1.0f, -t));
        AddVertex(vertices, new Vector3(0.0f, -1.0f, -t));

        AddVertex(vertices, new Vector3(0.0f, 1.0f, -t));
        AddVertex(vertices, new Vector3(1.0f, t, 0.0f));
        AddVertex(vertices, new Vector3(t, 0.0f, -1.0f));

        AddVertex(vertices, new Vector3(1.0f, -t, 0.0f));
        AddVertex(vertices, new Vector3(t, 0.0f, 1.0f));
        AddVertex(vertices, new Vector3(0.0f, -1.0f, t));

        AddVertex(vertices, new Vector3(1.0f, -t, 0.0f));
        AddVertex(vertices, new Vector3(0.0f, -1.0f, t));
        AddVertex(vertices, new Vector3(-1.0f, -t, 0.0f));

        AddVertex(vertices, new Vector3(1.0f, -t, 0.0f));
        AddVertex(vertices, new Vector3(-1.0f, -t, 0.0f));
        AddVertex(vertices, new Vector3(0.0f, -1.0f, -t));

        AddVertex(vertices, new Vector3(1.0f, -t, 0.0f));
        AddVertex(vertices, new Vector3(0.0f, -1.0f, -t));
        AddVertex(vertices, new Vector3(t, 0.0f, -1.0f));

        AddVertex(vertices, new Vector3(1.0f, -t, 0.0f));
        AddVertex(vertices, new Vector3(t, 0.0f, -1.0f));
        AddVertex(vertices, new Vector3(t, 0.0f, 1.0f));

        AddVertex(vertices, new Vector3(0.0f, -1.0f, t));
        AddVertex(vertices, new Vector3(t, 0.0f, 1.0f));
        AddVertex(vertices, new Vector3(0.0f, 1.0f, t));

        AddVertex(vertices, new Vector3(-1.0f, -t, 0.0f));
        AddVertex(vertices, new Vector3(0.0f, -1.0f, t));
        AddVertex(vertices, new Vector3(-t, 0.0f, 1.0f));

        AddVertex(vertices, new Vector3(0.0f, -1.0f, -t));
        AddVertex(vertices, new Vector3(-1.0f, -t, 0.0f));
        AddVertex(vertices, new Vector3(-t, 0.0f, -1.0f));

        AddVertex(vertices, new Vector3(t, 0.0f, -1.0f));
        AddVertex(vertices, new Vector3(0.0f, -1.0f, -t));
        AddVertex(vertices, new Vector3(0.0f, 1.0f, -t));

        AddVertex(vertices, new Vector3(t, 0.0f, 1.0f));
        AddVertex(vertices, new Vector3(t, 0.0f, -1.0f));
        AddVertex(vertices, new Vector3(1.0f, t, 0.0f));
    }

    private static void Subdivide(int subdivideLevel, ref List<Vector3> vertices, ref List<Vector2> barycentric, ref List<Vector4> tangents)
    {
        for (int i = 0; i < subdivideLevel; ++i)
        {
            var vertices2 = new List<Vector3>();
            var barycentric2 = new List<Vector2>();
            var tangents2 = new List<Vector4>();

            for (int j = 0; j < vertices.Count; j += 3)
            {
                Vector3 a = vertices[j];
                Vector3 b = vertices[j + 1];
                Vector3 c = vertices[j + 2];

                Vector3 m1 = 0.5f * (a + b);
                Vector3 m2 = 0.5f * (b + c);
                Vector3 m3 = 0.5f * (c + a);

                AddVertex(vertices2, a);
                AddVertex(vertices2, m1);
                AddVertex(vertices2, m3);
                barycentric2.Add(new Vector2(1.0f, 0.0f));
                barycentric2.Add(new Vector2(0.0f, 1.0f));
                barycentric2.Add(new Vector2(0.0f, 0.0f));
                CalculateTangent(tangents2, ref a, ref m1);

                AddVertex(vertices2, b);
                AddVertex(vertices2, m2);
                AddVertex(vertices2, m1);
                barycentric2.Add(new Vector2(1.0f, 0.0f));
                barycentric2.Add(new Vector2(0.0f, 1.0f));
                barycentric2.Add(new Vector2(0.0f, 0.0f));
                CalculateTangent(tangents2, ref b, ref m2);

                AddVertex(vertices2, c);
                AddVertex(vertices2, m3);
                AddVertex(vertices2, m2);
                barycentric2.Add(new Vector2(1.0f, 0.0f));
                barycentric2.Add(new Vector2(0.0f, 1.0f));
                barycentric2.Add(new Vector2(0.0f, 0.0f));
                CalculateTangent(tangents2, ref c, ref m3);

                AddVertex(vertices2, m1);
                AddVertex(vertices2, m2);
                AddVertex(vertices2, m3);
                barycentric2.Add(new Vector2(1.0f, 0.0f));
                barycentric2.Add(new Vector2(0.0f, 1.0f));
                barycentric2.Add(new Vector2(0.0f, 0.0f));
                CalculateTangent(tangents2, ref m1, ref m2);
            }

            vertices = vertices2;
            barycentric = barycentric2;
            tangents = tangents2;
        }
    }

    private static void CreateNormals(List<Vector3> vertices, List<Vector3> normals)
    {
        for (int i = 0; i < vertices.Count; ++i)
        {
            Vector3 n = vertices[i];
            n.Normalize();
            normals.Add(n);
        }
    }

    private static void CreateTriangles(int vertexCount, int[] triangles)
    {
        for (int i = 0; i < vertexCount; ++i)
        {
            triangles[i] = i;
        }
    }
}

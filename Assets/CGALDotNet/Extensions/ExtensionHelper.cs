using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

using CGALDotNet;
using CGALDotNetGeometry.Numerics;
using CGALDotNetGeometry.Shapes;
using CGALDotNet.Polyhedra;

public static class ExtensionHelper
{
    private static StringBuilder builder = new StringBuilder();

    public static void PrintObjectToUnity(this CGALObject obj)
    {
        builder.Clear();
        obj.Print(builder);
        Debug.Log(builder);
    }

    public static void PrintMeshToUnity(this IMesh obj)
    {
        builder.Clear();
        obj.Print(builder);
        Debug.Log(builder);
    }

    public static GameObject CreateGameobject(string name, Mesh mesh, Vector3 translation, Material material)
    {
        GameObject go = new GameObject(name);
        go.AddComponent<MeshRenderer>().material = material;
        go.AddComponent<MeshFilter>().mesh = mesh;
        go.transform.localPosition = translation;
        return go;
    }

    public static Mesh CreateMesh(Point3d[] points, int[] indices)
    {
        Mesh mesh = new Mesh();
        mesh.SetVertices(points.ToUnityVector3());
        mesh.SetTriangles(indices, 0);
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        return mesh;
    }

    public static Mesh CreateMeshXZ(Point2d[] points, int[] indices)
    {
        Mesh mesh = new Mesh();
        mesh.SetVertices(points.ToUnityVector3XZ());
        mesh.SetTriangles(indices, 0);
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        return mesh;
    }

    public static Mesh CreateMesh(Point2d[] points, int[] indices)
    {
        Mesh mesh = new Mesh();
        mesh.SetVertices(points.ToUnityVector3());
        mesh.SetTriangles(indices, 0);
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        return mesh;
    }

    public static Mesh CreateMesh(Vector3[] points, Color[] colors, int[] indices)
    {
        Mesh mesh = new Mesh();
        mesh.SetVertices(points);
        mesh.SetColors(colors);
        mesh.SetTriangles(indices, 0);
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        return mesh;
    }

    public static Mesh CreateMesh(List<Vector3> points, List<Color> colors, List<int> indices)
    {
        Mesh mesh = new Mesh();
        mesh.SetVertices(points);
        mesh.SetColors(colors);
        mesh.SetTriangles(indices, 0);
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        return mesh;
    }

    public static void SplitFaces(Point3d[] points, int[] indices, out Point3d[] splitPoints, out int[] splitIndices)
    {
        int triangles = indices.Length / 3;

        splitPoints = new Point3d[triangles * 3];
        splitIndices = new int[triangles * 3];

        for (int i = 0; i < triangles; i++)
        {
            var a = points[indices[i * 3 + 0]];
            var b = points[indices[i * 3 + 1]];
            var c = points[indices[i * 3 + 2]];

            splitPoints[i * 3 + 0] = a;
            splitPoints[i * 3 + 1] = b;
            splitPoints[i * 3 + 2] = c;

            splitIndices[i * 3 + 0] = i * 3 + 0;
            splitIndices[i * 3 + 1] = i * 3 + 1;
            splitIndices[i * 3 + 2] = i * 3 + 2;
        }

    }

}

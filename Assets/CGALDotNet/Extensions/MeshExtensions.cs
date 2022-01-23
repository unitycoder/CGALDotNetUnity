using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using CGALDotNet;
using CGALDotNet.Geometry;
using CGALDotNet.Polygons;
using CGALDotNet.Polyhedra;

public static class CGALMeshExtensions
{
    public static Polyhedron3<K> ToCGALPolyhedron3<K>(this Mesh mesh) where K : CGALKernel, new()
    {
        int[] triangles = mesh.triangles;
        var points = mesh.vertices.ToCGALPoint3d();

        var poly = new Polyhedron3<K>();

        if (points.Length > 0 && triangles.Length > 0)
            poly.CreateTriangleMesh(points, triangles);

        return poly;
    }

}



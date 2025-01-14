using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

using CGALDotNet;
using CGALDotNetGeometry.Numerics;
using CGALDotNetGeometry.Shapes;
using CGALDotNet.Polyhedra;
using CGALDotNet.Processing;
using CGALDotNet.Polylines;

using Common.Unity.Drawing;

namespace CGALDotNetUnity.Processing
{

    public enum SELECT_MODE
    {
        FACE, VERTEX, EDGE
    }

    public class ProcessingExample : MonoBehaviour
    {

        public Color lineColor = Color.black;

        public Color vertexNormalColor = Color.red;

        public Color faceNormalColor = Color.blue;

        public Material material;

        private Polyhedron3<EIK> m_mesh;

        private GameObject m_object;

        private SegmentRenderer m_wireframe, m_featureRenderer;

        private NormalRenderer m_vertNormalRenderer, m_faceNormalRenderer;

        private string m_info;

        private double m_refineFactor = 3;

        private double m_featureAngle = 60;

        private double m_targetEdgeLen = 0.05;

        private SELECT_MODE m_selectionMode = SELECT_MODE.FACE;

        private MeshFace3? m_hitFace;

        private MeshVertex3? m_hitVertex;

        private MeshHalfedge3? m_hitEdge;

        private void Start()
        {

        }

        private void CreateGameobject(string name, Polyhedron3 poly, Vector3 pos, Quaternion rot, Vector3 scale)
        {
            m_object = poly.ToUnityMesh(name, material, false);
            m_object.transform.position = pos;
            m_object.transform.rotation = rot;
            m_object.transform.localScale = scale;
        }

        private void RebuildGameobject(Polyhedron3 poly)
        {
            var go = poly.ToUnityMesh(m_object.name, material, false);
            go.transform.position = m_object.transform.position;
            go.transform.rotation = m_object.transform.rotation;
            go.transform.localScale = m_object.transform.localScale;
            Destroy(m_object);
            m_object = go;
        }

        private void CreateFeatureRenderer(List<int> edges)
        {
            m_featureRenderer = new SegmentRenderer();
            m_featureRenderer.DefaultColor = Color.red;

            foreach(var edge in edges)
            {
                Segment3d seg;
                if(m_mesh.GetSegment(edge, out seg))
                {
                    var a = seg.A.ToUnityVector3();
                    var b = seg.B.ToUnityVector3();
                    m_featureRenderer.Load(a, b);
                }
            }
    
        }

        private void CreateWireFrame()
        {
            bool enabled = m_wireframe != null ? m_wireframe.Enabled : true;
            m_wireframe = RendererBuilder.CreateWireframeRenderer(m_mesh, lineColor);
            m_wireframe.Enabled = enabled;
        }

        private void ToggleWireFrame()
        {
            if (m_wireframe == null)
            {
                m_wireframe = RendererBuilder.CreateWireframeRenderer(m_mesh, lineColor);
            }
            else if(m_wireframe != null)
            {
                m_wireframe.Enabled = !m_wireframe.Enabled;
            }
        }

        private void ToggleVertexNormals()
        {
            if (m_vertNormalRenderer == null)
            {
                m_vertNormalRenderer = RendererBuilder.CreateVertexNormalRenderer(m_mesh, vertexNormalColor, 0.01f);
            }
            else if (m_vertNormalRenderer != null)
            {
                m_vertNormalRenderer.Enabled = !m_vertNormalRenderer.Enabled;
            }
        }

        private void ToggleFaceNormals()
        {
            if (m_faceNormalRenderer == null)
            {
                m_faceNormalRenderer = RendererBuilder.CreateFaceNormalRenderer(m_mesh, faceNormalColor, 0.01f);
            }
            else if (m_faceNormalRenderer != null)
            {
                m_faceNormalRenderer.Enabled = !m_faceNormalRenderer.Enabled;
            }
        }

        private void LoadMesh(string file)
        {
            string filename = Application.dataPath + "/Examples/Data/" + file;

            var split = filename.Split('/', '.');
            int i = split.Length - 2;
            var name = i > 0 ? split[i] : "Mesh";

            m_mesh = new Polyhedron3<EIK>();
            m_mesh.ReadOFF(filename);;
        }

        private void ClearLast()
        {
            m_featureRenderer = null;
            m_vertNormalRenderer = null;
            m_faceNormalRenderer = null;
            m_info = "";
        }

        private void OnRenderObject()
        {
            if(m_wireframe != null && m_wireframe.Enabled)
            {
                m_wireframe.SetColor(lineColor);
                m_wireframe.LocalToWorld = m_object.transform.localToWorldMatrix;
                m_wireframe.Draw();
            }

            if(m_featureRenderer != null && m_featureRenderer.Enabled)
            {
                m_featureRenderer.LocalToWorld = m_object.transform.localToWorldMatrix;
                m_featureRenderer.Draw();
            }

            if (m_vertNormalRenderer != null && m_vertNormalRenderer.Enabled)
            {
                m_vertNormalRenderer.SetColor(vertexNormalColor);
                m_vertNormalRenderer.LocalToWorld = m_object.transform.localToWorldMatrix;
                m_vertNormalRenderer.Draw();
            }

            if (m_faceNormalRenderer != null && m_faceNormalRenderer.Enabled)
            {
                m_faceNormalRenderer.SetColor(faceNormalColor);
                m_faceNormalRenderer.LocalToWorld = m_object.transform.localToWorldMatrix;
                m_faceNormalRenderer.Draw();
            }

        }

        private void OnLeftClick()
        {

            if (m_mesh != null && Input.GetMouseButtonDown(0))
            {
                var ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                if (m_selectionMode == SELECT_MODE.FACE)
                {
                    if (m_mesh.LocateFace(ray.ToCGALRay3d(), out MeshFace3 face))
                    {
                        m_hitFace = face;
                        m_info = "Hit Face = " + m_hitFace.ToString();
                    }
                    else
                    {
                        m_hitFace = null;
                    }
                }
                else if (m_selectionMode == SELECT_MODE.VERTEX)
                {
                    if (m_mesh.LocateVertex(ray.ToCGALRay3d(), 0.01, out MeshVertex3 vertex))
                    {
                        m_hitVertex = vertex;
                        m_hitVertex.Value.Point.Round(4);
                        m_info = "Hit Vertex = " + m_hitVertex.ToString();
                    }
                    else
                    {
                        m_hitVertex = null;
                    }
                }
                else if (m_selectionMode == SELECT_MODE.EDGE)
                {
                    if (m_mesh.LocateHalfedge(ray.ToCGALRay3d(), 0.01, out MeshHalfedge3 edge))
                    {
                        m_hitEdge = edge;
                        m_info = "Hit Halfedge = " + m_hitEdge.ToString();
                    }
                    else
                    {
                        m_hitEdge = null;
                    }
                }
            }

        }

        private void Update()
        {

            OnLeftClick();

            if(m_mesh != null && Input.GetKeyDown(KeyCode.Space))
            {
                if (m_object != null)
                {
                    Destroy(m_object);
                    m_mesh = null;
                    m_wireframe = null;
                    m_featureRenderer = null;
                    m_vertNormalRenderer = null;
                    m_faceNormalRenderer = null;
                    m_info = "";
                }
            }

            if (m_mesh == null)
            {
                if (Input.GetKeyDown(KeyCode.F1))
                {
                    if (m_object != null)
                        Destroy(m_object);

                    var pos = new Vector3(0, 0, 0.5f);
                    var rot = Quaternion.Euler(0, 180, 0);
                    var scale = Vector3.one;

                    LoadMesh("bunny00.off");
                    CreateGameobject("Bunny", m_mesh, pos, rot, scale);
                    CreateWireFrame();

                }
                else if (Input.GetKeyDown(KeyCode.F2))
                {
                    if (m_object != null)
                        Destroy(m_object);

                    var pos = Vector3.zero;
                    var rot = Quaternion.identity;
                    var scale = Vector3.one;

                    LoadMesh("elephant.off");
                    CreateGameobject("elephant", m_mesh, pos, rot, scale);
                    CreateWireFrame();
                }
                else if (Input.GetKeyDown(KeyCode.F3))
                {
                    if (m_object != null)
                        Destroy(m_object);

                    var pos = new Vector3(0, 0, 4f);
                    var rot = Quaternion.Euler(-90, 0, 180);
                    var scale = new Vector3(0.1f, 0.1f, 0.1f);

                    LoadMesh("mannequin-devil.off");
                    CreateGameobject("mannequin", m_mesh, pos, rot, scale);
                    CreateWireFrame();
                }
                else if (Input.GetKeyDown(KeyCode.F4))
                {
                    if (m_object != null)
                        Destroy(m_object);

                    var pos = Vector3.zero;
                    var rot = Quaternion.Euler(180, 90, 0);
                    var scale = Vector3.one;

                    LoadMesh("fandisk.off");
                    CreateGameobject("fandisk", m_mesh, pos, rot, scale);
                    CreateWireFrame();
                }
            }
            else
            {
                if (Input.GetKeyDown(KeyCode.Tab))
                {
                    m_selectionMode = m_selectionMode.Next();
                }
                else if (Input.GetKeyDown(KeyCode.F1))
                {
                    ToggleWireFrame();
                }
                else if (Input.GetKeyDown(KeyCode.F2))
                {
                    ToggleVertexNormals();
                    
                }
                if (Input.GetKeyDown(KeyCode.F3))
                {
                    ToggleFaceNormals();
                }
                else if (Input.GetKeyDown(KeyCode.F4))
                {
                    ClearLast();

                    int new_verts = m_mesh.Refine(m_refineFactor);
                    m_info = "New vertices added " + new_verts;
                    RebuildGameobject(m_mesh);
                    CreateWireFrame();
                }
                else if (Input.GetKeyDown(KeyCode.F5))
                {
                    ClearLast();

                    var processor = MeshProcessingMeshing<EIK>.Instance;

                    var minmax = m_mesh.FindMinMaxAvgEdgeLength();
                    m_targetEdgeLen = Math.Round(minmax.Average, 4);

                    //Debug.Log("MinMax edge lengths " + minmax);

                    int new_verts = processor.IsotropicRemeshing(m_mesh, m_targetEdgeLen, 1);
                    m_info = "New vertices added " + new_verts;
                    RebuildGameobject(m_mesh);
                    CreateWireFrame();

                    ClearLast();
                }
                else if (Input.GetKeyDown(KeyCode.F6))
                {
                    var processor = MeshProcessingFeatures<EIK>.Instance;

                    var edges = new List<int>();
                    processor.DetectSharpEdges(m_mesh, new Degree(m_featureAngle), edges);
                    m_info = "Feature edges " + edges.Count;
                    CreateFeatureRenderer(edges);
                }
            }
        }

        protected void OnGUI()
        {
            int textLen = 1000;
            int textHeight = 25;
            GUI.color = Color.black;

            if (m_mesh == null)
            {
                GUI.Label(new Rect(10, 10, textLen, textHeight), "Tab to toggle wireframe.");
                GUI.Label(new Rect(10, 30, textLen, textHeight), "F1 to load bunny.");
                GUI.Label(new Rect(10, 50, textLen, textHeight), "F2 to load elephant.");
                GUI.Label(new Rect(10, 70, textLen, textHeight), "F3 to load mannequin.");
                GUI.Label(new Rect(10, 90, textLen, textHeight), "F4 to load fan disk.");
                GUI.Label(new Rect(10, 110, textLen, textHeight), "Space to clear mesh.");
            }
            else
            {
                GUI.Label(new Rect(10, 10, textLen, textHeight), "Tab to toggle selection mode.");
                GUI.Label(new Rect(10, 30, textLen, textHeight), string.Format("Left click to select {0}.", m_selectionMode));
                GUI.Label(new Rect(10, 50, textLen, textHeight), string.Format("F1 to toggle wireframe."));
                GUI.Label(new Rect(10, 70, textLen, textHeight), string.Format("F2 to toggle vertex normals."));
                GUI.Label(new Rect(10, 90, textLen, textHeight), string.Format("F3 to toggle face normals."));
                GUI.Label(new Rect(10, 110, textLen, textHeight), string.Format("F4 to refine with factor {0}.", m_refineFactor));
                GUI.Label(new Rect(10, 130, textLen, textHeight), string.Format("F5 perform isotropic remeshing with target edge length {0}.", m_targetEdgeLen));
                GUI.Label(new Rect(10, 150, textLen, textHeight), string.Format("F6 to dected sharp edges with angle {0}.", m_featureAngle));
                GUI.Label(new Rect(10, 170, textLen, textHeight), m_info);
            }

        }

    }

}

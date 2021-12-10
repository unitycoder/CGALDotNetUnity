using System;
using System.Collections.Generic;
using UnityEngine;

using Common.Unity.Drawing;
using CGALDotNet;
using CGALDotNet.Polygons;
using CGALDotNet.Geometry;

namespace CGALDotNetUnity.Polygons
{

    public class PolygonVisibilityExample : InputBehaviour
    {
        private Color redColor = new Color32(200, 80, 80, 128);

        private Color pointColor = new Color32(80, 80, 200, 255);

        private Color faceColor = new Color32(80, 80, 200, 128);

        private Color lineColor = new Color32(0, 0, 0, 255);

        private Point2d? Point;

        private PolygonWithHoles2<EEK> Polygon;

        private Dictionary<string, CompositeRenderer> Renderers;

        private bool AddHoles = true;

        protected override void Start()
        {
            base.Start();
            SetInputMode(INPUT_MODE.POLYGON);
            Renderers = new Dictionary<string, CompositeRenderer>();

            Polygon = CreatePolygonWithHolesExample.CreateRoom();
            CreateRenderer("Polygon", Polygon);
            CreateRenderer("Hole0", Polygon.GetHole(0));
            CreateRenderer("Hole1", Polygon.GetHole(1));
            CreateRenderer("Hole2", Polygon.GetHole(2));
        }

        protected override void OnInputComplete(List<Point2d> points)
        {

            if (Polygon == null)
            {
                var boundary = new Polygon2<EEK>(points.ToArray());

                if (boundary.IsSimple)
                {
                    if (!boundary.IsCounterClockWise)
                        boundary.Reverse();

                    Polygon = new PolygonWithHoles2<EEK>(boundary);

                    CreateRenderer("Polygon", Polygon);
                }
                else
                {
                    Debug.Log("Polygon was not simple.");
                }
            }
            else if (AddHoles)
            {
                var hole = new Polygon2<EEK>(points.ToArray());

                if (!hole.IsClockWise)
                    hole.Reverse();

                if (PolygonWithHoles2.IsValidHole(Polygon, hole))
                {
                    Polygon.AddHole(hole);
                    int holes = Polygon.HoleCount;

                    CreateRenderer("Polygon", Polygon);
                    CreateRenderer("Hole" + holes, hole);
                }
                else
                {
                    Debug.Log("Hole was not valid.");
                }
            }

            InputPoints.Clear();
        }

        private void ComputeVisibility(Point2d point)
        {
            POLYGON_VISIBILITY method = POLYGON_VISIBILITY.TRIANGULAR_EXPANSION;

            PolygonWithHoles2<EEK> region;
            if(PolygonVisibility<EEK>.Instance.ComputeVisibility(method, point, Polygon, out region))
            {
                Renderers["Region"] = Draw().
                    Faces(region, redColor).
                    Outline(region, lineColor).
                    PopRenderer();
            }
            else
            {
                Renderers.Remove("Region");
            }
        }

        private void CreateRenderer(string name, PolygonWithHoles2<EEK> pwh)
        {
            Renderers[name] = Draw().
            Faces(pwh, faceColor).
            Outline(pwh, lineColor).
            Points(pwh, lineColor, pointColor, PointSize).
            PopRenderer();
        }

        private void CreateRenderer(string name, Polygon2<EEK> polygon)
        {
            Renderers[name] = Draw().
            Outline(polygon, lineColor).
            PopRenderer();
        }

        protected override void OnCleared()
        {
            Point = null;
            Polygon = null;
            Renderers.Clear();
            AddHoles = true;
            InputPoints.Clear();
            SetInputMode(INPUT_MODE.POLYGON);
        }

        protected override void OnLeftClickDown(Point2d point)
        {
            ComputeVisibility(point);

            Point = point;
            Renderers["Point"] = Draw().
                Points(point, lineColor, redColor).
                PopRenderer();
        }

        protected override void Update()
        {
            base.Update();

            if (Input.GetKeyDown(KeyCode.F1))
            {
                AddHoles = false;
                SetInputMode(INPUT_MODE.POINT_CLICK);
            }
            else if (Input.GetKeyDown(KeyCode.F2))
            {
                AddHoles = true;
                SetInputMode(INPUT_MODE.POLYGON);
            }
        }

        private void OnPostRender()
        {
            DrawGrid();

            foreach (var renderer in Renderers.Values)
                renderer.Draw();

            DrawInput(lineColor, pointColor, PointSize);

        }

        protected void OnGUI()
        {
            int textLen = 1000;
            int textHeight = 25;
            GUI.color = Color.black;

            if (Polygon == null)
            {
                GUI.Label(new Rect(10, 10, textLen, textHeight), "Space to clear polygon.");
                GUI.Label(new Rect(10, 30, textLen, textHeight), "Left click to place point.");
                GUI.Label(new Rect(10, 50, textLen, textHeight), "Click on first point to close polygon.");
            }
            else if (AddHoles)
            {
                GUI.Label(new Rect(10, 10, textLen, textHeight), "Add holes to polygon.");
                GUI.Label(new Rect(10, 30, textLen, textHeight), "Left click to place point.");
                GUI.Label(new Rect(10, 50, textLen, textHeight), "Click on first point to close polygon.");
                GUI.Label(new Rect(10, 70, textLen, textHeight), "F1 to stop adding holes and F2 to start adding holes again.");
                GUI.Label(new Rect(10, 90, textLen, textHeight), "Holes must be simple and not intersect the polygon boundary or other holes.");
            }
            else
            {
                GUI.Label(new Rect(10, 10, textLen, textHeight), "Space to clear polygon.");
                GUI.Label(new Rect(10, 30, textLen, textHeight), "Click to compute visibility.");
            }

        }



    }
}

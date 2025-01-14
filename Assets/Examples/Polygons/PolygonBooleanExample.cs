using System;
using System.Collections.Generic;
using UnityEngine;

using Common.Unity.Drawing;
using Common.Unity.Utility;

using CGALDotNet;
using CGALDotNet.Polygons;
using CGALDotNetGeometry.Numerics;
using CGALDotNetGeometry.Shapes;

namespace CGALDotNetUnity.Polygons
{


    public class PolygonBooleanExample : InputBehaviour
    {
        private Color redColor = new Color32(200, 80, 80, 255);

        private Color pointColor = new Color32(80, 80, 200, 255);

        private Color faceColor = new Color32(80, 80, 200, 128);

        private Color lineColor = new Color32(0, 0, 0, 255);

        private List<PolygonWithHoles2<EEK>> Polygons;

        private Dictionary<string, CompositeRenderer> Renderers;

        private POLYGON_BOOLEAN Op = POLYGON_BOOLEAN.JOIN;

        protected override void Start()
        {
            base.Start();
            SetInputMode(INPUT_MODE.POLYGON);
            Renderers = new Dictionary<string, CompositeRenderer>();
            Polygons = new List<PolygonWithHoles2<EEK>>();

            ConsoleRedirect.Redirect();
        }

        protected override void OnInputComplete(List<Point2d> points)
        {
            if (Polygons.Count == 0)
            {
                var boundary = new Polygon2<EEK>(points.ToArray());

                if (boundary.IsSimple)
                {
                    if (!boundary.IsCounterClockWise)
                        boundary.Reverse();

                    var polygon = new PolygonWithHoles2<EEK>(boundary);
                    Polygons.Add(polygon);

                    for(int i = 0; i < Polygons.Count; i++)
                        CreateRenderer(i, Polygons[i]);

                }
            }
            else
            {
                var polygon = new Polygon2<EEK>(points.ToArray());

                if(polygon.IsSimple)
                {
                    if (!polygon.IsCounterClockWise)
                        polygon.Reverse();

                    var tmp = new List<PolygonWithHoles2<EEK>>(Polygons);
                    Polygons.Clear();

                    foreach(var poly in tmp)
                    {
                        if(PolygonBoolean2<EEK>.Instance.Op(Op, polygon, poly, Polygons))
                        {
                            for (int i = 0; i < Polygons.Count; i++)
                                CreateRenderer(i, Polygons[i]);

                            break;
                        }
                    }
                }

            }

            InputPoints.Clear();
        }

        private void CreateRenderer(int id, PolygonWithHoles2<EEK> polygon)
        {
            Renderers["Polygon " + id] = Draw().
                Faces(polygon, faceColor).
                Outline(polygon, lineColor).
                Points(polygon, lineColor, pointColor, PointSize).
                PopRenderer();

            int holes = polygon.HoleCount;
            for (int i = 0; i < holes; i++)
            {
                var hole = polygon.Copy(POLYGON_ELEMENT.HOLE, i);

                Renderers["Hole " + i + " " + id] = Draw().
                Outline(hole, lineColor).
                Points(hole, lineColor, pointColor, PointSize).
                PopRenderer();
            }
        }

        protected override void OnCleared()
        {
            Polygons.Clear();
            Renderers.Clear();
            InputPoints.Clear();
            SetInputMode(INPUT_MODE.POLYGON);
        }

        protected override void Update()
        {
            base.Update();

            if (Input.GetKeyDown(KeyCode.Tab))
            {
                Op = CGALEnum.Next(Op);

                //Triangulation polygons for symetric difference
                //not working at the moment so skip.
                if (Op == POLYGON_BOOLEAN.SYMMETRIC_DIFFERENCE)
                    Op = POLYGON_BOOLEAN.JOIN;
            }

        }

        private void OnPostRender()
        {
            DrawGrid();
            DrawInput(lineColor, pointColor, PointSize);

            foreach (var renderer in Renderers.Values)
                renderer.Draw();

        }

        protected void OnGUI()
        {
            int textLen = 400;
            int textHeight = 25;
            GUI.color = Color.black;

            GUI.Label(new Rect(10, 10, textLen, textHeight), "Space to clear polygon.");
            GUI.Label(new Rect(10, 30, textLen, textHeight), "Left click to place point.");
            GUI.Label(new Rect(10, 50, textLen, textHeight), "Click on first point to close polygon.");

            GUI.Label(new Rect(10, 70, textLen, textHeight), "Tab to change boolean op.");
            GUI.Label(new Rect(10, 90, textLen, textHeight), "Current op = " + Op);
        }

    }
}

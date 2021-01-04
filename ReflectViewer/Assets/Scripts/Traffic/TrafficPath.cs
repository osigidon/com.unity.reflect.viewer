using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;


namespace CivilFX.TrafficV5
{
    public enum PathType
    {
        Urban,
        Freeway
    }

    public class TrafficPath : MonoBehaviour
    {
        public PathType pathType;
        public float widthPerLane = 1.78f;
        [Tooltip("Width of all lanes; in meters")]
        public float calculatedWidth;

        public float pathLength; //overall arc length

        [Range(1, 10)]
        public int lanesCount = 1;
        public int splineResolution = 150;
        public Color splineColor = Color.green;
        public List<Vector3> nodes;

        /*
        public CutSegment [] cutSegments;
        */
        private SplineBuilder splineBuilder;

        public int GetNodesCount()
        {
            return nodes.Count;
        }

        public SplineBuilder GetSplineBuilder(bool forceRebuild = false)
        {
            if (forceRebuild || splineBuilder == null)
            {
                splineBuilder = new SplineBuilder(this);
            }

            return splineBuilder;
        }

        //Just for visualization for now
        //function to splice the path into different box segment
#if UNITY_EDITOR
        [DrawGizmo(GizmoType.Active | GizmoType.NotInSelectionHierarchy
            | GizmoType.InSelectionHierarchy | GizmoType.Pickable, typeof(TrafficPath))]
        static void DrawGizmos(TrafficPath path, GizmoType gizmoType)
        {
            if (path.GetNodesCount() < 2)
            {
                return;
            }
            var color = Gizmos.color;
            Gizmos.color = path.splineColor;
            SplineBuilder splineBuilder = path.GetSplineBuilder();
            var segmentation = 1.0f / path.splineResolution;
            var t = 0.0f;
            var lanesCount = path.lanesCount;

            var centerStart = splineBuilder.GetPoint(0);
            var centerEnd = Vector3.zero;
            var dir = Vector3.zero;
            var left = dir;
            var right = dir;
            t = segmentation;

            //draw starting line
            centerEnd = splineBuilder.GetPoint(t);
            dir = (centerEnd - centerStart).normalized;
            left = Vector3.Cross(Vector3.up, dir) * path.calculatedWidth;
            right = -left;
            Gizmos.DrawLine(centerStart, centerStart + left);
            Gizmos.DrawLine(centerStart, centerStart + right);

            while (t <= 1.0f)
            {
                centerEnd = splineBuilder.GetPoint(t);

                dir = (centerEnd - centerStart).normalized;
                left = Vector3.Cross(Vector3.up, dir) * path.calculatedWidth;
                right = -left;

                //draw inner lines
                var laneSegment = 1.0f / lanesCount; // |_._._| : . = laneSegment
                var laneTime = laneSegment;
                while (laneTime < 1.0f)
                {
                    var laneLastPoint = Vector3.Lerp((Vector3)(centerStart + left), (Vector3)(centerStart + right), laneTime);
                    var laneCurrentPoint = Vector3.Lerp(centerEnd + left, centerEnd + right, laneTime);
                    Gizmos.DrawLine(laneLastPoint, laneCurrentPoint);
                    laneTime += laneSegment;
                }

                //draw most outter lines
                Gizmos.DrawLine((Vector3)(centerStart + left), centerEnd + left); // E |
                Gizmos.DrawLine((Vector3)(centerStart + right), centerEnd + right); // | E



                centerStart = centerEnd;
                t += segmentation;

                //draw closing line
                if (t >= 1.0f)
                {
                    Gizmos.DrawLine(centerStart, centerStart + left);  // E_
                    Gizmos.DrawLine(centerStart, centerStart + right); // _E
                }
            }

            //draw starting arrows
            centerStart = splineBuilder.GetPoint(0.01f);
            centerEnd = splineBuilder.GetPoint(0.015f);
            dir = (centerEnd - centerStart).normalized;
            left = Vector3.Cross(Vector3.up, dir) * path.calculatedWidth;
            right = -left;
            var leftStart = centerStart + left;
            var leftEnd = centerEnd + left;
            var rightStart = centerStart + right;
            var rightEnd = centerEnd + right;
            var seg = 1.0f / (lanesCount * 2);
            var time = seg;
            var skip = false;
            Gizmos.color = Color.yellow;
            while (time < 1.0f)
            {
                if (!skip)
                {
                    centerStart = Vector3.Lerp(leftStart, rightStart, time);
                    centerEnd = Vector3.Lerp(leftEnd, rightEnd, time);
                    dir = (centerEnd - centerStart).normalized;
                    left = Vector3.Cross(Vector3.up, dir) * (path.calculatedWidth / (lanesCount * 2));
                    right = -left;
                    Gizmos.DrawLine(centerStart + left, centerEnd);
                    Gizmos.DrawLine(centerStart + right, centerEnd);
                    skip = true;
                }
                else
                {
                    skip = false;
                }
                time += seg;
            }
            Gizmos.color = color;
            /*
            //draw rough lanes
            Gizmos.color = Color.red;

            for (int i = 0; i < path.nodes.Count - 1; i++)
            {
                var centerStart = path.nodes[i];
                var centerEnd = path.nodes[i + 1];

                var dir = math.normalize(centerEnd - centerStart);
                var left = Vector3.Cross(Vector3.up, dir) * path.width;
                var right = -left;
                

                Gizmos.DrawLine(centerStart, centerStart + left);  // S_
                Gizmos.DrawLine(centerStart, centerStart + right); // _S
                Gizmos.DrawLine(centerEnd, centerEnd + left);  // E_
                Gizmos.DrawLine(centerEnd, centerEnd + right); // _E
                Gizmos.DrawLine(centerStart + left, centerEnd + left); // E |
                Gizmos.DrawLine(centerStart + right, centerEnd + right); // | E
            }
            */
            /*
            //draw center rough lines
            for (int i=0; i<path.nodes.Count-1; i++)
            {
                Gizmos.DrawLine(path.nodes[i], path.nodes[i + 1]);
            }
            */

        }
#endif 
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CivilFX.TrafficV5
{
    public class SplineBuilder
    {
        public struct Segment
        {
            public float time;
            public float distance;
            public Segment(float time, float distance)
            {
                this.time = time;
                this.distance = distance;
            }
        }

        public List<Vector3> _nodes;
        public List<Segment> segments;

        public int segmentResolution = 5;

        public float pathLength;

        //constructor
        public SplineBuilder(TrafficPath path)
        {
            _nodes = path.nodes;
            BuildPath();
        }

        public SplineBuilder(List<Vector3> nodes)
        {
            _nodes = nodes;
            BuildPath();
        }
        public void closePath()
        {
            // first, remove the control points
            _nodes.RemoveAt(0);
            _nodes.RemoveAt(_nodes.Count - 1);

            // if the first and last node are not the same add one
            if (_nodes[0] != _nodes[_nodes.Count - 1])
                _nodes.Add(_nodes[0]);


            // figure out the distances from node 0 to the first node and the second to last node (remember above
            // we made the last node equal to the first so node 0 and _nodes.Count - 1 are identical)
            var distanceToFirstNode = Vector3.Distance(_nodes[0], _nodes[1]);
            var distanceToLastNode = Vector3.Distance(_nodes[0], _nodes[_nodes.Count - 2]);


            // handle the first node. we want to use the distance to the LAST (opposite segment) node to figure out where this control point should be
            var distanceToFirstTarget = distanceToLastNode / Vector3.Distance(_nodes[1], _nodes[0]);
            var lastControlNode = (_nodes[0] + (_nodes[1] - _nodes[0]) * distanceToFirstTarget);


            // handle the last node. for this one, we want the distance to the first node for the control point but it should
            // be along the vector to the last node
            var distanceToLastTarget = distanceToFirstNode / Vector3.Distance(_nodes[_nodes.Count - 2], _nodes[0]);
            var firstControlNode = (_nodes[0] + (_nodes[_nodes.Count - 2] - _nodes[0]) * distanceToLastTarget);

            _nodes.Insert(0, firstControlNode);
            _nodes.Add(lastControlNode);
        }

        private void BuildPath()
        {
            var totalSubdivisions = _nodes.Count * segmentResolution;
            segments = new List<Segment>(totalSubdivisions);
            pathLength = 0;
            var segmentLength = 1f / totalSubdivisions;
            var lastPoint = GetPoint(0);

            for (int i=1; i<totalSubdivisions+1; i++)
            {
                var currentSegment = segmentLength * i;
                var currentPoint = GetPoint(currentSegment);
                pathLength += Vector3.Distance(currentPoint, lastPoint);
                lastPoint = currentPoint;
                segments.Add(new Segment(currentSegment, pathLength));
            }
        }

        public Vector3 GetPoint(float t)
        {
            int numSections = _nodes.Count - 3;
            int currentNode = Mathf.Min(Mathf.FloorToInt(t * (float)numSections), numSections - 1);
            float u = t * (float)numSections - (float)currentNode;

            Vector3 a = _nodes[currentNode];
            Vector3 b = _nodes[currentNode + 1];
            Vector3 c = _nodes[currentNode + 2];
            Vector3 d = _nodes[currentNode + 3];

            return .5f *
            (
                (-a + 3f * b - 3f * c + d) * (u * u * u)
                + (2f * a - 5f * b + 4f * c - d) * (u * u)
                + (-a + c) * u
                + 2f * b
            );
        }

        private Vector3 GetTangent(float t)
        {
            int numSections = _nodes.Count - 3;
            int currentNode = Mathf.Min(Mathf.FloorToInt(t * (float)numSections), numSections - 1);
            float u = t * (float)numSections - (float)currentNode;

            Vector3 a = _nodes[currentNode];
            Vector3 b = _nodes[currentNode + 1];
            Vector3 c = _nodes[currentNode + 2];
            Vector3 d = _nodes[currentNode + 3];

            return 0.5f * ((-a + c) + 2.0f * (2.0f * a - 5.0f * b + 4.0f * c - d) * u +
                               3 * (-a + 3 * b - 3 * c + d) * u * u);

        }
        public Vector3 GetTangentOnPath(float t)
        {
            t = GetSmoothTimeOnCurve(t);
            return GetTangent(t).normalized;
        }
        public Vector3 GetTangentOnPathSegment(float u)
        {
            return GetTangentOnPath(Mathf.Clamp01(u / pathLength));
        }
        // gets the point taking in to account constant speed. the default implementation approximates the length of the spline
        // by walking it and calculating the distance between each node
        public Vector3 GetPointOnPath(float t)
        {
            t = GetSmoothTimeOnCurve(t);
            return GetPoint(t);
        }

        public Vector3 GetPointOnPathSegment(float segment)
        {
            return GetPointOnPath(Mathf.Clamp01(segment / pathLength));
        }

        private float GetSmoothTimeOnCurve(float t)
        {
            // we know exactly how far along the path we want to be from the passed in t
            float targetDistance = pathLength * t;

            // loop through all the values in our lookup table and find the two nodes our targetDistance falls between
            // translate the values from the lookup table estimating the arc length between our known nodes from the lookup table
            int nextSegmentIndex;
            for (nextSegmentIndex = 0; nextSegmentIndex < segments.Count; nextSegmentIndex++) {
                if (segments[nextSegmentIndex].distance >= targetDistance)
                    break;
            }
#if (UNITY_EDITOR)
            nextSegmentIndex = Mathf.Clamp(nextSegmentIndex, 0, segments.Count - 1);
#endif
                Segment nextSegment = segments[nextSegmentIndex];

            if (nextSegmentIndex == 0) {
                // t within first segment
                t = (targetDistance / nextSegment.distance) * nextSegment.time;
            } else {
                // t within prev..next segment
                Segment previousSegment = segments[nextSegmentIndex - 1];

                float segmentTime = nextSegment.time - previousSegment.time;
                float segmentLength = nextSegment.distance - previousSegment.distance;

                t = previousSegment.time + ((targetDistance - previousSegment.distance) / segmentLength) * segmentTime;
            }
            return t;
        }

        public Quaternion GetOrientation(float time)
        {
            float t = GetSmoothTimeOnCurve(time);
            Vector3 tangent = GetTangent(t);
            return Quaternion.LookRotation(tangent.normalized, new Vector3(0.0f, 1.0f, 0.0f));
        }
    }

}
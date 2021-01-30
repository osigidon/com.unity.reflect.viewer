using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if (UNITY_EDITOR)
using UnityEditor;
#endif

namespace CivilFX.TrafficV5
{
    #if (UNITY_EDITOR)
    [CanEditMultipleObjects]
    [CustomEditor(typeof(TrafficPath))]
    public class TrafficPathEditor : Editor
    {
        private TrafficPath _target;
        private SerializedObject so;
        private GUIStyle labelStyle;
        private bool recalculateWidth;

        private readonly float newNodeDistance = 30.0f;

        private void OnEnable()
        {
            _target = (TrafficPath)target;
            so = serializedObject;

            so.Update();
            var nodesProp = so.FindProperty("nodes");
            int index = 1;
            var pathType = (PathType)so.FindProperty("pathType").enumValueIndex;

            float length = pathType == PathType.Freeway ? newNodeDistance : newNodeDistance - 5f;
            while (nodesProp.arraySize < 4)
            {
                var newPos = Vector3.one * (index++ * length);
                newPos.y = 0;
                nodesProp.InsertArrayElementAtIndex(nodesProp.arraySize == 0 ? 0 : nodesProp.arraySize - 1);
                nodesProp.GetArrayElementAtIndex(nodesProp.arraySize - 1).vector3Value = _target.transform.position + newPos;
            }

            //init some values
            var sp = so.FindProperty("calculatedWidth");
            recalculateWidth = sp.floatValue == 0;

            so.ApplyModifiedProperties();


            //add TraficPathController by default
            var controller = _target.gameObject.GetComponent<TrafficPathController>();
            if (controller == null)
            {
                controller = _target.gameObject.AddComponent<TrafficPathController>();
                var controllerSO = new SerializedObject(controller);
                controllerSO.Update();
                controllerSO.FindProperty("path").objectReferenceValue = _target;
                controllerSO.ApplyModifiedProperties();
            }


            labelStyle = new GUIStyle();
            labelStyle.fontStyle = FontStyle.Bold;
            labelStyle.normal.textColor = Color.white;
            labelStyle.fontSize = 16;

            Tools.hidden = true;
        }

        public override void OnInspectorGUI()
        {
            so.Update();

            //show script name
            SerializedProperty currentProp = so.FindProperty("m_Script");
            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.PropertyField(currentProp);
            }

            //pathtype
            currentProp = so.FindProperty("pathType");
            EditorGUILayout.PropertyField(currentProp);

            //path width
            currentProp = so.FindProperty("widthPerLane");
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(currentProp);
            recalculateWidth |= EditorGUI.EndChangeCheck();

            //calculated width
            using (new EditorGUI.DisabledGroupScope(true))
            {
                currentProp = so.FindProperty("calculatedWidth");
                if (recalculateWidth)
                {
                    currentProp.floatValue = so.FindProperty("widthPerLane").floatValue * so.FindProperty("lanesCount").intValue;
                    recalculateWidth = false;
                }
                EditorGUILayout.PropertyField(currentProp);

                //path length
                currentProp = so.FindProperty("pathLength");
                currentProp.floatValue = _target.GetSplineBuilder(true).pathLength;
                EditorGUILayout.PropertyField(currentProp);
            }

            //lanes count
            currentProp = so.FindProperty("lanesCount");
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(currentProp);
            recalculateWidth |= EditorGUI.EndChangeCheck();

            //spline resolution
            currentProp = so.FindProperty("splineResolution");
            EditorGUILayout.PropertyField(currentProp);

            //spline color
            currentProp = so.FindProperty("splineColor");
            EditorGUILayout.PropertyField(currentProp);
            //nodes
            currentProp = so.FindProperty("nodes");
            GUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(currentProp, new GUIContent(""), false, GUILayout.MaxWidth(1));
            EditorGUILayout.LabelField("Nodes:", EditorStyles.boldLabel, GUILayout.MaxWidth(100));
            if (GUILayout.Button("Reverse", GUILayout.MaxWidth(100)))
            {
                SerializedProperty nodesProp = so.FindProperty("nodes");
                List<Vector3> nodes = new List<Vector3>(nodesProp.arraySize);
                var iterator = nodesProp.GetEnumerator();
                while (iterator.MoveNext())
                {
                    nodes.Add(((SerializedProperty)iterator.Current).vector3Value);
                }
                iterator = nodesProp.GetEnumerator();
                for (int i = nodes.Count - 1; i >= 0; i--)
                {
                    iterator.MoveNext();
                    ((SerializedProperty)iterator.Current).vector3Value = nodes[i];
                }
            }
            if (GUILayout.Button("Project", GUILayout.MaxWidth(100)))
            {
                ProjectNodes(so.FindProperty("nodes"));
            }
            if (GUILayout.Button("Close", GUILayout.MaxWidth(100)))
            {
                var sp = so.FindProperty("nodes");

                if (sp.GetArrayElementAtIndex(1).vector3Value != sp.GetArrayElementAtIndex(sp.arraySize - 2).vector3Value)
                {


                    sp.DeleteArrayElementAtIndex(0);
                    sp.DeleteArrayElementAtIndex(sp.arraySize - 1);

                    if (sp.GetArrayElementAtIndex(0) != sp.GetArrayElementAtIndex(sp.arraySize - 1))
                    {
                        sp.arraySize++;
                        sp.GetArrayElementAtIndex(sp.arraySize - 1).vector3Value = sp.GetArrayElementAtIndex(0).vector3Value;
                    }

                    var node0 = sp.GetArrayElementAtIndex(0).vector3Value;
                    var node1 = sp.GetArrayElementAtIndex(1).vector3Value;
                    var nodeMinus2 = sp.GetArrayElementAtIndex(sp.arraySize - 2).vector3Value;

                    var distanceToFirstNode = Vector3.Distance(node0, node1);
                    var distanceToLastNode = Vector3.Distance(node0, nodeMinus2);

                    var distanceToFirstTarget = distanceToLastNode / Vector3.Distance(node1, node0);
                    var lastControlNode = (node0 + (node1 - node0) * distanceToFirstTarget);

                    var distanceToLastTarget = distanceToFirstNode / Vector3.Distance(nodeMinus2, node0);
                    var firstControlNode = (node0 + (nodeMinus2 - node0) * distanceToLastTarget);

                    sp.InsertArrayElementAtIndex(0);
                    sp.GetArrayElementAtIndex(0).vector3Value = firstControlNode;
                    sp.arraySize++;
                    sp.GetArrayElementAtIndex(sp.arraySize - 1).vector3Value = lastControlNode;
                }
            }
            GUILayout.EndHorizontal();
            if (currentProp.isExpanded)
            {
                EditorGUI.indentLevel++;
                for (int i = 0; i < currentProp.arraySize; i++)
                {
                    GUILayout.BeginHorizontal();
                    if (i == 0)
                    {
                        EditorGUILayout.LabelField("Begin", GUILayout.MaxWidth(50));
                    }
                    else if (i == currentProp.arraySize - 1)
                    {
                        EditorGUILayout.LabelField("End", GUILayout.MaxWidth(50));
                    }
                    else
                    {
                        EditorGUILayout.LabelField(i.ToString(), GUILayout.MaxWidth(50));
                    }
                    SerializedProperty nodeProp = currentProp.GetArrayElementAtIndex(i);
                    EditorGUILayout.PropertyField(nodeProp, new GUIContent(""));

                    //delete button
                    if (GUILayout.Button(new GUIContent("X", "Delete this node"), GUILayout.MaxWidth(50)))
                    {
                        if (currentProp.arraySize > 4)
                        {
                            currentProp.MoveArrayElement(i, currentProp.arraySize - 1);
                            currentProp.arraySize -= 1;
                        }
                    }
                    GUILayout.EndHorizontal();
                }
                EditorGUI.indentLevel--;
            }

            if (GUILayout.Button("Adjust Parent Transform"))
            {
                _target.transform.position = so.FindProperty("nodes").GetArrayElementAtIndex(1).vector3Value;
            }

            so.ApplyModifiedProperties();
        }

        public void OnSceneGUI()
        {
            //short cut to project nodes
            Event e = Event.current;
            if (e.type == EventType.KeyDown && e.keyCode == KeyCode.Space)
            {
                ProjectNodes(_target.nodes);
            }

            //short cut to delete node
            if (e.type == EventType.KeyUp && e.keyCode == KeyCode.X && e.control && _target.nodes.Count > 4)
            {
                int index = LocateNearestNode(_target.nodes, e.mousePosition);
                Undo.RecordObject(target, "DeleteNode");
                _target.nodes.RemoveAt(index);
            }

            //move scene camera to begin of node
            if (e.control && e.type == EventType.KeyDown && e.keyCode == KeyCode.F)
            {
                MoveSceneView(_target.nodes[0]);
            }

            //move scene camera to selected node
            if (e.type == EventType.KeyDown && e.keyCode == KeyCode.B)
            {
                var index = LocateNearestNode(_target.nodes, e.mousePosition);
                if (index > 0)
                {

                    Vector3 pos = Vector3.Lerp(_target.nodes[index - 1], _target.nodes[index], 0.5f);
                    MoveSceneView(pos);
                }
                /*
                Vector3 pos = _target.nodes[(LocateNearestNode(_target.nodes, e.mousePosition))];
                MoveSceneView(pos);
                */
            }
            //double click to add
            if (e.type == EventType.MouseDown && e.clickCount > 1)
            {
                int index = LocateNearestNode(_target.nodes, e.mousePosition);
                var direction = Vector3.zero;
                var newNode = Vector3.zero;
                var length = _target.pathType == PathType.Freeway ? newNodeDistance : newNodeDistance - 5f;
                var nodesCount = _target.GetNodesCount();

                if (index == nodesCount - 1)
                {
                    direction = (_target.nodes[nodesCount - 1] - _target.nodes[nodesCount - 2]).normalized;
                    newNode = _target.nodes[nodesCount - 1] + (direction * length);
                }
                else if (index == 0)
                {
                    direction = (_target.nodes[1] - _target.nodes[0]).normalized;
                    newNode = _target.nodes[0] + (-direction * length);
                }
                else
                {
                    direction = (_target.nodes[index + 1] - _target.nodes[index]).normalized;
                    length = Vector3.Distance(_target.nodes[index], _target.nodes[index + 1]) / 2;
                    newNode = _target.nodes[index] + (direction * length);
                }
                newNode = GetProjectedNode(newNode);
                Undo.RecordObject(target, "AddNodeSingle");
                _target.nodes.Insert(index == 0 ? index : index + 1, newNode);
            }


            //draw nodes handle
            for (int i = 0; i < _target.nodes.Count; i++)
            {

                Vector3 currentPos = _target.nodes[i];

                //draw label
                if (i == 0)
                {
                    Handles.Label(currentPos, "Begin", labelStyle);
                }
                else if (i == _target.nodes.Count - 1)
                {
                    Handles.Label(currentPos, "End", labelStyle);
                }
                else
                {
                    Handles.Label(currentPos, i.ToString(), labelStyle);
                }

                //draw handle
                EditorGUI.BeginChangeCheck();
                Vector3 newPos = Handles.PositionHandle(currentPos, Quaternion.identity);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(target, "MoveSingleNode");
                    if (e.control)
                    {
                        MoveAllNodes(_target.nodes, newPos - currentPos);
                    }
                    else
                    {
                        _target.nodes[i] = GetProjectedNode(newPos);
                        /*
                        //auto-adjust start controlled point (Begin)
                        if (i == 1) {
                            _target.nodes[0] = newPos + (newPos - _target.nodes[2]).normalized * 20f;
                        } else if (i == _target.nodes.Count - 2) {
                            //auto adjust end controlled point (End)
                            _target.nodes[_target.nodes.Count - 1] = newPos + (newPos - _target.nodes[_target.nodes.Count - 3]).normalized * 20f;
                        }
                        */
                    }
                }
            }

        }

        private int ProjectNodes(SerializedProperty prop)
        {
            var iterator = prop.GetEnumerator();
            int count = 0;
            while (iterator.MoveNext())
            {
                SerializedProperty nodeProp = (SerializedProperty)iterator.Current;

                Vector3 currentPos = nodeProp.vector3Value;
                RaycastHit hit;

                if (Physics.Raycast(currentPos + Vector3.up * 5.0f, Vector3.down, out hit, 10000f))
                {
                    //cast down
                    nodeProp.vector3Value = hit.point;
                }
                else if (Physics.Raycast(currentPos + Vector3.down * 5.0f, Vector3.up, out hit, 10000f))
                {
                    nodeProp.vector3Value = hit.point;
                }
                else
                {
                    count++;
                    Debug.Log("Not Hit");
                }
            }
            return count;
        }

        private int ProjectNodes(List<Vector3> nodes)
        {
            Undo.RecordObject(target, "ProjectNodes");
            Vector3 currentPos;
            RaycastHit hit;
            int count = 0;
            for (int i = 0; i < nodes.Count; i++)
            {
                currentPos = nodes[i];
                if (Physics.Raycast(currentPos + Vector3.up, Vector3.down, out hit, 10000f))
                {
                    //cast down
                    nodes[i] = hit.point;
                }
                else if (Physics.Raycast(currentPos + Vector3.down, Vector3.up, out hit, 10000f))
                {
                    nodes[i] = hit.point;
                }
                else
                {
                    count++;
                    Debug.Log("Not Hit");
                }
            }
            return count;
        }

        private void MoveAllNodes(List<Vector3> nodes, Vector3 delta)
        {
            Undo.RecordObject(target, "MoveAllNodes");
            for (int i = 0; i < nodes.Count; i++)
            {
                nodes[i] += delta;
            }
        }

        private Vector3 GetProjectedNode(Vector3 node, float castDistance = 10000.0f)
        {
            Vector3 result = node;
            if (Physics.Raycast(node + Vector3.up * 100.0f, Vector3.down, out RaycastHit hit, castDistance))
            {
                result = hit.point;
            }
            return result;
        }


        private int LocateNearestNode(List<Vector3> nodes, Vector2 mousePos)
        {
            int index = -1;
            float minDistance = float.MaxValue;

            for (int i = 0; i < nodes.Count; i++)
            {
                var nodeToGUI = HandleUtility.WorldToGUIPoint(nodes[i]);
                var dis = Vector2.Distance(nodeToGUI, mousePos);
                if (dis < minDistance)
                {
                    minDistance = dis;
                    index = i;
                }
            }
            return index;
        }
        private void MoveSceneView(Vector3 pos)
        {
            var view = SceneView.currentDrawingSceneView;
            if (view != null)
            {
                var target = new GameObject();
                var y = Camera.current.transform.position.y;
                var rot = Camera.current.transform.rotation;
                target.transform.rotation = rot;
                target.transform.position = pos + new Vector3(0, 50, 0);
                //target.transform.LookAt(pos);
                view.AlignViewToObject(target.transform);
                GameObject.DestroyImmediate(target);
            }
        }

        private void OnDisable()
        {
            Tools.hidden = false;
        }

    }
    #endif
}

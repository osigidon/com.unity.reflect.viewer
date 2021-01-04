using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Text;
using UnityEditor.Experimental;

namespace CivilFX.TrafficV5
{
    [CustomEditor(typeof(IntersectionController))]
    public class IntersectionControllerEditor : Editor
    {
        private SerializedObject so;
        private SerializedProperty currentProp;
        private IntersectionController _target;

        private List<SerializedProperty> greenIntervalProps; //used for fixedinterval
        private List<string> propsName;

        private int minInterval = 3;
        private int maxInterval = 300;

        private void OnEnable()
        {
            so = serializedObject;
            _target = (IntersectionController)target;
            greenIntervalProps = new List<SerializedProperty>();
            propsName = new List<string>();

            CursorStateUtility.ClearState();
        }

        public override void OnInspectorGUI()
        {
            greenIntervalProps.Clear();
            propsName.Clear();

            so.Update();
            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.PropertyField(so.FindProperty("m_Script"));
            }

            //stopBarPrefab
            currentProp = so.FindProperty("stopBarPrefab");
            EditorGUILayout.PropertyField(currentProp);

            /*
            //models
            currentProp = so.FindProperty("virtualLongModel");
            EditorGUILayout.PropertyField(currentProp);
            currentProp = so.FindProperty("virtualLCModel");
            EditorGUILayout.PropertyField(currentProp);
            */

            //signal type
            currentProp = so.FindProperty("signalType");
            EditorGUILayout.PropertyField(currentProp);

            //draw fixedinterval
            var signalType = (SignalType)currentProp.enumValueIndex;
            if (signalType == SignalType.FixedInterval)
            {
                currentProp = so.FindProperty("interval");
                EditorGUILayout.IntSlider(currentProp, minInterval, maxInterval);
            }

            //signal
            EditorGUILayout.LabelField("Signal Infomation:", EditorStyles.boldLabel);
            currentProp = so.FindProperty("signalPaths");
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(currentProp, FormatName(currentProp.name, currentProp.arraySize), false, GUILayout.MaxWidth(200));
            if (GUILayout.Button("+", GUILayout.MaxWidth(50)))
            {
                currentProp.arraySize++;
            }
            EditorGUILayout.EndHorizontal();
            var childProp = currentProp.GetEnumerator();
            var index = -1;
            var isDeleted = false;
            var isSwapped = 0;
            var tempIndex = 0;
            while (currentProp.isExpanded && childProp.MoveNext())
            {
                EditorGUI.indentLevel++;
                var currentChild = (SerializedProperty)childProp.Current;
                var labelName = "<Null>";
                var currentGrandChild = currentChild.GetEnumerator();
                var lanesCount = 0;
                var pathLength = 0f;

                while (currentGrandChild.MoveNext())
                {
                    var current = (SerializedProperty)currentGrandChild.Current;
                    var currentName = current.propertyPath;
                    //Debug.Log(currentName);
                    EditorGUI.indentLevel++;
                    if (currentName.EndsWith("pathController"))
                    {
                        EditorGUI.indentLevel--;
                        if (current.objectReferenceValue != null)
                        {
                            labelName = current.objectReferenceValue.name;
                            propsName.Add(labelName);
                            lanesCount = (current.objectReferenceValue as TrafficPathController).path.lanesCount;
                            pathLength = (current.objectReferenceValue as TrafficPathController).path.pathLength;
                        }
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.PropertyField(current, new GUIContent(labelName), GUILayout.MaxWidth(900));
                        var img = PathPickerEditor.LoadWidgetIcon();

                        Color GUIColor = GUI.backgroundColor;
                        if (CursorStateUtility.GetCursorState() && CursorStateUtility.GetTriggerIndex(0) == tempIndex)
                        {
                            GUI.backgroundColor = Color.green;
                        }
                        if (GUILayout.Button(img, GUILayout.MaxWidth(30), GUILayout.MaxHeight(18)))
                        {
                            if (CursorStateUtility.GetTriggerIndex(0) == tempIndex)
                            {
                                CursorStateUtility.SetState(false, -1);
                            }
                            else
                            {
                                CursorStateUtility.SetState(true, tempIndex);
                            }
                        }
                        GUI.backgroundColor = GUIColor;

                        if (GUILayout.Button("^", GUILayout.MaxWidth(50)))
                        {
                            isSwapped = 1;
                            index = tempIndex;
                        }
                        if (GUILayout.Button("v", GUILayout.MaxWidth(50)))
                        {
                            isSwapped = 2;
                            index = tempIndex;
                        }
                        if (GUILayout.Button("-", GUILayout.MaxWidth(50)))
                        {
                            isDeleted = true;
                            index = tempIndex;
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                    else if (currentName.EndsWith("movementType") && signalType == SignalType.RingBarriers)
                    {
                        EditorGUILayout.PropertyField(current);
                        EditorGUI.indentLevel--;
                    }
                    else if (currentName.EndsWith("stoppedPoints"))
                    {
                        if (current.arraySize != lanesCount)
                        {
                            current.arraySize = lanesCount;
                        }
                        lanesCount = 0;
                        EditorGUILayout.PropertyField(current, false);
                        EditorGUI.indentLevel++;
                        while (current.isExpanded && lanesCount != current.arraySize)
                        {
                            var p = current.GetArrayElementAtIndex(lanesCount);
                            EditorGUILayout.Slider(p, 0, pathLength - 0.001f, new GUIContent("Lane " + lanesCount.ToString()));
                            lanesCount++;
                        }
                        EditorGUI.indentLevel--;
                        EditorGUI.indentLevel--;
                    }
                    else if (currentName.EndsWith("greenInterval"))
                    {
                        greenIntervalProps.Add(current.Copy());
                        EditorGUI.indentLevel--;
                    }
                    else if (currentName.EndsWith("controlLight") || currentName.EndsWith("greenLights")
                              || currentName.EndsWith("yellowLights") || currentName.EndsWith("redLights"))
                    {
                        EditorGUILayout.PropertyField(current);
                        EditorGUI.indentLevel--;
                    }
                    else
                    {
                        EditorGUI.indentLevel--;
                    }
                }
                tempIndex++;
                EditorGUI.indentLevel--;
                EditorGUILayout.Space();
                EditorGUILayout.Space();
            }
            if (isDeleted)
            {
                currentProp.DeleteArrayElementAtIndex(index);
                so.ApplyModifiedProperties();
                return;
            }

            if (isSwapped != 0)
            {
                currentProp = so.FindProperty("signalPaths");
                if (isSwapped == 1 && index > 0)
                {
                    //up
                    currentProp.MoveArrayElement(index, index - 1);
                }
                else if (isSwapped == 2 && index < currentProp.arraySize - 1)
                {
                    //down
                    currentProp.MoveArrayElement(index, index + 1);
                }
                so.ApplyModifiedProperties();
                return;
            }

            //draw greenInterval
            if ((SignalType)so.FindProperty("signalType").enumValueIndex == SignalType.FixedInterval && greenIntervalProps.Count == propsName.Count)
            {

                using (new EditorGUI.DisabledGroupScope(true))
                {
                    //draw current time for debugging
                    var currentTimeProp = so.FindProperty("currentTime");
                    var minTime = 0f;
                    var maxTime = (float)so.FindProperty("interval").intValue;
                    GUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Time", GUILayout.MaxWidth(100));
                    EditorGUILayout.IntField(Mathf.CeilToInt(minTime), GUILayout.MaxWidth(50));
                    EditorGUILayout.Slider(currentTimeProp.floatValue, minTime, maxTime);
                    GUILayout.EndHorizontal();
                }

                //draw intervals
                for (int i = 0; i < greenIntervalProps.Count; i++)
                {
                    var prop = greenIntervalProps[i];
                    var name = propsName[i];
                    if (prop.arraySize != 2)
                    {
                        prop.arraySize = 2;
                    }
                    var min = (float)prop.GetArrayElementAtIndex(0).intValue;
                    var max = (float)prop.GetArrayElementAtIndex(1).intValue;
                    EditorGUI.BeginChangeCheck();
                    GUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(name, GUILayout.MaxWidth(100));
                    min = EditorGUILayout.IntField(Mathf.CeilToInt(min), GUILayout.MaxWidth(50));
                    EditorGUILayout.MinMaxSlider(ref min, ref max, 0, so.FindProperty("interval").intValue);
                    max = EditorGUILayout.IntField(Mathf.CeilToInt(max), GUILayout.MaxWidth(50));
                    GUILayout.EndHorizontal();
                    if (EditorGUI.EndChangeCheck())
                    {
                        prop.GetArrayElementAtIndex(0).intValue = Mathf.CeilToInt(min);
                        prop.GetArrayElementAtIndex(1).intValue = Mathf.CeilToInt(max);
                    }

                }
            }
            else
            {
                //ringbarrier
            }

            so.ApplyModifiedProperties();
        }

        private void OnSceneGUI()
        {
            if (_target.signalPaths != null)
            {
                foreach (var signalPath in _target.signalPaths)
                {
                    var pathController = signalPath.pathController;
                    for (int i = 0; i < signalPath.stoppedPoints.Length; i++)
                    {
                        var node = GetPositionFromLaneIndex(pathController.path.GetSplineBuilder(), signalPath.stoppedPoints[i], i, pathController.path.lanesCount, pathController.path.calculatedWidth);
                        var node1 = GetPositionFromLaneIndex(pathController.path.GetSplineBuilder(), signalPath.stoppedPoints[i] + 1f, i, pathController.path.lanesCount, pathController.path.calculatedWidth);
                        Handles.Label(node, "STOP");
                        Handles.PositionHandle(node, Quaternion.identity);

                        //draw direction
                        var dir = Vector3.Normalize(node1 - node);
                        node1 = node + dir * 2f;
                        var left = Vector3.Cross(dir, Vector3.up);
                        Handles.DrawLine(node, node1);
                        Handles.DrawLine(node + dir + left, node1);
                        Handles.DrawLine(node + dir + -left, node1);
                    }
                }
            }
            //path picker
            if (CursorStateUtility.GetCursorState())
            {
                TrafficPathController selected = PathPickerEditor.PickPath(_target.gameObject.scene.name, _target/*, ref _target.ramps[cursorIndex].newPath*/);
                if (selected != null)
                {
                    Undo.RecordObject(_target, "Seting new pathcontroller");
                    _target.signalPaths[CursorStateUtility.GetTriggerIndex(0)].pathController = selected;
                    CursorStateUtility.SetState(false, -1);
                    HandleUtility.AddDefaultControl(0);
                }
            }
        }


        private Vector3 GetPositionFromLaneIndex(SplineBuilder spline, float u, int lane, int lanesCount, float width)
        {
            var centerStart = spline.GetPointOnPathSegment(u);
            var centerEnd = spline.GetPointOnPathSegment(u + 0.001f);
            var dir = Vector3.Normalize(centerEnd - centerStart);
            var right = Vector3.Cross(Vector3.up, dir) * width;
            var left = -right;
            var seg = (2f * lane + 1f) / (2f * lanesCount); //lerp value based on lane number               
            var pos = Vector3.Lerp(centerStart + left, centerStart + right, seg); //actualy position based on lane number
            return pos;
        }

        private GUIContent FormatName(string name, int size)
        {
            StringBuilder sb = new StringBuilder();

            foreach (var c in name)
            {
                if (char.IsUpper(c))
                {
                    sb.Append(' ');
                }
                sb.Append(c);
            }
            sb[0] = char.ToUpper(sb[0]);
            sb.Append(' ');
            sb.Append('(');
            sb.Append(size.ToString());
            sb.Append(')');

            return new GUIContent(sb.ToString());

        }

    }
}
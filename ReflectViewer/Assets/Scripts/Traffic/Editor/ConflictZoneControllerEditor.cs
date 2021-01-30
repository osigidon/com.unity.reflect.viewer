using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if (UNITY_EDITOR)
using UnityEditor;
#endif

namespace CivilFX.TrafficV5
{
    #if (UNITY_EDITOR)
    [CustomEditor(typeof(ConflictZoneController))]
    public class ConflictZoneControllerEditor : Editor
    {
        private SerializedObject so;
        private ConflictZoneController _target;


        private enum PickerMode
        {
            HighPriority,
            LowPriority
        }

        PickerMode pickerMode;

        private void OnEnable()
        {
            so = serializedObject;
            _target = (ConflictZoneController)target;

            CursorStateUtility.ClearState();
        }

        public override void OnInspectorGUI()
        {
            so.Update();
            //show script name
            SerializedProperty currentProp = so.FindProperty("m_Script");
            using (new EditorGUI.DisabledScope(true)) {
                EditorGUILayout.PropertyField(currentProp);
            }


            //conflict zone
            currentProp = so.FindProperty("conflictZones");
            EditorGUILayout.LabelField("Conflict Zones Infomation:", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(currentProp, TrafficPathControllerEditor.FormatName(currentProp.name, currentProp.arraySize), false, GUILayout.MaxWidth(150));
            //so.FindProperty("showRampHandles").boolValue = EditorGUILayout.Toggle(so.FindProperty("showRampHandles").boolValue, GUILayout.MaxWidth(20));
            if (GUILayout.Button("+", GUILayout.MaxWidth(50))) {
                currentProp.arraySize++;
            }
            EditorGUILayout.EndHorizontal();
            //draw child
            var childProp = currentProp.GetEnumerator();
            EditorGUI.indentLevel++;
            var partitionIndex = 0;
            while (currentProp.isExpanded && childProp.MoveNext()) {
                var ccp = childProp.Current as SerializedProperty;
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(ccp, false, GUILayout.MaxWidth(900));
                if (GUILayout.Button("-", GUILayout.MaxWidth(50))) {
                    currentProp.DeleteArrayElementAtIndex(partitionIndex);
                    continue;
                }
                EditorGUILayout.EndHorizontal();
                //grandchild
                var cccp = ccp.Copy().GetEnumerator();
                EditorGUI.indentLevel++;

                var lowPriorityPathLength = 0f;
                int elementIndex = -1;
                while (ccp.isExpanded && cccp.MoveNext()) {
                    ++elementIndex;
                    var cgc = cccp.Current as SerializedProperty;
                    if (cgc.name.Equals("highPriorities")) {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.PropertyField(cgc, TrafficPathControllerEditor.FormatName(cgc.name, cgc.arraySize), false, GUILayout.MaxWidth(200));
                        //so.FindProperty("showRampHandles").boolValue = EditorGUILayout.Toggle(so.FindProperty("showRampHandles").boolValue, GUILayout.MaxWidth(20));
                        if (GUILayout.Button("+", GUILayout.MaxWidth(50))) {
                            cgc.arraySize++;
                        }
                        EditorGUILayout.EndHorizontal();
                        if (!cgc.isExpanded) {
                            continue;
                        }
                        for (int i = 0; i < cgc.arraySize; i++) {
                            var conflictPathProp = cgc.GetArrayElementAtIndex(i);
                            var childEnumerator = conflictPathProp.GetEnumerator();
                            var pathLength = 0f;
                            var lanesCount = 0;
                            while (childEnumerator.MoveNext()) {
                                var grandChildProp = childEnumerator.Current as SerializedProperty;
                                if (grandChildProp.name.Equals("pathController")) {
                                    if (grandChildProp.objectReferenceValue != null) {
                                        pathLength = (grandChildProp.objectReferenceValue as TrafficPathController).path.pathLength;
                                        lanesCount = (grandChildProp.objectReferenceValue as TrafficPathController).path.lanesCount;
                                    }
                                    EditorGUILayout.BeginHorizontal();
                                    EditorGUILayout.PropertyField(grandChildProp, false);

                                    //picker button
                                    var img = PathPickerEditor.LoadWidgetIcon();

                                    Color GUIColor = GUI.backgroundColor;
                                    if (CursorStateUtility.GetCursorState() && CursorStateUtility.CompareTriggerValues(elementIndex, i) && pickerMode == PickerMode.HighPriority) {
                                        GUI.backgroundColor = Color.green;
                                    }
                                    if (GUILayout.Button(img, GUILayout.MaxWidth(20), GUILayout.MaxHeight(18))) {
                                        if (CursorStateUtility.CompareTriggerValues(elementIndex, i)) {
                                            CursorStateUtility.ClearState();
                                        } else {
                                            pickerMode = PickerMode.HighPriority;
                                            CursorStateUtility.SetState(true, elementIndex, i, -1);
                                        }
                                    }
                                    GUI.backgroundColor = GUIColor;

                                    if (GUILayout.Button("^", GUILayout.MaxWidth(20))) {
                                        cgc.MoveArrayElement(i, i - 1);
                                    }
                                    if (GUILayout.Button("v", GUILayout.MaxWidth(20))) {
                                        cgc.MoveArrayElement(i, i + 1);
                                    }
                                    if (GUILayout.Button("-", GUILayout.MaxWidth(20))) {
                                        cgc.DeleteArrayElementAtIndex(i);
                                        break;
                                    }
                                    EditorGUILayout.EndHorizontal();
                                } else {
                                    if (pathLength == 0f) {
                                        break;
                                    }
                                    EditorGUI.indentLevel++;
                                    if (grandChildProp.name.Equals("umin") || grandChildProp.name.Equals("umax")) {
                                        grandChildProp.floatValue = Mathf.Clamp(grandChildProp.floatValue, 0, pathLength);
                                        EditorGUILayout.Slider(grandChildProp, 0, pathLength);
                                    } else if (grandChildProp.name.Equals("lanes")) {
                                        //limit lanes array
                                        grandChildProp.arraySize = Mathf.Clamp(grandChildProp.arraySize, 1, lanesCount);
                                        EditorGUILayout.BeginHorizontal();
                                        EditorGUILayout.PropertyField(grandChildProp, TrafficPathControllerEditor.FormatName(grandChildProp.name, grandChildProp.arraySize), false, GUILayout.MaxWidth(150));
                                        if (GUILayout.Button("+", GUILayout.MaxWidth(50))) {
                                            grandChildProp.arraySize++;
                                            continue;
                                        }
                                        if (GUILayout.Button("-", GUILayout.MaxWidth(50))) {
                                            grandChildProp.arraySize--;
                                            continue;
                                        }
                                        EditorGUILayout.EndHorizontal();

                                        if (!grandChildProp.isExpanded) {
                                            continue;
                                        }

                                        var laneChildProp = grandChildProp.GetArrayElementAtIndex(0);
                                        laneChildProp.intValue = Mathf.Clamp(laneChildProp.intValue, 0, lanesCount - grandChildProp.arraySize);

                                        //auto set lanes
                                        for (int j = 1; j < grandChildProp.arraySize; j++) {
                                            laneChildProp = grandChildProp.GetArrayElementAtIndex(j);
                                            laneChildProp.intValue = grandChildProp.GetArrayElementAtIndex(j - 1).intValue + 1;
                                        }
                                        //show lanes child
                                        EditorGUI.indentLevel++;
                                        for (int j = 0; j < grandChildProp.arraySize; j++) {
                                            laneChildProp = grandChildProp.GetArrayElementAtIndex(j);
                                            using (new EditorGUI.DisabledScope(j > 0)) {
                                                EditorGUILayout.IntSlider(laneChildProp, 0, lanesCount - 1);
                                            }
                                        }
                                        EditorGUI.indentLevel--;
                                    }
                                    EditorGUI.indentLevel--;
                                }                       
                            }
                            if (pathLength != 0.0f) {
                                EditorGUI.indentLevel--;
                            }

                        }
                    } else if (cgc.name.Equals("lowPriority")) {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.PropertyField(cgc);

                        //picker button
                        var img = PathPickerEditor.LoadWidgetIcon();

                        Color GUIColor = GUI.backgroundColor;
                        
                        if (CursorStateUtility.GetCursorState() && CursorStateUtility.GetTriggerIndex(2) == partitionIndex && pickerMode == PickerMode.LowPriority) {
                            GUI.backgroundColor = Color.green;
                        }
                        if (GUILayout.Button(img, GUILayout.MaxWidth(20), GUILayout.MaxHeight(18))) {
                            if (CursorStateUtility.GetTriggerIndex(2) == partitionIndex) {
                                CursorStateUtility.ClearState();
                            } else {
                                pickerMode = PickerMode.LowPriority;
                                CursorStateUtility.SetState(true, -1, -1, partitionIndex);
                            }
                        }

                        GUI.backgroundColor = GUIColor;
                        EditorGUILayout.EndHorizontal();
                        if (cgc.objectReferenceValue != null) {
                            lowPriorityPathLength = (cgc.objectReferenceValue as TrafficPathController).path.pathLength;
                        }
                    } else if (cgc.name.Equals("lowPriorityYield") || cgc.name.Equals("lowPriorityStop")) {
                        EditorGUI.indentLevel++;
                        cgc.floatValue = Mathf.Clamp(cgc.floatValue, 0, lowPriorityPathLength);
                        EditorGUILayout.Slider(cgc, 0, lowPriorityPathLength);
                        EditorGUI.indentLevel--;
                    }
                }
                EditorGUI.indentLevel--;

                partitionIndex++;
            }
            EditorGUI.indentLevel--;

            EditorGUILayout.HelpBox("For Low Priority: Yield Point must be lower than Stop Point", MessageType.Info);

            so.ApplyModifiedProperties();

            //CursorStateUtility.Debug();
        }

        private void OnSceneGUI()
        {
            if (_target.conflictZones != null) {
                foreach (var zone in _target.conflictZones) {
                    //draw highpriority
                    foreach (var zonePath in zone.highPriorities) {
                        if (zonePath == null) {
                            continue;
                        }
                        if (zonePath.pathController == null) {
                            continue;
                        }
                        var spline = zonePath.pathController.path.GetSplineBuilder();
                        var width = zonePath.pathController.path.calculatedWidth;
                        var lanesCount = zonePath.pathController.path.lanesCount;

                        foreach (var lane in zonePath.lanes) {
                            var startPos = TrafficPathControllerEditor.GetPositionFromLaneIndex(spline, width, zonePath.umin, lane, lanesCount);
                            var endPos = TrafficPathControllerEditor.GetPositionFromLaneIndex(spline, width, zonePath.umax, lane, lanesCount);

                            Handles.DrawLine(startPos, endPos);

                        }
                    }

                    //draw lowpriority
                    if (zone.lowPriority != null) {
                        var spline = zone.lowPriority.path.GetSplineBuilder();
                        var width = zone.lowPriority.path.calculatedWidth;
                        var lanesCount = zone.lowPriority.path.lanesCount;
                        Color handleColor =  Handles.color;
                        for (int i = 0; i < lanesCount; i++) {
                            //yield
                            var yieldPos = TrafficPathControllerEditor.GetPositionFromLaneIndex(spline, width, zone.lowPriorityYield, i, lanesCount);
                            var yieldDir = spline.GetTangentOnPathSegment(zone.lowPriorityYield);                        
                            Vector3 left = Vector3.Cross(Vector3.up, yieldDir);
                            Handles.color = Color.yellow;
                            Handles.DrawLine(yieldPos + left * width, yieldPos + (-left * width));

                            //stop
                            var stopPos = TrafficPathControllerEditor.GetPositionFromLaneIndex(spline, width, zone.lowPriorityStop, i, lanesCount);
                            var stopDir = spline.GetTangentOnPathSegment(zone.lowPriorityStop);
                            left = Vector3.Cross(Vector3.up, stopDir);
                            Handles.color = Color.red;
                            Handles.DrawLine(stopPos + left * width, stopPos + (-left * width));
                        }
                        Handles.color = handleColor;
                    }
                    //draw yield point and stop point
                }
            }

            //path picker
            if (CursorStateUtility.GetCursorState()) {
                if (pickerMode == PickerMode.HighPriority) {
                    TrafficPathController highPrioritySelected = PathPickerEditor.PickPath(_target.gameObject.scene.name, _target/*, ref _target.ramps[cursorIndex].newPath*/);
                    if (highPrioritySelected != null) {
                        Undo.RecordObject(_target, "Seting new pathcontroller");
                        _target.conflictZones[CursorStateUtility.GetTriggerIndex(0)].highPriorities[CursorStateUtility.GetTriggerIndex(1)].pathController = highPrioritySelected;
                        CursorStateUtility.ClearState();
                        HandleUtility.AddDefaultControl(0);
                    }
                } else {
                    TrafficPathController lowPrioritySelected = PathPickerEditor.PickPath(_target.gameObject.scene.name, _target/*, ref _target.ramps[cursorIndex].newPath*/);
                    if (lowPrioritySelected != null) {
                        Undo.RecordObject(_target, "Seting new pathcontroller");
                        _target.conflictZones[CursorStateUtility.GetTriggerIndex(2)].lowPriority = lowPrioritySelected;
                        CursorStateUtility.ClearState();
                        HandleUtility.AddDefaultControl(0);
                    }
                }
            }
        }
    }
    #endif
}

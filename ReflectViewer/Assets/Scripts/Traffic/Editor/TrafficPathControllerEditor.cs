using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Text;

namespace CivilFX.TrafficV5
{
    [CustomEditor(typeof(TrafficPathController))]
    [CanEditMultipleObjects]
    public class TrafficPathControllerEditor : Editor
    {
        private enum VehicleTypeSub
        {
            None = VehicleType.Shadow,
            Car = VehicleType.Car,
            Motorcycle = VehicleType.Motorcycle,
            Truck = VehicleType.Truck
        }


        private SerializedObject so;
        private TrafficPathController _target;

        private void OnEnable()
        {
            so = serializedObject;
            _target = (TrafficPathController)target;

            CursorStateUtility.ClearState();
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

            #region ramps
            //ramps
            currentProp = so.FindProperty("ramps");
            EditorGUILayout.LabelField("Ramps Infomation:", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(currentProp, FormatName(currentProp.name, currentProp.arraySize), false, GUILayout.MaxWidth(150));
            so.FindProperty("showRampHandles").boolValue = EditorGUILayout.Toggle(so.FindProperty("showRampHandles").boolValue, GUILayout.MaxWidth(20));
            if (GUILayout.Button("+", GUILayout.MaxWidth(50)))
            {
                currentProp.arraySize++;
            }
            EditorGUILayout.EndHorizontal();
            //show ramp child
            var rampIndex = 0;
            var childProp = currentProp.GetEnumerator();
            //loop through each element
            while (currentProp.isExpanded && childProp.MoveNext())
            {
                EditorGUI.indentLevel++;
                var currentChild = childProp.Current as SerializedProperty;
                var currentGrandChild = currentChild.GetEnumerator();
                SerializedProperty newPathProp = null;
                SerializedProperty modeProp = null;
                SerializedProperty typeProp = null;
                SerializedProperty directionProp = null;
                SerializedProperty uminNewPathProp = null;
                SerializedProperty uminProp = null;
                SerializedProperty umaxProp = null;
                SerializedProperty isExpandedProp = null;
                SerializedProperty fromLanesProp = null;
                SerializedProperty toLanesProp = null;

                //loop through each field to get temp prop
                while (currentGrandChild.MoveNext())
                {
                    var current = currentGrandChild.Current as SerializedProperty;
                    var name = current.name;
                    if (name.Equals("newPath"))
                    {
                        newPathProp = current.Copy();
                    }
                    else if (name.Equals("mode"))
                    {
                        modeProp = current.Copy();
                    }
                    else if (name.Equals("type"))
                    {
                        typeProp = current.Copy();
                    }
                    else if (name.Equals("direction"))
                    {
                        directionProp = current.Copy();
                    }
                    else if (name.Equals("uminNewPath"))
                    {
                        uminNewPathProp = current.Copy();
                    }
                    else if (name.Equals("umin"))
                    {
                        uminProp = current.Copy();
                    }
                    else if (name.Equals("umax"))
                    {
                        umaxProp = current.Copy();
                    }
                    else if (name.Equals("isExpanded"))
                    {
                        isExpandedProp = current.Copy();
                    }
                    else if (name.Equals("fromLanes"))
                    {
                        fromLanesProp = current.Copy();
                    }
                    else if (name.Equals("toLanes"))
                    {
                        toLanesProp = current.Copy();
                    }
                }

                //init some props
                if (fromLanesProp.arraySize == 0)
                {
                    fromLanesProp.arraySize = 1;
                    toLanesProp.arraySize = 1;
                }

                //draw them
                var pathName = newPathProp.objectReferenceValue == null ? "<Null>" : newPathProp.objectReferenceValue.name;
                isExpandedProp.boolValue = EditorGUILayout.Foldout(isExpandedProp.boolValue, pathName);
                if (isExpandedProp.boolValue)
                {
                    EditorGUI.indentLevel++;
                    //draw path
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(newPathProp, new GUIContent(pathName), GUILayout.MaxWidth(900));
                    var img = PathPickerEditor.LoadWidgetIcon();

                    Color GUIColor = GUI.backgroundColor;

                    if (CursorStateUtility.GetCursorState() && CursorStateUtility.GetTriggerIndex(0) == rampIndex)
                    {
                        GUI.backgroundColor = Color.green;
                    }
                    if (GUILayout.Button(img, GUILayout.MaxWidth(30), GUILayout.MaxHeight(18)))
                    {
                        if (CursorStateUtility.GetTriggerIndex(0) == rampIndex)
                        {
                            CursorStateUtility.SetState(false, -1);
                        }
                        else
                        {
                            CursorStateUtility.SetState(true, rampIndex);
                        }
                    }
                    GUI.backgroundColor = GUIColor;

                    if (GUILayout.Button("^", GUILayout.MaxWidth(50)))
                    {
                        //move up
                        currentProp.MoveArrayElement(rampIndex, rampIndex - 1);
                        so.ApplyModifiedProperties();
                        return;
                    }
                    if (GUILayout.Button("v", GUILayout.MaxWidth(50)))
                    {
                        //move down
                        currentProp.MoveArrayElement(rampIndex, rampIndex + 1);
                        so.ApplyModifiedProperties();
                        return;
                    }
                    if (GUILayout.Button("-", GUILayout.MaxWidth(50)))
                    {
                        currentProp.DeleteArrayElementAtIndex(rampIndex);
                        so.ApplyModifiedProperties();
                        return;
                    }
                    EditorGUILayout.EndHorizontal();

                    if (newPathProp.objectReferenceValue == null)
                    {
                        EditorGUILayout.HelpBox("Assign path controller!", MessageType.Info);
                        EditorGUI.indentLevel--;
                        EditorGUI.indentLevel--;
                        continue;
                    }

                    var type = (Ramp.RampType)typeProp.enumValueIndex;
                    //draw type
                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField(typeProp);
                    if (EditorGUI.EndChangeCheck())
                    {
                        type = (Ramp.RampType)typeProp.enumValueIndex;
                        if (type == Ramp.RampType.OnRamp || type == Ramp.RampType.OffRamp)
                        {
                            modeProp.enumValueIndex = (int)Ramp.RampMode.Auto;
                        }
                    }

                    //draw mode
                    EditorGUI.BeginChangeCheck();
                    using (new EditorGUI.DisabledScope(type == Ramp.RampType.OnRamp || type == Ramp.RampType.OffRamp))
                    {
                        EditorGUILayout.PropertyField(modeProp);
                    }
                    if (EditorGUI.EndChangeCheck())
                    {
                        fromLanesProp.ClearArray();
                        toLanesProp.ClearArray();
                        fromLanesProp.arraySize++;
                        toLanesProp.arraySize++;
                    }
                    switch (modeProp.enumValueIndex)
                    {
                        case 0:
                            //auto
                            EditorGUILayout.PropertyField(directionProp);
                            break;
                        case 1:
                            //manual
                            //show from lanes to lanes
                            //EditorGUILayout.PropertyField();
                            var removedIndex = -1;
                            var newPathLanesCount = (newPathProp.objectReferenceValue as TrafficPathController).path.lanesCount;
                            var lanesCount = _target.path.lanesCount;
                            EditorGUILayout.BeginVertical();
                            EditorGUILayout.BeginHorizontal();
                            EditorGUIUtility.labelWidth = 1;
                            EditorGUILayout.LabelField("From:");
                            EditorGUIUtility.labelWidth = 0;
                            if (GUILayout.Button("+", GUILayout.MaxHeight(15), GUILayout.MaxWidth(30)))
                            {
                                if (fromLanesProp.arraySize < lanesCount && toLanesProp.arraySize < newPathLanesCount)
                                {
                                    fromLanesProp.arraySize++;
                                    toLanesProp.arraySize++;
                                }
                            }
                            EditorGUILayout.Space();
                            EditorGUILayout.LabelField("To:");
                            EditorGUILayout.EndHorizontal();
                            EditorGUILayout.Space();
                            //draw row
                            EditorGUI.indentLevel++;
                            //auto set rows                          
                            var fromValue = fromLanesProp.GetArrayElementAtIndex(0).intValue;
                            fromValue = Mathf.Clamp(fromValue, 0, lanesCount - fromLanesProp.arraySize);
                            fromLanesProp.GetArrayElementAtIndex(0).intValue = fromValue;

                            var toValue = toLanesProp.GetArrayElementAtIndex(0).intValue;
                            toValue = Mathf.Clamp(toValue, 0, newPathLanesCount - toLanesProp.arraySize);
                            toLanesProp.GetArrayElementAtIndex(0).intValue = toValue;

                            for (int i = 1; i < fromLanesProp.arraySize; i++)
                            {
                                fromLanesProp.GetArrayElementAtIndex(i).intValue = fromValue + i;
                                toLanesProp.GetArrayElementAtIndex(i).intValue = toValue + i;
                            }


                            for (int i = 0; i < fromLanesProp.arraySize; i++)
                            {
                                EditorGUILayout.BeginHorizontal();
                                //only draw the first row
                                //subsequence rows are automatically set above
                                using (new EditorGUI.DisabledScope(i != 0))
                                {
                                    //EditorGUILayout.IntSlider(fromLanesProp.GetArrayElementAtIndex(i), 0, lanesCount - 1, new GUIContent(), GUILayout.MaxWidth(200));
                                    //EditorGUILayout.IntSlider(toLanesProp.GetArrayElementAtIndex(i), 0, newPathLanesCount - 1, new GUIContent(), GUILayout.MaxWidth(200));
                                    EditorGUILayout.PropertyField(fromLanesProp.GetArrayElementAtIndex(i), new GUIContent(""));
                                    EditorGUILayout.PropertyField(toLanesProp.GetArrayElementAtIndex(i), new GUIContent(""));
                                }
                                if (GUILayout.Button("-", GUILayout.MaxWidth(50), GUILayout.MinWidth(50)))
                                {
                                    removedIndex = i;
                                }
                                EditorGUILayout.EndHorizontal();
                            }

                            if (removedIndex >= 0)
                            {
                                fromLanesProp.DeleteArrayElementAtIndex(removedIndex);
                                toLanesProp.DeleteArrayElementAtIndex(removedIndex);
                            }
                            EditorGUI.indentLevel--;
                            EditorGUILayout.EndVertical();
                            break;
                    }

                    //draw uminnewpath
                    using (new EditorGUI.DisabledScope(true))
                    {
                        EditorGUILayout.PropertyField(uminNewPathProp);
                    }

                    var typeSelection = (Ramp.RampType)typeProp.enumValueIndex;
                    var pathLength = _target.path.pathLength;
                    if (typeSelection == Ramp.RampType.OnRamp || typeSelection == Ramp.RampType.OffRamp)
                    {
                        //draw umin
                        uminProp.floatValue = Mathf.Clamp(uminProp.floatValue, 0, pathLength);
                        EditorGUILayout.Slider(uminProp, 0, pathLength);
                    }

                    if (typeSelection == Ramp.RampType.Merged && umaxProp.floatValue == 0f)
                    {
                        umaxProp.floatValue = pathLength;
                    }

                    //draw umax
                    umaxProp.floatValue = Mathf.Clamp(umaxProp.floatValue, 0, pathLength);
                    EditorGUILayout.Slider(umaxProp, 0, pathLength);

                    //visual to debug from to lanes
                    if (type == Ramp.RampType.Merged || type == Ramp.RampType.Diverged)
                    {
                        var sb = new StringBuilder();
                        for (int i = 0; i < fromLanesProp.arraySize; i++)
                        {
                            sb.Append(fromLanesProp.GetArrayElementAtIndex(i).intValue);
                            sb.Append("|");
                        }
                        sb.Append(" -> ");
                        for (int i = 0; i < toLanesProp.arraySize; i++)
                        {
                            sb.Append(toLanesProp.GetArrayElementAtIndex(i).intValue);
                            sb.Append("|");
                        }
                        EditorGUILayout.HelpBox($"Lanes: {sb.ToString()}", MessageType.Info, true);
                    }
                    //connect button
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.Space();
                    if (GUILayout.Button("Connect"))
                    {
                        ConnectPaths(newPathProp, typeProp, modeProp, directionProp, fromLanesProp, toLanesProp, uminNewPathProp, uminProp, umaxProp);
                    }
                    EditorGUILayout.Space();
                    EditorGUILayout.EndHorizontal();

                    //done
                    EditorGUI.indentLevel--;
                }

                ++rampIndex;
                EditorGUI.indentLevel--;
            }



            #endregion
            EditorGUILayout.Space();

            #region obstacles
            //obstacles
            EditorGUILayout.LabelField("Obstacles Infomation:", EditorStyles.boldLabel);
            currentProp = so.FindProperty("obstacles");
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(currentProp, FormatName(currentProp.name, currentProp.arraySize), false, GUILayout.MaxWidth(150));
            so.FindProperty("showObstacleHandles").boolValue = EditorGUILayout.Toggle(so.FindProperty("showObstacleHandles").boolValue, GUILayout.MaxWidth(20));
            if (GUILayout.Button("+", GUILayout.MaxWidth(50)))
            {
                currentProp.arraySize++;
            }
            EditorGUILayout.EndHorizontal();
            //show obstacles child
            childProp = currentProp.GetEnumerator();
            EditorGUI.indentLevel++;
            var obstaclesIndex = 0;
            while (currentProp.isExpanded && childProp.MoveNext())
            {

                var ccp = childProp.Current as SerializedProperty;
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(ccp, false, GUILayout.MaxWidth(900));
                if (GUILayout.Button("-", GUILayout.MaxWidth(50)))
                {
                    currentProp.DeleteArrayElementAtIndex(obstaclesIndex);
                    continue;
                }
                EditorGUILayout.EndHorizontal();

                //grandchild
                var cccp = ccp.Copy().GetEnumerator();
                var pathLength = _target.path.pathLength;
                var lanesCount = _target.path.lanesCount;
                EditorGUI.indentLevel++;
                while (ccp.isExpanded && cccp.MoveNext())
                {
                    var cgc = cccp.Current as SerializedProperty;
                    if (cgc.propertyPath.EndsWith("u"))
                    {
                        EditorGUILayout.Slider(cgc, 0, pathLength);
                    }
                    else if (cgc.propertyPath.EndsWith("lane"))
                    {
                        EditorGUILayout.IntSlider(cgc, 0, lanesCount - 1);
                    }
                }
                EditorGUI.indentLevel--;

                obstaclesIndex++;
            }
            EditorGUI.indentLevel--;
            #endregion

            EditorGUILayout.Space();
            #region PartitionSegments
            //partition segment
            EditorGUILayout.LabelField("Partition Infomation:", EditorStyles.boldLabel);
            currentProp = so.FindProperty("partitionSegments");
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(currentProp, FormatName(currentProp.name, currentProp.arraySize), false, GUILayout.MaxWidth(150));
            so.FindProperty("showPartitionHandles").boolValue = EditorGUILayout.Toggle(so.FindProperty("showPartitionHandles").boolValue, GUILayout.MaxWidth(20));
            if (GUILayout.Button("+", GUILayout.MaxWidth(50)))
            {
                currentProp.arraySize++;
            }
            EditorGUILayout.EndHorizontal();
            //show partition child
            childProp = currentProp.GetEnumerator();
            EditorGUI.indentLevel++;
            var partitionIndex = 0;
            while (currentProp.isExpanded && childProp.MoveNext())
            {
                var ccp = childProp.Current as SerializedProperty;
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(ccp, false, GUILayout.MaxWidth(900));
                if (GUILayout.Button("-", GUILayout.MaxWidth(50)))
                {
                    currentProp.DeleteArrayElementAtIndex(partitionIndex);
                    continue;
                }
                EditorGUILayout.EndHorizontal();
                //grandchild
                var cccp = ccp.Copy().GetEnumerator();
                var pathLength = _target.path.pathLength;
                var lanesCount = _target.path.lanesCount;
                EditorGUI.indentLevel++;
                while (ccp.isExpanded && cccp.MoveNext())
                {
                    var cgc = cccp.Current as SerializedProperty;
                    if (cgc.propertyPath.EndsWith("start") || cgc.propertyPath.EndsWith("end"))
                    {
                        EditorGUILayout.Slider(cgc, 0, pathLength);
                    }
                    else if (cgc.propertyPath.EndsWith("lane"))
                    {
                        EditorGUILayout.IntSlider(cgc, 0, lanesCount - 1);
                    }
                }
                EditorGUI.indentLevel--;

                partitionIndex++;
            }
            EditorGUI.indentLevel--;
            #endregion

            #region yields
            //yields
            EditorGUILayout.LabelField("Yields Infomation:", EditorStyles.boldLabel);
            currentProp = so.FindProperty("yieldPoints");
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(currentProp, FormatName(currentProp.name, currentProp.arraySize), false, GUILayout.MaxWidth(150));
            so.FindProperty("showYieldHandles").boolValue = EditorGUILayout.Toggle(so.FindProperty("showYieldHandles").boolValue, GUILayout.MaxWidth(20));
            if (GUILayout.Button("+", GUILayout.MaxWidth(50)))
            {
                currentProp.arraySize++;
            }
            EditorGUILayout.EndHorizontal();
            //show obstacles child
            childProp = currentProp.GetEnumerator();
            EditorGUI.indentLevel++;
            var yieldsIndex = 0;
            while (currentProp.isExpanded && childProp.MoveNext())
            {

                var ccp = childProp.Current as SerializedProperty;
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(ccp, false, GUILayout.MaxWidth(900));
                if (GUILayout.Button("-", GUILayout.MaxWidth(50)))
                {
                    currentProp.DeleteArrayElementAtIndex(yieldsIndex);
                    continue;
                }
                EditorGUILayout.EndHorizontal();

                //grandchild
                var cccp = ccp.Copy().GetEnumerator();
                var pathLength = _target.path.pathLength;
                var lanesCount = _target.path.lanesCount;
                EditorGUI.indentLevel++;
                while (ccp.isExpanded && cccp.MoveNext())
                {
                    var cgc = cccp.Current as SerializedProperty;
                    if (cgc.name.Equals("u"))
                    {
                        EditorGUILayout.Slider(cgc, 0, pathLength);
                    }
                }
                EditorGUI.indentLevel--;
                yieldsIndex++;
            }
            EditorGUI.indentLevel--;
            #endregion


            #region exclude vehicletype
            EditorGUILayout.LabelField("Exclude type:", EditorStyles.boldLabel);
            currentProp = so.FindProperty("excludeType");
            var excludeType = (VehicleTypeSub)currentProp.enumValueIndex;
            EditorGUI.BeginChangeCheck();
            excludeType = (VehicleTypeSub)EditorGUILayout.EnumPopup(new GUIContent("Exclude Type"), excludeType);
            if (EditorGUI.EndChangeCheck())
            {
                currentProp.enumValueIndex = (int)excludeType;
            }


            #endregion

            EditorGUILayout.Space();
            #region extra settings
            EditorGUILayout.LabelField("Extra Settings:", EditorStyles.boldLabel);
            currentProp = so.FindProperty("initialVehiclesCount");
            EditorGUILayout.PropertyField(currentProp);
            currentProp = so.FindProperty("allowRespawning");
            EditorGUILayout.PropertyField(currentProp);
            using (new EditorGUI.DisabledGroupScope(!currentProp.boolValue))
            {
                currentProp = so.FindProperty("inflowCount");
                EditorGUILayout.PropertyField(currentProp);
            }
            currentProp = so.FindProperty("allowDespawning");
            EditorGUILayout.PropertyField(currentProp);

            #endregion

            so.ApplyModifiedProperties();

        }

        private void OnSceneGUI()
        {
            //ramps
            SplineBuilder spline = _target.path.GetSplineBuilder();
            var pathLength = _target.path.pathLength;
            var lanesCount = _target.path.lanesCount;
            if (_target.showRampHandles)
            {
                foreach (var ramp in _target.ramps)
                {
                    if (ramp.newPath == null)
                    {
                        continue;
                    }
                    if (ramp.type == Ramp.RampType.OnRamp || ramp.type == Ramp.RampType.OffRamp)
                    {
                        var startNode = spline.GetPointOnPathSegment(ramp.umin);
                        Handles.Label(startNode, "umin:" + ramp.newPath.gameObject.name);
                        Handles.PositionHandle(startNode, Quaternion.identity);
                    }
                    var endNode = spline.GetPointOnPathSegment(ramp.umax);
                    Handles.Label(endNode, "umax:" + ramp.newPath.gameObject.name);
                    Handles.PositionHandle(endNode, Quaternion.identity);
                    //Handles.ArrowHandleCap(0, endNode, Quaternion.LookRotation(-Vector3.up), 10f, EventType.Repaint);
                }
            }
            //obstacles
            if (_target.showObstacleHandles)
            {
                foreach (var obstacle in _target.obstacles)
                {
                    var node = GetPositionFromLaneIndex(spline, _target.path.calculatedWidth, obstacle.u, obstacle.lane, lanesCount);
                    Handles.Label(node, "Obstacle:");
                    Handles.PositionHandle(node, Quaternion.identity);
                }
            }

            //partition
            if (_target.showPartitionHandles)
            {
                foreach (var segment in _target.partitionSegments)
                {
                    var startNode = GetPositionFromLaneIndex(spline, _target.path.calculatedWidth, segment.start, segment.lane, lanesCount);
                    var endNode = GetPositionFromLaneIndex(spline, _target.path.calculatedWidth, segment.end, segment.lane, lanesCount);
                    Handles.Label(startNode, "Partition Start");
                    Handles.PositionHandle(startNode, Quaternion.identity);

                    Handles.Label(endNode, "Partition End");
                    Handles.PositionHandle(endNode, Quaternion.identity);
                }
            }

            //yields
            if (_target.showYieldHandles)
            {
                foreach (var point in _target.yieldPoints)
                {
                    for (int i = 0; i < _target.path.lanesCount; i++)
                    {
                        var startNode = GetPositionFromLaneIndex(spline, _target.path.calculatedWidth, point.u, i, lanesCount);
                        var target = -(startNode + Vector3.up * 5f) + startNode;
                        Handles.Label(startNode, $"Yield");
                        Handles.ConeHandleCap(0, startNode, Quaternion.LookRotation(target, Vector3.up), 1f, EventType.Repaint);
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
                    _target.ramps[CursorStateUtility.GetTriggerIndex(0)].newPath = selected;
                    CursorStateUtility.SetState(false, -1);
                    HandleUtility.AddDefaultControl(0);
                }
            }
        }

        private void ConnectPaths(SerializedProperty newPathProp, SerializedProperty typeProp, SerializedProperty modeProp, SerializedProperty directionProp, SerializedProperty fromLanesProp, SerializedProperty toLanesProp, SerializedProperty uminNewPathProp, SerializedProperty uminProp, SerializedProperty umaxProp)
        {
            var typeSelection = (Ramp.RampType)typeProp.enumValueIndex;
            var modeSelection = (Ramp.RampMode)modeProp.enumValueIndex;
            var directionSelection = (Ramp.RampDirection)directionProp.enumValueIndex;
            var currentSpline = _target.path.GetSplineBuilder(true);
            var newPath = (newPathProp.objectReferenceValue as TrafficPathController).path;
            var newSpline = newPath.GetSplineBuilder(true);
            var newPathSO = new SerializedObject(newPath);
            var pathLength = _target.path.pathLength;
            var lanesCount = _target.path.lanesCount;
            newPathSO.Update();
            if (typeSelection == Ramp.RampType.Merged || typeSelection == Ramp.RampType.Diverged)
            {
                uminNewPathProp.floatValue = 0f;
                var fromLane = 0;
                var toLane = 0;
                var compatibleLanesCount = newPath.lanesCount;
                //determine from to lanes
                if (modeSelection == Ramp.RampMode.Auto)
                {
                    if (_target.path.lanesCount > newPath.lanesCount)
                    {
                        if (directionSelection == Ramp.RampDirection.ToRight)
                        {
                            fromLane = _target.path.lanesCount - newPath.lanesCount;
                        }
                        else
                        {
                            fromLane = 0;
                        }
                        toLane = 0;
                        compatibleLanesCount = newPath.lanesCount;
                    }
                    else if (_target.path.lanesCount < newPath.lanesCount)
                    {
                        if (directionSelection == Ramp.RampDirection.ToRight)
                        {
                            toLane = newPath.lanesCount - _target.path.lanesCount;
                        }
                        else
                        {
                            toLane = 0;
                        }
                        fromLane = 0;
                        compatibleLanesCount = _target.path.lanesCount;
                    }
                    else
                    {
                        fromLane = 0;
                        toLane = 0;
                    }
                    //calculate fromLanes toLanes
                    fromLanesProp.arraySize = compatibleLanesCount;
                    toLanesProp.arraySize = compatibleLanesCount;
                    for (int i = 0; i < compatibleLanesCount; i++)
                    {
                        fromLanesProp.GetArrayElementAtIndex(i).intValue = fromLane + i;
                        toLanesProp.GetArrayElementAtIndex(i).intValue = toLane + i;
                    }
                }
                else
                {
                    //manual
                    fromLane = fromLanesProp.GetArrayElementAtIndex(0).intValue;
                    toLane = toLanesProp.GetArrayElementAtIndex(0).intValue;
                }
                var posStart = GetPositionFromLaneIndex(currentSpline, _target.path.calculatedWidth, umaxProp.floatValue - 0.02f, fromLane, lanesCount);
                var posEnd = GetPositionFromLaneIndex(currentSpline, _target.path.calculatedWidth, umaxProp.floatValue - 0.01f, fromLane, lanesCount);
                var dir = (posEnd - posStart).normalized;
                var left = Vector3.Cross(dir, Vector3.up);

                var newPathRawPos = newSpline.GetPointOnPath(0);
                var newPathLanePos = GetPositionFromLaneIndex(newSpline, newPath.calculatedWidth, 0, toLane, newPath.lanesCount);
                var dis = Vector3.Distance(newPathRawPos, newPathLanePos);
                dis = Mathf.Abs(dis);
                var physicalDir = toLane < newPath.lanesCount / 2f ? -1 : 1;
                var newPos = posEnd + (left * physicalDir) * dis;

                var nodesSP = newPathSO.FindProperty("nodes");
                var nodes = new List<Vector3>(nodesSP.arraySize);
                for (int i = 0; i < nodesSP.arraySize; i++)
                {
                    nodes.Add(nodesSP.GetArrayElementAtIndex(i).vector3Value);
                }
                var nearestIndex = GetNearestNodeIndex(nodes, newPos);
                nearestIndex = Mathf.Clamp(nearestIndex, 1, nodesSP.arraySize - 2);
                Debug.Log(nearestIndex);
                //calculate uminnewpath
                nodes.Clear();
                for (int i = 0; i <= nearestIndex + 1; i++)
                {
                    nodes.Add(nodesSP.GetArrayElementAtIndex(i).vector3Value);
                }
                if (nodes.Count >= 4)
                {
                    SplineBuilder builder = new SplineBuilder(nodes);
                    uminNewPathProp.floatValue = builder.pathLength;
                }
                else
                {
                    uminNewPathProp.floatValue = 0;
                }

                newPathSO.FindProperty("nodes").GetArrayElementAtIndex(nearestIndex).vector3Value = newPos;
            }
            else
            {
                //onramp offramp
                var mainWidth = _target.path.calculatedWidth;
                var rampWidth = newPath.calculatedWidth;
                var segment = (umaxProp.floatValue - uminProp.floatValue) / 3f;
                Vector3 node0, node1, node2, node3;

                //calculate new nodes
                node0 = GetNode(currentSpline, uminProp.floatValue, pathLength, mainWidth, rampWidth, typeSelection, directionSelection);
                node1 = GetNode(currentSpline, uminProp.floatValue + segment, pathLength, mainWidth, rampWidth, typeSelection, directionSelection);
                node2 = GetNode(currentSpline, uminProp.floatValue + segment * 2, pathLength, mainWidth, rampWidth, typeSelection, directionSelection);
                node3 = GetNode(currentSpline, umaxProp.floatValue, pathLength, mainWidth, rampWidth, typeSelection, directionSelection);

                //set nodes
                var newPathNodesProp = newPathSO.FindProperty("nodes");
                //copy nodes to list
                var nodes = new List<Vector3>(newPathNodesProp.arraySize);
                for (int i = 0; i < newPathNodesProp.arraySize; i++)
                {
                    nodes.Add(newPathNodesProp.GetArrayElementAtIndex(i).vector3Value);
                }
                var firstIndex = GetNearestNodeIndex(nodes, node0);
                firstIndex = firstIndex == 0 ? 1 : firstIndex;

                if (nodes.Count >= 6)
                {
                    //first node
                    newPathNodesProp.GetArrayElementAtIndex(firstIndex).vector3Value = node0;
                    //second node
                    newPathNodesProp.GetArrayElementAtIndex(firstIndex + 1).vector3Value = node1;
                    //third node
                    newPathNodesProp.GetArrayElementAtIndex(firstIndex + 2).vector3Value = node2;
                    //last node
                    newPathNodesProp.GetArrayElementAtIndex(firstIndex + 3).vector3Value = node3;
                    //before first node
                    newPathNodesProp.GetArrayElementAtIndex(firstIndex - 1).vector3Value = node0 + (node0 - node1);
                }
                else
                {
                    Debug.LogError($"{newPathNodesProp.name} needs at least 6 nodes");
                }

                //set uminnewpath
                nodes.Clear();
                nodes.Add(newPathNodesProp.GetArrayElementAtIndex(firstIndex + 1).vector3Value);
                while (firstIndex >= 0)
                {
                    nodes.Add(newPathNodesProp.GetArrayElementAtIndex(firstIndex).vector3Value);
                    --firstIndex;
                }
                if (nodes.Count >= 4)
                {
                    SplineBuilder builder = new SplineBuilder(nodes);
                    uminNewPathProp.floatValue = builder.pathLength;
                }
                else
                {
                    uminNewPathProp.floatValue = 0;
                }
            }
            newPathSO.ApplyModifiedProperties();
            newPathSO.Dispose();
        }

        private void CreateObject(Vector3 pos, PrimitiveType type = PrimitiveType.Cube)
        {
            GameObject go = GameObject.CreatePrimitive(type);
            go.transform.position = pos;

        }

        private Vector3 GetNode(SplineBuilder spline, float segmentLength, float maxLength, float mainWidth, float rampWidth, Ramp.RampType type, Ramp.RampDirection direction)
        {
            Vector3 node;
            //var start = spline.GetPointOnPathSegment(segmentLength);
            //var end = spline.GetPointOnPathSegment(segmentLength + 0.001f);
            //var dir = Vector3.Normalize(end - start);
            var dir = spline.GetTangentOnPathSegment(segmentLength);
            var cross = Vector3.Cross(dir, Vector3.up);
            Vector3 physicalDir;
            if (type == Ramp.RampType.OnRamp || type == Ramp.RampType.OffRamp)
            {
                physicalDir = direction == Ramp.RampDirection.ToLeft ? cross : -cross;
            }
            else
            {
                physicalDir = direction == Ramp.RampDirection.ToLeft ? -cross : cross;
            }

            node = spline.GetPointOnPath(segmentLength / maxLength) + (physicalDir * (mainWidth + rampWidth));
            return node;
        }

        static public Vector3 GetPositionFromLaneIndex(SplineBuilder spline, float width, float u, int lane, int lanesCount)
        {
            var centerStart = spline.GetPointOnPathSegment(u);
            var dir = spline.GetTangentOnPathSegment(u);
            var right = Vector3.Cross(Vector3.up, dir) * width;
            var left = -right;
            var seg = (2f * lane + 1f) / (2f * lanesCount); //lerp value based on lane number
            var pos = Vector3.Lerp(centerStart + left, centerStart + right, seg); //actualy position based on lane number
            return pos;
        }

        private int GetNearestNodeIndex(List<Vector3> nodes, Vector3 node)
        {
            int index = -1;
            float minDis = float.MaxValue;
            for (int i = 0; i < nodes.Count; i++)
            {
                float temp = Vector3.Distance(nodes[i], node);
                if (temp < minDis)
                {
                    minDis = temp;
                    index = i;
                }
            }
            return index;
        }

        static public GUIContent FormatName(string name, int size)
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
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CivilFX.TrafficV5
{
    [CustomEditor(typeof(VehicleController))]
    public class VehicleControllerEditor : Editor
    {
        private enum VehicleTypeSub
        {
            Car = VehicleType.Car,
            Motorcycle = VehicleType.Motorcycle,
            Truck = VehicleType.Truck,
            Biker = VehicleType.Biker,
            Pedestrian = VehicleType.Pedestrian
        }

        private SerializedObject so;
        private VehicleController _target;


        private bool enableMeasureTool;
        private Vector3 head;
        private Vector3 tail;
        private float distance;
        private bool shouldDisplayHelpBox;
        private void OnEnable()
        {
            so = serializedObject;
            _target = (VehicleController)target;

            so.Update();
            var transSP = so.FindProperty("vehicleTrans");
            if (transSP.objectReferenceValue == null)
            {
                transSP.objectReferenceValue = _target.transform;
            }
            so.ApplyModifiedProperties();

            head = _target.transform.position;
            tail = head;
            tail.z += 5;
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

            //dimensions
            EditorGUILayout.LabelField("Vehicle Dimensions:", EditorStyles.boldLabel);
            EditorGUI.BeginChangeCheck();
            enableMeasureTool = EditorGUILayout.Toggle("Enable Measure Tool", enableMeasureTool);
            if (EditorGUI.EndChangeCheck())
            {
                Tools.hidden = enableMeasureTool;
                if (enableMeasureTool)
                {
                    head = _target.transform.position;
                }
            }



            //frontOffset
            EditorGUILayout.BeginHorizontal();
            currentProp = so.FindProperty("frontOffset");
            EditorGUILayout.PropertyField(currentProp);
            EditorGUILayout.Space();
            using (new EditorGUI.DisabledScope(!enableMeasureTool))
            {
                if (GUILayout.Button("Paste", GUILayout.MaxWidth(100)))
                {
                    currentProp.floatValue = distance;
                }
            }
            EditorGUILayout.EndHorizontal();
            //rearOffset
            EditorGUILayout.BeginHorizontal();
            currentProp = so.FindProperty("rearOffset");
            EditorGUILayout.PropertyField(currentProp);
            EditorGUILayout.Space();
            using (new EditorGUI.DisabledScope(!enableMeasureTool))
            {
                if (GUILayout.Button("Paste", GUILayout.MaxWidth(100)))
                {
                    currentProp.floatValue = distance;
                }
            }
            EditorGUILayout.EndHorizontal();

            //length
            currentProp = so.FindProperty("vehicleLength");
            using (new EditorGUI.DisabledScope(true))
            {
                currentProp.floatValue = so.FindProperty("frontOffset").floatValue + so.FindProperty("rearOffset").floatValue;
                EditorGUILayout.PropertyField(currentProp);
            }

            //vehicleType
            currentProp = so.FindProperty("vehicleType");
            var currentType = (VehicleTypeSub)currentProp.enumValueIndex;
            EditorGUI.BeginChangeCheck();
            currentType = (VehicleTypeSub)EditorGUILayout.EnumPopup("Vehicle Type", currentType);
            if (EditorGUI.EndChangeCheck())
            {
                currentProp.enumValueIndex = (int)currentType;
            }

            //vehicleTrans
            currentProp = so.FindProperty("vehicleTrans");
            EditorGUILayout.PropertyField(currentProp);
            //trailerTrans
            if ((VehicleType)so.FindProperty("vehicleType").enumValueIndex == VehicleType.Truck)
            {
                currentProp = so.FindProperty("trailerTrans");
                EditorGUILayout.PropertyField(currentProp);
            }

            //vehicle wheels
            currentProp = so.FindProperty("wheels");
            GUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(currentProp, new GUIContent(""), false, GUILayout.MaxWidth(1));
            EditorGUILayout.LabelField("Wheels", EditorStyles.boldLabel, GUILayout.MaxWidth(100));
            if (GUILayout.Button("Get Wheels", GUILayout.MaxWidth(100)))
            {
                var children = GetChildren(_target.transform);
                currentProp.arraySize = children.Count;
                for (int i = 0; i < children.Count; i++)
                {
                    currentProp.GetArrayElementAtIndex(i).objectReferenceValue = children[i];
                }
            }
            EditorGUILayout.EndHorizontal();
            if (currentProp.isExpanded)
            {
                EditorGUI.indentLevel++;
                for (int i = 0; i < currentProp.arraySize; i++)
                {
                    GUILayout.BeginHorizontal();
                    SerializedProperty nodeProp = currentProp.GetArrayElementAtIndex(i);
                    EditorGUILayout.PropertyField(nodeProp, new GUIContent(""));

                    //delete button
                    if (GUILayout.Button(new GUIContent("X", "Delete this node"), GUILayout.MaxWidth(50)))
                    {
                        currentProp.MoveArrayElement(i, currentProp.arraySize - 1);
                        currentProp.arraySize -= 1;
                    }
                    GUILayout.EndHorizontal();
                }
                EditorGUI.indentLevel--;
            }

            //wheel axis
            currentProp = so.FindProperty("wheelAxis");
            EditorGUILayout.PropertyField(currentProp);

            //debug
            currentProp = so.FindProperty("debug");
            EditorGUILayout.PropertyField(currentProp);

            currentProp = so.FindProperty("externalLead");
            EditorGUILayout.PropertyField(currentProp);

            currentProp = so.FindProperty("longModel");
            EditorGUILayout.PropertyField(currentProp);
            currentProp = so.FindProperty("LCModel");
            EditorGUILayout.PropertyField(currentProp);

            so.ApplyModifiedProperties();
        }

        private void OnSceneGUI()
        {
            if (enableMeasureTool)
            {
                Handles.color = Color.black;
                tail = Handles.PositionHandle(tail, Quaternion.identity);
                tail.x = head.x;
                tail.y = head.y;
                Handles.DrawLine(head, tail);
                distance = Vector3.Distance(head, tail);
                Handles.Label(Vector3.Lerp(head, tail, 0.5f), new GUIContent(distance.ToString()), new GUIStyle { fontSize = 30, fontStyle = FontStyle.Bold });
            }
        }

        private List<Transform> GetChildren(Transform parent)
        {
            List<Transform> result = new List<Transform>();
            GetChildrenRecursive(parent, result);
            return result;

        }

        private void GetChildrenRecursive(Transform parent, List<Transform> result)
        {
            if (parent.childCount == 0)
            {
                return;
            }
            for (int i = 0; i < parent.childCount; i++)
            {
                var child = parent.GetChild(i);
                result.Add(child);
                GetChildrenRecursive(child, result);
            }
        }

    }
}
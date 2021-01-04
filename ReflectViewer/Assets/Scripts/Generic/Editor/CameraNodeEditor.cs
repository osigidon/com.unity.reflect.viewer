using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace CivilFX.Generic2
{
    [CustomEditor(typeof(CameraNode))]
    public class CameraNodeEditor : Editor
    {
        SerializedObject so;
        private void OnEnable()
        {
            so = serializedObject;
        }

        public override void OnInspectorGUI()
        {
            so.Update();
            using (new EditorGUI.DisabledGroupScope(true)) {
                EditorGUILayout.PropertyField(so.FindProperty("m_Script"));
            }
            EditorGUILayout.PropertyField(so.FindProperty("order"));
            var fovProp = so.FindProperty("fov");
            EditorGUILayout.PropertyField(fovProp);
            using (new EditorGUI.DisabledGroupScope(!fovProp.boolValue)) {
                EditorGUILayout.Slider(so.FindProperty("fovValue"), 10, 100);
            }
            so.ApplyModifiedProperties();
        }
    }
}
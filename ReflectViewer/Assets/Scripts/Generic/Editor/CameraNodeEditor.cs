using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if (UNITY_EDITOR)
using UnityEditor;
#endif

namespace CivilFX.Generic2
{
    #if (UNITY_EDITOR)
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
                EditorGUILayout.Slider(so.FindProperty("fovValue"), 15, 90);
            }
            so.ApplyModifiedProperties();
        }
    }
    #endif
}

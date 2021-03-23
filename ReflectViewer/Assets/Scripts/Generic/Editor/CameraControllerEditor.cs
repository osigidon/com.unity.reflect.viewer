using System.Collections;
using System.Collections.Generic;
#if (UNITY_EDITOR)
using UnityEditor;
using UnityEditor.IMGUI.Controls;
#endif
using UnityEngine;

namespace CivilFX.Generic2
{
    #if (UNITY_EDITOR)
    [CustomEditor(typeof(CameraController))]
    public class CameraControllerEditor : Editor
    {
        private BoxBoundsHandle boxHandle;

        private SerializedObject so;
        private CameraController _target;

        private bool drawBoxHandle;
        private void OnEnable()
        {
            drawBoxHandle = false;
            boxHandle = new BoxBoundsHandle();
            so = serializedObject;
            _target = (CameraController)target;
        }

        public override void OnInspectorGUI()
        {
            so.Update();

            //show script name
            using (new EditorGUI.DisabledScope(true)) {
                EditorGUILayout.PropertyField(so.FindProperty("m_Script"));
            }

            SerializedProperty sp;

            EditorGUILayout.LabelField("Bounding Box:", EditorStyles.boldLabel);
            sp = so.FindProperty("useBoundingBox");
            EditorGUILayout.PropertyField(sp);

            using (new EditorGUI.DisabledScope(!sp.boolValue)) {
                EditorGUILayout.PropertyField(so.FindProperty("boxCenter"));
                EditorGUILayout.PropertyField(so.FindProperty("boxSize"));
            }



            so.ApplyModifiedProperties();

        }

        private void OnSceneGUI()
        {
            //draw bounding box
            if (_target.useBoundingBox) {
                boxHandle.size = _target.boxSize;
                boxHandle.center = _target.boxCenter;
                boxHandle.DrawHandle();
                Undo.RecordObject(_target, "CameraBoundingBox");
                _target.boxSize = boxHandle.size;
                _target.boxCenter = boxHandle.center;
            }
        }
    }

    #endif
}

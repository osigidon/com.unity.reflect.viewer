using System.Collections;
using System.Collections.Generic;
#if (UNITY_EDITOR)
using UnityEditor;
#endif
using UnityEngine;

namespace Entropedia
{
    #if (UNITY_EDITOR)
    [CustomEditor(typeof(SunController))]
    public class SunControllerEditor : Editor
    {
        private SerializedObject so;
        private SunController _target;
        private void OnEnable()
        {
            so = serializedObject;
            _target = (SunController)target;
        }


        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (GUILayout.Button("Apply")) {
                Undo.RecordObject(_target.gameObject, "Setting Sun");
                _target.SetPosition();
            }
        }
    }
    #endif
}

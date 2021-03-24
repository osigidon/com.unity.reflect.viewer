using System;
#if (UNITY_EDITOR)
using UnityEditor;
#endif
using UnityEngine;

namespace UTJ.FrameCapturer
{
    #if (UNITY_EDITOR)
    [CustomEditor(typeof(AudioRecorder))]
    public class AudioRecorderEditor : RecorderBaseEditor
    {
        public override void OnInspectorGUI()
        {
            var so = serializedObject;

            CommonConfig();
            EditorGUILayout.Space();
            FramerateControl();
            EditorGUILayout.Space();
            RecordingControl();

            so.ApplyModifiedProperties();
        }
    }
    #endif
}

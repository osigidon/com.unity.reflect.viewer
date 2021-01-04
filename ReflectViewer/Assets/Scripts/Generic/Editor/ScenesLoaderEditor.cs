using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
namespace CivilFX.Generic2
{
    [CustomEditor(typeof(ScenesLoader))]
    public class ScenesLoaderEditor : Editor
    {
        private SerializedObject so;
        private ScenesLoader _target;

        private void OnEnable()
        {
            so = serializedObject;
            _target = (ScenesLoader)target;
        }

        public override void OnInspectorGUI()
        {
            so.Update();

            //script name
            using (new EditorGUI.DisabledScope(true)) {
                EditorGUILayout.PropertyField(so.FindProperty("m_Script"));
            }

            EditorGUILayout.LabelField("Step-by-step Scenes:", EditorStyles.boldLabel);
            //step by step scenes
            var sp = so.FindProperty("stepScenes");
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(sp, new GUIContent($"{sp.name} ({sp.arraySize})"), false, GUILayout.MaxWidth(200));
            if (GUILayout.Button("+", GUILayout.MaxWidth(50))) {
                sp.arraySize++;
            }
            EditorGUILayout.EndHorizontal();
            //info
            EditorGUILayout.HelpBox("These scenes will be loaded first - in order", MessageType.Info);

            //show child
            if (sp.isExpanded) {
                int index = 0;
                EditorGUI.indentLevel++;
                var enumerator = sp.GetEnumerator();
                while (enumerator.MoveNext()) {
                    var csp = enumerator.Current as SerializedProperty;
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(csp);
                    if (GUILayout.Button("-", GUILayout.MaxWidth(50))) {
                        sp.MoveArrayElement(index, sp.arraySize - 1);
                        sp.arraySize--;
                    }
                    EditorGUILayout.EndHorizontal();
                    index++;
                }
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Concurrence Scenes:", EditorStyles.boldLabel);
            //concurrence scenes
            sp = so.FindProperty("concurrenceScenes");
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(sp, new GUIContent($"{sp.name} ({sp.arraySize})"), false, GUILayout.MaxWidth(200));
            if (GUILayout.Button("+", GUILayout.MaxWidth(50))) {
                sp.arraySize++;
            }
            EditorGUILayout.EndHorizontal();
            //info
            EditorGUILayout.HelpBox("These scenes will be loaded second - at the same time", MessageType.Info);

            //show child
            if (sp.isExpanded) {
                int index = 0;
                EditorGUI.indentLevel++;
                var enumerator = sp.GetEnumerator();
                while (enumerator.MoveNext()) {
                    var csp = enumerator.Current as SerializedProperty;
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(csp);
                    if (GUILayout.Button("-", GUILayout.MaxWidth(50))) {
                        sp.MoveArrayElement(index, sp.arraySize - 1);
                        sp.arraySize--;
                    }
                    EditorGUILayout.EndHorizontal();
                    index++;
                }
                EditorGUI.indentLevel--;
            }



            EditorGUILayout.Space();
            EditorGUILayout.LabelField("UI Setup:", EditorStyles.boldLabel);
            //bar
            sp = so.FindProperty("barProgress");
            EditorGUILayout.PropertyField(sp);
            //circular
            sp = so.FindProperty("circularProgress");
            EditorGUILayout.PropertyField(sp);
            //text
            sp = so.FindProperty("textProgress");
            EditorGUILayout.PropertyField(sp);

            MakeCopy("stepScenes", "stepSceneNames");
            MakeCopy("concurrenceScenes", "concurrenceSceneNames");

            so.ApplyModifiedProperties();
        }

        private void MakeCopy(string from, string to)
        {
            var scenesProp = so.FindProperty(from);
            var sceneNamesProp = so.FindProperty(to);

            sceneNamesProp.ClearArray();
            sceneNamesProp.arraySize = scenesProp.arraySize;

            for (int i = 0; i < scenesProp.arraySize; i++) {
                var cp = scenesProp.GetArrayElementAtIndex(i);
                var sceneAsset = cp.objectReferenceValue as SceneAsset;
                if (sceneAsset != null) {
                    sceneNamesProp.GetArrayElementAtIndex(i).stringValue = sceneAsset.name;
                }
            }
        }

    }
}
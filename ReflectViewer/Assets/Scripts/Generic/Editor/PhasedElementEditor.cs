using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CivilFX.Generic2
{
    [CustomEditor(typeof(PhasedElement))]
    public class PhasedElementEditor : Editor
    {
        SerializedObject so;
        PhasedElement _target;
        private void OnEnable()
        {
            so = serializedObject;
            _target = (PhasedElement)target;
        }

        public override void OnInspectorGUI()
        {
            so.Update();
            //script name
            using (new EditorGUI.DisabledScope(true)) {
                EditorGUILayout.PropertyField(so.FindProperty("m_Script"));
            }

            //phase types
            var sp = so.FindProperty("phasedTypes");
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(sp, new GUIContent($"{sp.name} ({sp.arraySize})"), false, GUILayout.MaxWidth(200));
            if (GUILayout.Button("+", GUILayout.MaxWidth(50))) {
                sp.arraySize++;
            }
            EditorGUILayout.EndHorizontal();
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

            //reverse
            sp = so.FindProperty("reverse");
            EditorGUILayout.PropertyField(sp, new GUIContent("Reverse","If enable, will hide on selected phases, and show all on un-selected phases"));

            so.ApplyModifiedProperties();
        }



    }
}
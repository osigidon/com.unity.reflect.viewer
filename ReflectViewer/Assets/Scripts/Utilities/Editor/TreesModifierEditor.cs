using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if (UNITY_EDITOR)
using UnityEditor;
#endif

namespace CivilFX
{
    #if (UNITY_EDITOR)
    [CanEditMultipleObjects]
    [CustomEditor(typeof(TreesModifier))]
    public class TreesModifierEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();


            serializedObject.Update();


            if (GUILayout.Button("Apply"))
            {
                for (int i=0; i<Selection.gameObjects.Length; i++)
                {
                    Undo.RecordObject(target, "Apply Scaling Effect");
                    Selection.gameObjects[i].GetComponent<TreesModifier>().Apply();
                }
            }


            serializedObject.ApplyModifiedProperties();

        }
    }
    #endif
}

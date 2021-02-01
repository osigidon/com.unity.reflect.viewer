
#if (UNITY_EDITOR)
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEditor;
using UnityEngine;
using static CivilFX.UI2.NavPanelController;

namespace CivilFX.Generic2
{

    [CustomEditor(typeof(CameraNodeManager))]
    public class CameraNodeManagerEditor : Editor
    {
        private SerializedObject so;
        private CameraNodeManager _target;

        private void OnEnable()
        {
            so = serializedObject;
            _target = (CameraNodeManager)target;
        }

        public override void OnInspectorGUI()
        {
            so.Update();

            //script name
            using (new EditorGUI.DisabledScope(true)) {
                EditorGUILayout.PropertyField(so.FindProperty("m_Script"));
            }

            //create button
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.Space();
            if (GUILayout.Button("Create", GUILayout.MaxWidth(100))) {
                if (SceneView.lastActiveSceneView != null) {
                    var pos = SceneView.lastActiveSceneView.camera.transform.position;
                    var rot = SceneView.lastActiveSceneView.camera.transform.rotation;
                    var currentOrder = _target.gameObject.transform.childCount;
                    GameObject dummy = new GameObject("New Camera Node");
                    dummy.transform.position = pos;
                    dummy.transform.rotation = rot;
                    dummy.transform.SetParent(_target.gameObject.transform);
                    var cameraNode = dummy.AddComponent<CameraNode>();
                    var cameraNodeSO = new SerializedObject(cameraNode);
                    cameraNodeSO.Update();
                    cameraNodeSO.FindProperty("order").intValue = currentOrder++;
                    cameraNodeSO.ApplyModifiedProperties();
                    Undo.RegisterCreatedObjectUndo(dummy, "CreateCameraNode");
                }
            }
            EditorGUILayout.Space();
            EditorGUILayout.EndHorizontal();

            //load button
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.Space();
            if (GUILayout.Button("Load External", GUILayout.MaxWidth(100))) {
                string path = EditorUtility.OpenFilePanel("Load Camera Nodes", "", "*.*");
                if (!string.IsNullOrEmpty(path)) {
                    Extract(path);
                }
            }
            EditorGUILayout.Space();
            EditorGUILayout.EndHorizontal();

            //reorder button
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.Space();
            if (GUILayout.Button(new GUIContent("Reorder", "Nodes will be reordered based on their positions in the hierarchy!"), GUILayout.MaxWidth(100))) {
                var currentOrder = 0;
                var nodes = _target.gameObject.GetComponentsInChildren<CameraNode>();
                if (nodes != null && nodes.Length > 0) {
                    foreach (var node in nodes) {
                        var nodeSO = new SerializedObject(node);
                        nodeSO.Update();
                        nodeSO.FindProperty("order").intValue = currentOrder++;
                        nodeSO.ApplyModifiedProperties();
                    }
                }
            }
            EditorGUILayout.Space();
            EditorGUILayout.EndHorizontal();

            so.ApplyModifiedProperties();
        }
        private void Extract(string path)
        {
            var currentOrder = _target.gameObject.transform.childCount;
            var fstream = File.Open(path, FileMode.Open);
            var bin = new BinaryFormatter();
            var nodes = (StillNodeHandler)bin.Deserialize(fstream);
            fstream.Close();
            if (nodes != null && nodes.Count > 0) {
                for (int i = 0; i < nodes.Count; i++) {
                    GameObject dummy = new GameObject(nodes[i].cameraName);
                    dummy.transform.position = nodes[i].position;
                    dummy.transform.eulerAngles = nodes[i].rotation;
                    dummy.transform.SetParent(_target.gameObject.transform);
                    var cameraNode = dummy.AddComponent<CameraNode>();
                    var cameraNodeSO = new SerializedObject(cameraNode);
                    cameraNodeSO.Update();
                    cameraNodeSO.FindProperty("order").intValue = currentOrder++;
                    cameraNodeSO.FindProperty("fov").boolValue = true;
                    cameraNodeSO.FindProperty("fovValue").floatValue = nodes[i].fov;
                    cameraNodeSO.ApplyModifiedProperties();
                    Undo.RegisterCreatedObjectUndo(dummy, "CreateCameraNode");
                }
                if (EditorUtility.DisplayDialog("Load Node From File", $"Created: {nodes.Count} node(s). Delete config file?", "Yes", "No")) {
                    File.Delete(path);
                }
            }
        }
    }
}

#endif
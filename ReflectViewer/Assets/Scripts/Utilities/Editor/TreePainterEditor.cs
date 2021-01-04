using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;

namespace CivilFX
{

    [CustomEditor(typeof(TreePainter))]
    public class TreePainterEditor : Editor
    {
        private TreePainter _target;
        private SerializedObject so;
        private float timeSinceLastPaint;
        private Vector3 lastHitPoint;
        private float lastHitDistance;

        private GameObject cursorObject;
        private void OnEnable()
        {
            so = serializedObject;
            _target = (TreePainter)target;
            timeSinceLastPaint = Time.realtimeSinceStartup;
        }

        private void OnDisable()
        {
            if (cursorObject != null) {
                DestroyImmediate(cursorObject);
            }
        }

        public override void OnInspectorGUI()
        {
            so.Update();
            //script name
            var currentProperty = so.FindProperty("m_Script");
            using (new EditorGUI.DisabledScope(true)) {
                EditorGUILayout.PropertyField(currentProperty);
            }

            SerializedProperty currentProp;

            //parent object
            currentProp = so.FindProperty("parentObj");
            EditorGUILayout.PropertyField(currentProp);

            //prefabs
            currentProp = so.FindProperty("prefabs");
            EditorGUILayout.PropertyField(currentProp, true);

            //interval
            currentProp = so.FindProperty("paintInterval");
            EditorGUILayout.PropertyField(currentProp);

            //layer
            currentProp = so.FindProperty("layer");
            EditorGUILayout.PropertyField(currentProp);

            //mode
            currentProp = so.FindProperty("mode");
            EditorGUILayout.PropertyField(currentProp);
            var paintMode = (TreePainter.PaintMode)currentProp.enumValueIndex;
            if (paintMode == TreePainter.PaintMode.Group) {
                //groupScale
                currentProp = so.FindProperty("groupScale");
                EditorGUILayout.PropertyField(currentProp);
                //groupCount
                currentProp = so.FindProperty("groupCount");
                EditorGUILayout.PropertyField(currentProp);
            }

            //rotation
            currentProp = so.FindProperty("applyRandomRotation");
            EditorGUILayout.PropertyField(currentProp);
            bool randomRotation = currentProp.boolValue;
            EditorGUI.indentLevel++;
            using (new EditorGUI.DisabledScope(randomRotation)) {
                currentProp = so.FindProperty("angle");
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(currentProp);
                if (GUILayout.Button("Get")) {
                    currentProp.vector3Value = cursorObject.transform.eulerAngles;
                }
                EditorGUILayout.EndHorizontal();
            }
            using (new EditorGUI.DisabledScope(!randomRotation)) {
                currentProp = so.FindProperty("axis");
                EditorGUILayout.PropertyField(currentProp);
            }
            EditorGUI.indentLevel--;
            //scale
            currentProp = so.FindProperty("applyRandomScale");
            EditorGUILayout.PropertyField(currentProp);
            EditorGUI.indentLevel++;
            using (new EditorGUI.DisabledScope(!currentProp.boolValue)) {
                currentProp = so.FindProperty("minScale");
                EditorGUILayout.PropertyField(currentProp);
                currentProp = so.FindProperty("maxScale");
                EditorGUILayout.PropertyField(currentProp);
            }
            EditorGUI.indentLevel--;

            //clear ghost button
            if (GUILayout.Button("Clear Cursor Object")) {
                if (cursorObject != null) {
                    DestroyImmediate(cursorObject);
                }
            }

            //info
            EditorGUILayout.HelpBox(
                "1)Add Mesh Collision to terrain.\n" +
                "2)Choose a layer for terrain.\n" +
                "3)Set prefabs on script.\n" +
                "4)Select matching layer on script with the terrain.\n\n" +
                "-Left click to place object.\n" +
                "-Scrollwheel to rotate object.",
                MessageType.Info);
            so.ApplyModifiedProperties();
        }

        private void OnSceneGUI()
        {
            Event e = Event.current;
            bool shouldConsumeEvent = false;

            //when mouse is moved, we attach a dummy object to the cursor
            //if the object is not successfully created (by pressing activation key)
            //or switching to group mode
            //  remove it
            if (e.type == EventType.MouseMove) {
                GetPoint(e.mousePosition);
                //create the object and attach it to the mouse cursor
                if (_target.mode == TreePainter.PaintMode.Individual && cursorObject == null) {
                    cursorObject = InstantiatePrefab();
                    if (cursorObject != null) {
                        ApplyTransform(cursorObject, lastHitPoint);
                    }
                } else {
                    cursorObject.transform.position = lastHitPoint;
                }
                HandleUtility.Repaint();
            }

            //rotate the cursor object when scrollwheel is used
            if (e.isScrollWheel && cursorObject != null) {
                shouldConsumeEvent = true;
                Vector3 newAngle = cursorObject.transform.eulerAngles;
                switch (_target.axis) {
                    case TreePainter.RotationAxis.X:
                        newAngle.x += e.delta.y;
                        break;
                    case TreePainter.RotationAxis.Y:
                        newAngle.y += e.delta.y;
                        break;
                    case TreePainter.RotationAxis.Z:
                        newAngle.z += e.delta.y;
                        break;
                }
                cursorObject.transform.eulerAngles = newAngle;
            }

            //for group painting
            if (_target.mode == TreePainter.PaintMode.Group) {
                Handles.DrawWireDisc(lastHitPoint, new Vector3(0, 1, 0), _target.groupScale);
            }

            //actually commit the creating object
            if (e.type == EventType.MouseUp && e.button == 0) {
                float currentTime = Time.realtimeSinceStartup;
                if (currentTime - timeSinceLastPaint >= _target.paintInterval) {                  
                    PermanentlyCreateObject(e.mousePosition);
                    timeSinceLastPaint = currentTime;
                }
            }
            
            //consume mouse click
            //so selection is still stayed on the painting object
            if (e.type == EventType.Layout) {
                HandleUtility.AddDefaultControl(0);
            }
            
            //consume all other keys
            if (shouldConsumeEvent) {
                e.Use();
            }
        }

        private bool GetPoint(Vector3 mousePos)
        {
            Ray r = HandleUtility.GUIPointToWorldRay(mousePos);
            if (Physics.Raycast(r, out RaycastHit hitInfo, float.MaxValue, _target.layer.value)) {
                lastHitPoint = hitInfo.point;
                lastHitDistance = hitInfo.distance;
                return true;
            }
            return false;
        }

        private void PermanentlyCreateObject(Vector2 mousePos)
        {
            if (_target.prefabs == null || _target.prefabs.Length == 0) {
                Debug.Log("Prefab list is empty!!!");
                return;
            }

            if (_target.mode == TreePainter.PaintMode.Individual) {
                //set parent
                if (_target.parentObj != null) {
                    cursorObject.transform.SetParent(_target.parentObj);
                }
                Undo.RegisterCreatedObjectUndo(cursorObject, "Tree Paint Undo");
                //done with this cursor object
                //request to get new one
                cursorObject = null;
            } else if (_target.mode == TreePainter.PaintMode.Group) {
                Vector3 lastPoint = lastHitPoint;
                for (int i = 0; i < _target.groupCount; i++) {
                    Vector2 offset = Vector2.one;
                    offset.x = Random.Range(-_target.groupScale * _target.groupScale, _target.groupScale * _target.groupScale) / (Vector3.Distance(Camera.current.transform.position, lastHitPoint) / 200.0f);
                    offset.y = Random.Range(-_target.groupScale * _target.groupScale, _target.groupScale * _target.groupScale) / (Vector3.Distance(Camera.current.transform.position, lastHitPoint) / 200.0f);
                    if (GetPoint(mousePos + offset)) {
                        GameObject obj = InstantiatePrefab(true);
                        //set parent
                        if (_target.parentObj != null) {
                            obj.transform.SetParent(_target.parentObj);
                        }
                        ApplyTransform(obj, lastHitPoint);
                    }
                }
                lastHitPoint = lastPoint;
            }
        }

        private GameObject InstantiatePrefab(bool isBelongedToGroup=false)
        {
            if (_target.prefabs == null || _target.prefabs.Length == 0) {
                return null;
            }
            Scene scene = _target.gameObject.scene;
            var prefab = _target.prefabs[Random.Range(0, _target.prefabs.Length - 1)];
            GameObject obj = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
            if (isBelongedToGroup) {
                Undo.RegisterCreatedObjectUndo(obj, "Tree Paint Undo");
            }
            return obj;
        }

        private void ApplyTransform(GameObject go, Vector3 pos)
        {
            go.transform.position = pos;

            //apply rotation
            if (_target.applyRandomRotation) {
                Vector3 rot = Vector3.zero;
                float rand = Random.Range(0.0f, 360.0f);
                switch (_target.axis) {
                    case TreePainter.RotationAxis.X:
                        rot.x = rand;
                        break;
                    case TreePainter.RotationAxis.Y:
                        rot.y = rand;
                        break;
                    case TreePainter.RotationAxis.Z:
                        rot.z = rand;
                        break;
                }
                go.transform.localEulerAngles = rot;
            } else {
                go.transform.localEulerAngles = _target.angle;
            }

            //apply scale
            if (_target.applyRandomScale) {
                go.transform.localScale = Vector3.one * Random.Range(_target.minScale, _target.maxScale);
            }

        }
    }
}
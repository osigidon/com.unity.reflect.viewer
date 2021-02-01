#if (UNITY_EDITOR)
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace CivilFX.TrafficV5
{

    /* helper class to enable drawing picker icon in viewport
    * used by:  IntersectionControllerEditor,
    *           ConflictZoneControllerEditor,
    *           TrafficPathControllerEditor
    *  Make sure to call ClearState() at the begining of each editor as data
    *  could still persit from last used.
    */
    public static class CursorStateUtility
    {
        public static bool selected;
        private static int[] triggerIndices = new int[5] { -1, -1, -1, -1, -1 };

        public static void SetState(bool state, params int[] args)
        {
            selected = state;
            for (int i=0; i<args.Length; ++i) {
                triggerIndices[i] = args[i];
            }
        }

        public static bool GetCursorState()
        {
            return selected;
        }
        public static int GetTriggerIndex(int polledIndex)
        {
            return triggerIndices[polledIndex];
        }


        //comparing trigger values in order
        public static bool CompareTriggerValues(params int[] args)
        {
            if (args.Length > triggerIndices.Length) {
                return false;
            }
            for (int i=0; i<args.Length; i++) {
                if (triggerIndices[i] != args[i]) {
                    return false;
                }
            }
            return true;
        }

        public static void ClearState()
        {
            selected = false;
            for (int i = 0; i < triggerIndices.Length; i++) {
                triggerIndices[i] = -1;
            }
        }

        public static void Debug()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append($"State: {selected}");
            foreach (int value in triggerIndices) {
                sb.Append($" [{value}]");
            }
            UnityEngine.Debug.Log(sb);
        }
    }

    public static class PathPickerEditor
    {
        public static TrafficPathController PickPath(string sceneName, Object self)
        {
            TrafficPathController result = null;

            Event e = Event.current;
            var mouseImage = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/Scripts/Traffic/Editor/PickerImage2D.png", typeof(Texture2D));
            Cursor.SetCursor(mouseImage, new Vector2(0, 31), CursorMode.Auto);
            EditorGUIUtility.AddCursorRect(new Rect(0, 0, Screen.width, Screen.height), MouseCursor.CustomCursor);

            //find all path
            var pathControllers = Resources.FindObjectsOfTypeAll<TrafficPathController>();
            var tempPathControllers = new List<TrafficPathController>(pathControllers.Length);
            foreach (var pathController in pathControllers) {
                if (pathController.gameObject.scene.name.Equals(sceneName)) {
                    tempPathControllers.Add(pathController);
                }
            }

            //add mesh to path
            foreach (var pathController in tempPathControllers) {
                pathController.AddMeshCollider();
            }

            //check collider for selected path
            Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);

            Color drawingColor = Color.white;
            Vector3 drawingPoint = Vector3.zero;
            RaycastHit[] hits = Physics.RaycastAll(ray, 10000f);
            foreach (RaycastHit hit in hits) {
                TrafficPathController pathController = hit.transform.gameObject.GetComponent<TrafficPathController>();
                if (pathController != null && pathController != self) {
                    drawingColor = Color.green;
                    result = pathController;
                }
                drawingPoint = hit.point;
            }
            Handles.color = drawingColor;
            Handles.DrawSolidDisc(drawingPoint, Vector3.up, 1f);

            //consume clicking event
            if (e.type == EventType.Layout) {
                HandleUtility.AddDefaultControl(0);
            }

            //only save the result if the left mouse button is clicked
            //otherwise set it to null
            if (e.type == EventType.MouseUp && e.button == 0) {
                //reset cursor
                Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
                e.Use();
            } else {
                result = null;
            }

            //remove mesh from path
            foreach (var pathController in tempPathControllers) {
                pathController.RemoveMeshCollider();
            }

            return result;
        }

        public static GUIContent LoadWidgetIcon()
        {
            return new GUIContent((Texture)AssetDatabase.LoadAssetAtPath("Assets/Scripts/Traffic/Editor/PickerImage.png", typeof(Texture)), "Pick Path");
        }
    }
}
#endif
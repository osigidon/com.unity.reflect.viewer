using CivilFX.Generic2;
using CivilFX.TrafficV5;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace CivilFX.UI2
{



    [CustomEditor(typeof(AnimatedCameraPath))]
    public class AnimatedCameraPathEditor : Editor
    {
        private SerializedObject so;
        private AnimatedCameraPath _target;
        private bool saveCameraPosition;
        private Vector3 cameraPosition;
        private Quaternion cameraRotation;
        private Transform previewCameraTrans;
        private static CamerasPreview preview;

        private void OnEnable()
        {
            so = serializedObject;
            _target = (AnimatedCameraPath)target;
            Tools.hidden = true;
            saveCameraPosition = true;
            
            if (so.FindProperty("asset").objectReferenceValue != null) {
                UpdateAsset(new SerializedObject(so.FindProperty("asset").objectReferenceValue), so);
            } else {
                so.Update();
                so.FindProperty("positions").ClearArray();
                so.FindProperty("rotations").ClearArray();
                so.FindProperty("linkDatas").ClearArray();
                so.ApplyModifiedProperties();
            }
            
        }

        private void UpdateAsset(SerializedObject source, SerializedObject dest)
        {
            //Debug.Log("Updateing Asset");
            dest.Update();
            var destPositionsProp = dest.FindProperty("positions");
            var destRotationsProp = dest.FindProperty("rotations");
            var destLinkDataProp = dest.FindProperty("linkDatas");
            destPositionsProp.ClearArray();
            destRotationsProp.ClearArray();

            source.Update();
            var sourcePositionProp = source.FindProperty("positions");
            var sourceRotationProp = source.FindProperty("rotations");
            var sourceLinkDataProp = source.FindProperty("linkDatas");

            destPositionsProp.arraySize = sourcePositionProp.arraySize;
            destRotationsProp.arraySize = sourceRotationProp.arraySize;
            destLinkDataProp.arraySize = sourceLinkDataProp.arraySize;

            for (int i = 0; i < sourcePositionProp.arraySize; i++) {
                destPositionsProp.GetArrayElementAtIndex(i).vector3Value = sourcePositionProp.GetArrayElementAtIndex(i).vector3Value;
                destRotationsProp.GetArrayElementAtIndex(i).quaternionValue = sourceRotationProp.GetArrayElementAtIndex(i).quaternionValue;
                var sourceLink = sourceLinkDataProp.GetArrayElementAtIndex(i);
                var destLink = destLinkDataProp.GetArrayElementAtIndex(i);
                var data = PropertyToLinkData(sourceLink);
                LinkDataToProperty(data, destLink);
            }
            source.ApplyModifiedProperties();
            dest.ApplyModifiedProperties();
        }

        public override void OnInspectorGUI()
        {
            so.Update();
            //script name
            var currentProperty = so.FindProperty("m_Script");
            using (new EditorGUI.DisabledScope(true)) {
                EditorGUILayout.PropertyField(currentProperty);
            }

            //preview camera
            currentProperty = so.FindProperty("previewCamera");
            EditorGUILayout.PropertyField(currentProperty);
            if (currentProperty.objectReferenceValue == null) {
                EditorGUILayout.HelpBox("Need to assign preview camera", MessageType.Error);
            } else {
                previewCameraTrans = (currentProperty.objectReferenceValue as Camera).transform;
            }
 
            //asset to edit
            currentProperty = so.FindProperty("asset");
            var oldRef = currentProperty.objectReferenceValue;
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(currentProperty);
            if (EditorGUI.EndChangeCheck()) {
                var newRef = currentProperty.objectReferenceValue;
                if (oldRef != null) {
                    //copy back
                    so.ApplyModifiedProperties();
                    UpdateAsset(so, new SerializedObject(oldRef));
                }
                if (newRef != null) {
                    so.ApplyModifiedProperties();
                    UpdateAsset(new SerializedObject(newRef), so);
                } else {
                    so.FindProperty("positions").ClearArray();
                    so.FindProperty("rotations").ClearArray();
                    so.FindProperty("linkDatas").ClearArray();
                }
            }

            var currentPositionsProperty = so.FindProperty("positions");
            var currentRotationsProperty = so.FindProperty("rotations");
            var currentLinkDataProperty = so.FindProperty("linkDatas");

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.Space();
            if (GUILayout.Button("Create", GUILayout.MaxWidth(100))) {
                currentPositionsProperty.ClearArray();
                currentRotationsProperty.ClearArray();
                currentLinkDataProperty.ClearArray();
                var folderPath = "Assets/CameraAssets";
                if (!AssetDatabase.IsValidFolder(folderPath)) {
                    AssetDatabase.CreateFolder("Assets", "CameraAssets");
                }
                var ins = ScriptableObject.CreateInstance<AnimatedCameraPathData>();
                currentProperty.objectReferenceValue = ins.SaveAssetToDisk(folderPath, "New Camera");
            }
            if (GUILayout.Button("Load", GUILayout.MaxWidth(100))) {

            }
            if (GUILayout.Button("Load External", GUILayout.MaxWidth(100))) {

            }
            EditorGUILayout.Space();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            //draw positions
            GUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(currentPositionsProperty, new GUIContent(""), false, GUILayout.MaxWidth(1));
            EditorGUILayout.LabelField("Nodes:", EditorStyles.boldLabel, GUILayout.MaxWidth(100));
            if (GUILayout.Button("Clear", GUILayout.MaxWidth(100))) {
                if (EditorUtility.DisplayDialog("Clear Data", "Removing all data from asset. Proceed?", "Yes", "Cancel")) {
                    currentPositionsProperty.ClearArray();
                    currentRotationsProperty.ClearArray();
                    currentLinkDataProperty.ClearArray();
                }
            }
            GUILayout.EndHorizontal();
            if (currentPositionsProperty.isExpanded) {
                EditorGUI.indentLevel++;
                for (int i = 0; i < currentPositionsProperty.arraySize; i++) {
                    GUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(i.ToString(), GUILayout.MaxWidth(50));
                    SerializedProperty nodeProp = currentPositionsProperty.GetArrayElementAtIndex(i);
                    EditorGUILayout.PropertyField(nodeProp, new GUIContent(""));
                    //delete button
                    if (GUILayout.Button(new GUIContent("X", "Delete this node"), GUILayout.MaxWidth(50))) {
                        currentPositionsProperty.MoveArrayElement(i, currentPositionsProperty.arraySize - 1);
                        currentPositionsProperty.arraySize--;
                        currentRotationsProperty.MoveArrayElement(i, currentRotationsProperty.arraySize - 1);
                        currentRotationsProperty.arraySize--;
                        currentLinkDataProperty.MoveArrayElement(i, currentLinkDataProperty.arraySize - 1);
                        currentLinkDataProperty.arraySize--;
                        so.ApplyModifiedProperties();
                        return;
                    }
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    EditorGUILayout.Space();
                    EditorGUIUtility.labelWidth = 110;
                    if (i != currentPositionsProperty.arraySize - 1) {
                        var linkProp = currentLinkDataProperty.GetArrayElementAtIndex(i);
                        var linkEnumerator = linkProp.GetEnumerator();
                        while (linkEnumerator.MoveNext()) {
                            var linkCurr = linkEnumerator.Current as SerializedProperty;
                            var name = linkCurr.name;
                            if (name.Equals("translateType") || name.Equals("translateSpeed") || name.Equals("rotationType")) {
                                EditorGUILayout.PropertyField(linkCurr);
                            }
                        }
                    }
                    EditorGUILayout.Space();
                    EditorGUIUtility.labelWidth = 0;
                    GUILayout.EndHorizontal();
                }
                EditorGUI.indentLevel--;
            }

            //**
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Capture mode:", EditorStyles.boldLabel, GUILayout.MaxWidth(100));
            currentProperty = so.FindProperty("mode");
            EditorGUILayout.PropertyField(currentProperty);
            var selection = currentProperty.enumValueIndex;
            switch (selection) {
                case 0: {
                    //Orbit
                    var transProp = so.FindProperty("originTrans");
                    var heightProp = so.FindProperty("height");
                    var radiusProp = so.FindProperty("radius");
                    EditorGUILayout.PropertyField(radiusProp);
                    EditorGUILayout.PropertyField(heightProp);
                    EditorGUILayout.PropertyField(transProp);

                    if (transProp.objectReferenceValue != null) {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.Space();
                        if (GUILayout.Button("Generate", GUILayout.MaxWidth(100))) {
                            var origin = (transProp.objectReferenceValue as Transform).position;
                            var positions = GetEclipse(heightProp.floatValue, heightProp.floatValue, origin, heightProp.floatValue, 0, 12);
                            currentPositionsProperty.ClearArray();
                            currentRotationsProperty.ClearArray();
                            currentLinkDataProperty.ClearArray();

                            currentPositionsProperty.arraySize = positions.Length;
                            currentRotationsProperty.arraySize = positions.Length;
                            currentLinkDataProperty.arraySize = positions.Length;
                            for (int i = 0; i < positions.Length; i++) {
                                var rotation = Quaternion.LookRotation(origin - positions[i], Vector3.up);
                                currentPositionsProperty.GetArrayElementAtIndex(i).vector3Value = positions[i];
                                currentRotationsProperty.GetArrayElementAtIndex(i).quaternionValue = rotation;
                            }
                        }
                        EditorGUILayout.Space();
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.Space();
                        EditorGUILayout.Space();
                    } else {
                        EditorGUILayout.HelpBox("Assign transform", MessageType.Error);
                    }
                        break;               
                }
                case 1: {
                    //Eclipse
                    var xRadiusProp = so.FindProperty("xRadius");
                    var zRadiusProp = so.FindProperty("zRadius");
                    var transProp = so.FindProperty("originTrans");
                    var heightProp = so.FindProperty("height");

                    EditorGUILayout.PropertyField(xRadiusProp);
                    EditorGUILayout.PropertyField(zRadiusProp);
                    EditorGUILayout.PropertyField(heightProp);
                    EditorGUILayout.PropertyField(transProp);
                    var theta = 0f;
                    var resolution = 12;
                    if (transProp.objectReferenceValue != null) {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.Space();
                        if (GUILayout.Button("Generate", GUILayout.MaxWidth(100))) {
                            var origin = (transProp.objectReferenceValue as Transform).position;
                            var positions = GetEclipse(xRadiusProp.floatValue, zRadiusProp.floatValue, origin, heightProp.floatValue, theta, resolution);
                            currentPositionsProperty.ClearArray();
                            currentRotationsProperty.ClearArray();
                            currentLinkDataProperty.ClearArray();

                            currentPositionsProperty.arraySize = positions.Length;
                            currentRotationsProperty.arraySize = positions.Length;
                            currentLinkDataProperty.arraySize = positions.Length;
                            for (int i = 0; i < positions.Length; i++) {
                                var rotation = Quaternion.LookRotation(origin - positions[i], Vector3.up);
                                currentPositionsProperty.GetArrayElementAtIndex(i).vector3Value = positions[i];
                                currentRotationsProperty.GetArrayElementAtIndex(i).quaternionValue = rotation;
                            }
                        }
                        EditorGUILayout.Space();
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.Space();
                        EditorGUILayout.Space();
                    } else {
                        EditorGUILayout.HelpBox("Assign transform", MessageType.Error);
                    }
                    break;

                }
                case 2: {
                    //FollowPath
                    var pathProp = so.FindProperty("path");
                    var xTiltAngleProp = so.FindProperty("xTiltAngle");
                    var heightProp = so.FindProperty("height");
                    EditorGUILayout.PropertyField(xTiltAngleProp);
                    EditorGUILayout.PropertyField(heightProp);
                    EditorGUILayout.PropertyField(pathProp);
                    if (pathProp.objectReferenceValue != null) {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.Space();
                        if (GUILayout.Button("Generate", GUILayout.MaxWidth(100))) {

                            var positions = new List<Vector3>((pathProp.objectReferenceValue as TrafficPath).nodes);
                            currentPositionsProperty.ClearArray();
                            currentRotationsProperty.ClearArray();
                            currentLinkDataProperty.ClearArray();

                            currentPositionsProperty.arraySize = positions.Count - 2;
                            currentRotationsProperty.arraySize = positions.Count - 2;
                            currentLinkDataProperty.arraySize = positions.Count - 2;
                            var index = 0;
                            for (int i = 1; i < positions.Count-1; i++) {
                                var pos = positions[i];
                                pos.y += heightProp.floatValue;
                                var rotation = Quaternion.LookRotation(positions[i+1] - positions[i], Vector3.up);
                                var euler = rotation.eulerAngles;
                                euler.x += xTiltAngleProp.floatValue;
                                rotation = Quaternion.Euler(euler);
                                currentPositionsProperty.GetArrayElementAtIndex(index).vector3Value = pos;
                                currentRotationsProperty.GetArrayElementAtIndex(index++).quaternionValue = rotation;
                            }
                        }
                        EditorGUILayout.Space();
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.Space();
                        EditorGUILayout.Space();
                        
                    } else {
                        EditorGUILayout.HelpBox("Assign path", MessageType.Error);
                    }

                    break;
                }
                case 3: {
                    //Manual
                    if (currentPositionsProperty.arraySize == 0) {
                        EditorGUILayout.HelpBox("Move scene camera to view and click \"Capture\"", MessageType.Info);
                    }
                    //snap button
                    EditorGUILayout.Space();
                    EditorGUILayout.Space();
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.Space();
                    if (GUILayout.Button("Capture", GUILayout.MaxWidth(100), GUILayout.MaxHeight(50))) {
                        if (SceneView.lastActiveSceneView != null && previewCameraTrans != null) {
                            if (saveCameraPosition) {
                                cameraPosition = previewCameraTrans.position;
                                cameraRotation = previewCameraTrans.rotation;
                                saveCameraPosition = false;
                            }
                            var pos = SceneView.lastActiveSceneView.camera.transform.position;
                            var rot = SceneView.lastActiveSceneView.camera.transform.rotation;
                            previewCameraTrans.position = pos;
                            previewCameraTrans.rotation = rot;
                            currentPositionsProperty.arraySize++;
                            currentPositionsProperty.GetArrayElementAtIndex(currentPositionsProperty.arraySize - 1).vector3Value = pos;
                            currentRotationsProperty.arraySize++;
                            currentRotationsProperty.GetArrayElementAtIndex(currentRotationsProperty.arraySize - 1).quaternionValue = rot;
                            currentLinkDataProperty.arraySize++;
                        }
                    }
                    EditorGUILayout.Space();
                    EditorGUILayout.EndHorizontal();
                    break;
                }
            }



            //**

            //preview button
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.Space();
            if (GUILayout.Button("Preview", GUILayout.MaxWidth(80), GUILayout.MaxHeight(30))) {
                if (saveCameraPosition) {
                    cameraPosition = previewCameraTrans.position;
                    cameraRotation = previewCameraTrans.rotation;
                    saveCameraPosition = false;
                }


                preview = new CamerasPreview(so.FindProperty("asset").objectReferenceValue as AnimatedCameraPathData, previewCameraTrans);
                EditorApplication.update += EditorUpdateCallback;
            }
            EditorGUILayout.Space();
            EditorGUILayout.EndHorizontal();

            so.ApplyModifiedProperties();
        }

        private void EditorUpdateCallback()
        {
            var done = preview.Update(Time.deltaTime);
            if (done) {
                //remove callback
                EditorApplication.update -= EditorUpdateCallback;
            }
        }

        private void OnSceneGUI()
        {
            //draw nodes handle
            if (_target.positions != null) {
                for (int i = 0; i < _target.positions.Count; i++) {
                    Vector3 currentPos = _target.positions[i];
                    //draw label
                    Handles.Label(currentPos, i.ToString(), Utilities.genericGUIStyle);
                    //draw handle
                    EditorGUI.BeginChangeCheck();
                    //Debug.Log(_target.rotations[i]);
                    Vector3 newPos = Handles.PositionHandle(currentPos, _target.rotations[i]);
                    if (EditorGUI.EndChangeCheck()) {
                        Undo.RecordObject(target, "PositionModified");
                        _target.positions[i] = newPos;
                    }
                    //draw rotation
                    EditorGUI.BeginChangeCheck();
                    var newRot = Handles.RotationHandle(_target.rotations[i], currentPos);
                    if (EditorGUI.EndChangeCheck()) {
                        Undo.RecordObject(target, "RotationModified");
                        _target.rotations[i] = newRot;
                    }
                }
            }

            //draw splines
            if (_target.positions != null && _target.positions.Count > 1) {
                List<Vector3> nodes = new List<Vector3>(_target.positions.Count + 2);
                var anchorBefore = _target.positions[0] + (_target.positions[0] - _target.positions[1]);
                var anchorAfter = _target.positions[_target.positions.Count - 1] + (_target.positions[_target.positions.Count - 1] - _target.positions[_target.positions.Count - 2]);
                nodes.Add(anchorBefore);
                nodes.AddRange(_target.positions);
                nodes.Add(anchorAfter);

                SplineBuilder spline = new SplineBuilder(nodes);
                var resolution = 50 * nodes.Count;
                var prevPoint = spline.GetPoint(0);
                for (int i = 0; i < resolution; i++) {
                    var t = (float)i / resolution;
                    var currentPoint = spline.GetPoint(t);
                    Handles.DrawLine(currentPoint, prevPoint);
                    prevPoint = currentPoint;
                }
            }



        }

        private void OnDisable()
        {
            Tools.hidden = false;
            if (so.FindProperty("asset").objectReferenceValue != null) {
                UpdateAsset(so, new SerializedObject(so.FindProperty("asset").objectReferenceValue));
            }
            EditorApplication.update -= EditorUpdateCallback;
            if (previewCameraTrans != null && !saveCameraPosition) {
                previewCameraTrans.position = cameraPosition;
                previewCameraTrans.rotation = cameraRotation;
            }
        }



        private LinkData PropertyToLinkData(SerializedProperty sp)
        {
            LinkData linkData = new LinkData();
            var enumerator = sp.GetEnumerator();
            while (enumerator.MoveNext()) {
                var curr = enumerator.Current as SerializedProperty;
                var name = curr.name;
                if (name.Equals("translateType")) {
                    linkData.translateType = (LinkData.TranslateType)curr.enumValueIndex;
                } else if (name.Equals("translateSpeed")) {
                    linkData.translateSpeed = curr.intValue;
                } else if (name.Equals("rotationType")) {
                    linkData.rotationType = (LinkData.RotationType)curr.enumValueIndex;
                } else if (name.Equals("constantRotation")) {
                    linkData.constantRotation = curr.quaternionValue;
                } else if (name.Equals("lookatTarget")) {
                    linkData.lookatTarget = curr.vector3Value;
                }
            }
            return linkData;
        }

        private void LinkDataToProperty(LinkData linkData, SerializedProperty sp)
        {
            var enumerator = sp.GetEnumerator();
            while (enumerator.MoveNext()) {
                var curr = enumerator.Current as SerializedProperty;
                var name = curr.name;
                if (name.Equals("translateType")) {
                    curr.enumValueIndex = (int)linkData.translateType;
                } else if (name.Equals("translateSpeed")) {
                    curr.intValue = linkData.translateSpeed;
                } else if (name.Equals("rotationType")) {
                    curr.enumValueIndex = (int)linkData.rotationType;
                } else if (name.Equals("constantRotation")) {
                    curr.quaternionValue = linkData.constantRotation;
                } else if (name.Equals("lookatTarget")) {
                    curr.vector3Value = linkData.lookatTarget;
                }
            }
        }

        public static void LinkPropertyToList(SerializedProperty linkSP, out List<int> speeds)
        {
            Debug.Log(linkSP.arraySize);
            Debug.Log(linkSP.propertyPath);
            speeds = new List<int>(linkSP.arraySize);
            var enumerator = linkSP.GetEnumerator();
            while (enumerator.MoveNext()) {
                var curr = enumerator.Current as SerializedProperty;

                var grandEnumerator = curr.GetEnumerator();
                while (grandEnumerator.MoveNext()) {
                    var childCurr = grandEnumerator.Current as SerializedProperty;
                    var name = childCurr.name;
                    if (name.Equals("translateSpeed")) {
                        speeds.Add(curr.intValue);
                    }
                }
            }
        }
        private Vector3[] GetEclipse(float xRadius, float zRadius, Vector3 origin, float height, float theta, int resolution)
        {
            Vector3[] positions = new Vector3[resolution + 1];
            Quaternion q = Quaternion.AngleAxis(theta, Vector3.forward);
            Vector3 center = origin;
            center.y += height;

            //horizonal
            for (int i = 0; i <= resolution; i++) {
                float angle = (float)i / (float)resolution * 2.0f * Mathf.PI;
                positions[i] = new Vector3(xRadius * Mathf.Cos(angle), 0.0f, zRadius * Mathf.Sin(angle));
                positions[i] = q * positions[i] + center;
            }

            return positions;
        }
    }


    public class CamerasPreview
    {

        private List<SplineBuilder> splines;
        private List<Quaternion> rotations;
        private List<int> movementSpeeds;
        private float currentProgress;
        private float progressSegment;
        private int currentIndex;
        private Transform cameraTrans;

        public CamerasPreview(AnimatedCameraPathData asset, Transform cam)
        {
            cameraTrans = cam;
            List<Vector3> positions = new List<Vector3>();

            var assetSO = new SerializedObject(asset);
            var positionsProp = assetSO.FindProperty("positions");
            for (int i=0; i<positionsProp.arraySize; i++) {
                positions.Add(positionsProp.GetArrayElementAtIndex(i).vector3Value);
            }
            splines = new List<SplineBuilder>();
            GetSplinesFromVector3(positions);
            Debug.Log(splines.Count);
            currentIndex = 0;
            currentProgress = 0;

            //rotations
            var rotationsProp = assetSO.FindProperty("rotations");
            rotations = new List<Quaternion>(rotationsProp.arraySize);
            for (int i=0; i<rotationsProp.arraySize; i++) {
                rotations.Add(rotationsProp.GetArrayElementAtIndex(i).quaternionValue);
            }

            //speed
            var linkSO = assetSO.FindProperty("linkDatas");
            AnimatedCameraPathEditor.LinkPropertyToList(linkSO, out movementSpeeds);
            progressSegment = splines[0].pathLength / movementSpeeds[0] * 0.44704f;
        }


        public bool Update(float dt)
        {
            if (currentProgress >= 1f) {
                currentIndex++;
                if (currentIndex >= splines.Count) {
                    return true;
                } else {
                    progressSegment = splines[currentIndex].pathLength / movementSpeeds[currentIndex] * 0.44704f;
                    currentProgress = 0;
                }
            }

            //translation
            cameraTrans.position = splines[currentIndex].GetPointOnPath(currentProgress);

            //rotation
            cameraTrans.rotation = Quaternion.Slerp(rotations[currentIndex], rotations[currentIndex + 1], currentProgress);

            currentProgress += dt / progressSegment;
            return false;
        }

        private void GetSplinesFromVector3(List<Vector3> positions)
        {
            for (int i=0; i<positions.Count-1; i++) {
                var anchorBefore = Vector3.zero;
                var anchorAfter = Vector3.zero;
                if (i == 0) {
                    anchorBefore = positions[0] + (positions[0] - positions[1]);
                } else {
                    anchorBefore = positions[i - 1];
                }

                if (i == positions.Count - 2) {
                    anchorAfter = positions[positions.Count - 1] + (positions[positions.Count - 1] - positions[positions.Count - 2]);
                } else {
                    anchorAfter = positions[i + 2];
                }
                List<Vector3> nodes = new List<Vector3>(4);
                nodes.Add(anchorBefore);
                nodes.Add(positions[i]);
                nodes.Add(positions[i + 1]);
                nodes.Add(anchorAfter);
                splines.Add(new SplineBuilder(nodes));
            }
        }     
    }

    /*
    public sealed class LinkModifierWindow : EditorWindow
    {
        public static AnimatedCameraPathData asset;
        public static int linkIndex;

        private static LinkModifierWindow window;
        private static Transform trans = null;
        public static void OpenWindow()
        {
            if (window == null) {
                window = CreateInstance<LinkModifierWindow>();
            }
            window.ShowModalUtility();
            trans = null;
        }
        private void OnGUI()
        {
            if (asset == null) {
                return;
            }
            var so = new SerializedObject(asset);
            so.Update();
            using (new EditorGUI.DisabledScope(true)) {
                EditorGUILayout.PropertyField(so.FindProperty("m_Script"));
                EditorGUILayout.ObjectField("Asset", asset, typeof(AnimatedCameraPathData), false);
                EditorGUILayout.IntField("Link Index", linkIndex);
            }

            var linkDataProperty = so.FindProperty("linkDatas").GetArrayElementAtIndex(linkIndex);
            var enumerator = linkDataProperty.GetEnumerator();
            SerializedProperty translateTypeProp = null;
            SerializedProperty translateSpeedProp = null;
            SerializedProperty rotationTypeProp = null;
            SerializedProperty lookatTargetProp = null;
            while (enumerator.MoveNext()) {
                var curr = enumerator.Current as SerializedProperty;
                var name = curr.name;
                if (name.Equals("translateType")) {
                    translateTypeProp = curr.Copy();
                } else if (name.Equals("translateSpeed")) {
                    translateSpeedProp = curr.Copy();
                } else if (name.Equals("rotationType")) {
                    rotationTypeProp = curr.Copy();
                } else if (name.Equals("lookatTarget")) {
                    lookatTargetProp = curr.Copy();
                }
            }


            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Movement", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(translateTypeProp);
            EditorGUILayout.PropertyField(translateSpeedProp);


            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Rotation", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(rotationTypeProp);
            if (rotationTypeProp.enumValueIndex == 3) {
                EditorGUI.BeginChangeCheck();
                trans = (Transform)EditorGUILayout.ObjectField("Lookat Target", trans, typeof(Transform), true);
                if (EditorGUI.EndChangeCheck()) {
                    if (trans != null) {
                        lookatTargetProp.vector3Value = trans.position;
                    } else {
                        lookatTargetProp.vector3Value = Vector3.zero;
                    }
                }
                using (new EditorGUI.DisabledScope(true)) {
                    EditorGUILayout.PropertyField(lookatTargetProp);
                }
            }

            so.ApplyModifiedProperties();

        }
    }
    */
}

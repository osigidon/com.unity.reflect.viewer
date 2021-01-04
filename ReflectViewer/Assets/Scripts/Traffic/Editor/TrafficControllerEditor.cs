using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CivilFX.TrafficV5
{
    [CustomEditor(typeof(TrafficController))]
    public class TrafficControllerEditor : Editor
    {

        private SerializedObject so;
        private TrafficController _target;

        private void OnEnable()
        {
            so = serializedObject;
            _target = (TrafficController)target;
        }

        public override void OnInspectorGUI()
        {
            so.Update();

            //show script name
            SerializedProperty currentProp = so.FindProperty("m_Script");
            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.PropertyField(currentProp);
            }


            //cars
            ShowProperty("carPrefabs");
            //motocycle
            ShowProperty("motorcyclePrefabs");
            //truck
            ShowProperty("truckPrefabs");
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.Space();
            if (GUILayout.Button("Load vehicles", GUILayout.MaxWidth(100)))
            {
                Scene currentScene = _target.gameObject.scene;
                string[] allAssetNames = AssetDatabase.GetAllAssetPaths();
                List<string> vehiclePrefabPaths = new List<string>();
                List<GameObject> vehiclePrefabs = new List<GameObject>();
                foreach (var name in allAssetNames)
                {
                    if (name.Contains(".prefab"))
                    {
                        var go = AssetDatabase.LoadAssetAtPath<GameObject>(name);
                        if (go.GetComponent<VehicleController>() != null)
                        {
                            vehiclePrefabs.Add(go);
                        }
                    }
                }
                List<GameObject> cars = new List<GameObject>(vehiclePrefabs.Count);
                List<GameObject> motocycles = new List<GameObject>(vehiclePrefabs.Count);
                List<GameObject> trucks = new List<GameObject>(vehiclePrefabs.Count);

                foreach (var vehicle in vehiclePrefabs)
                {
                    var vc = vehicle.GetComponent<VehicleController>();
                    switch (vc.vehicleType)
                    {
                        case VehicleType.Car:
                            cars.Add(vehicle);
                            break;
                        case VehicleType.Motorcycle:
                            motocycles.Add(vehicle);
                            break;
                        case VehicleType.Truck:
                            trucks.Add(vehicle);
                            break;
                    }
                }

                //cars
                SetPropertyArray(so.FindProperty("carPrefabs"), cars);
                //motocycles
                SetPropertyArray(so.FindProperty("motorcyclePrefabs"), motocycles);
                //trucks
                SetPropertyArray(so.FindProperty("truckPrefabs"), trucks);

            }
            EditorGUILayout.Space();
            EditorGUILayout.EndHorizontal();



            //allpathcontrollers
            currentProp = so.FindProperty("allPathControllers");
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(currentProp, FormatName(currentProp.name, currentProp.arraySize), false, GUILayout.MaxWidth(200));
            //add all instances button
            if (GUILayout.Button(new GUIContent("++", "Add all instances"), GUILayout.MaxWidth(50)))
            {
                var sceneName = _target.gameObject.scene.name;
                var allScripts = Resources.FindObjectsOfTypeAll<TrafficPathController>();

                var validScripts = new List<TrafficPathController>(allScripts.Length);
                foreach (var script in allScripts)
                {
                    //if (script.gameObject.scene.name.Equals(sceneName))
                    //{
                    validScripts.Add(script);
                    //}
                }
                currentProp.ClearArray();
                currentProp.arraySize = validScripts.Count;

                validScripts = validScripts.OrderBy(x => x.name).ToList();
                for (int i = 0; i < validScripts.Count; i++)
                {
                    currentProp.GetArrayElementAtIndex(i).objectReferenceValue = validScripts[i];
                }
            }

            //remove all instances button
            if (GUILayout.Button(new GUIContent("--", "Remove all instances"), GUILayout.MaxWidth(50)))
            {
                currentProp.ClearArray();
            }

            EditorGUILayout.EndHorizontal();
            if (currentProp.isExpanded)
            {
                EditorGUI.indentLevel++;
                for (int i = 0; i < currentProp.arraySize; i++)
                {
                    GUILayout.BeginHorizontal();
                    SerializedProperty nodeProp = currentProp.GetArrayElementAtIndex(i);
                    EditorGUILayout.PropertyField(nodeProp, new GUIContent(""));

                    //path type
                    TrafficPathController pathController = nodeProp.objectReferenceValue as TrafficPathController;
                    if (pathController != null)
                    {
                        TrafficPath path = pathController.path;
                        SerializedObject pathSO = new SerializedObject(path);
                        pathSO.Update();
                        SerializedProperty pathTypeSP = pathSO.FindProperty("pathType");
                        EditorGUILayout.PropertyField(pathTypeSP, new GUIContent(""), GUILayout.MaxWidth(150));
                        pathSO.ApplyModifiedProperties();
                    }
                    //delete button
                    if (GUILayout.Button(new GUIContent("X", "Delete"), GUILayout.MaxWidth(50)))
                    {
                        currentProp.MoveArrayElement(i, currentProp.arraySize - 1);
                        currentProp.arraySize -= 1;
                    }
                    GUILayout.EndHorizontal();
                }
                EditorGUI.indentLevel--;
            }

            so.ApplyModifiedProperties();
        }

        private GUIContent FormatName(string name, int size)
        {
            StringBuilder sb = new StringBuilder();

            foreach (var c in name)
            {
                if (char.IsUpper(c))
                {
                    sb.Append(' ');
                }
                sb.Append(c);
            }
            sb[0] = char.ToUpper(sb[0]);
            sb.Append(' ');
            sb.Append('(');
            sb.Append(size.ToString());
            sb.Append(')');

            return new GUIContent(sb.ToString());

        }

        private void ShowProperty(string propName)
        {
            EditorGUILayout.PropertyField(so.FindProperty(propName));
        }


        private void SetPropertyArray(SerializedProperty prop, List<GameObject> list)
        {
            prop.arraySize = list.Count;
            for (int i = 0; i < list.Count; i++)
            {
                prop.GetArrayElementAtIndex(i).objectReferenceValue = list[i];
            }
        }

    }
}
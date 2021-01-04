using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CivilFX.TrafficV5
{

    public enum SignalType
    {
        FixedInterval,
        RingBarriers
    }

    public enum MovementType
    {
        Through,
        LeftTurn
    }

    [System.Serializable]
    public class SignalPath
    {
        public TrafficPathController pathController;
        public TrafficPathController targetPath;
        public MovementType movementType;
        public float[] stoppedPoints;
        public int[] greenInterval;

        [HideInInspector]
        public VehicleController[] obstacles;

        private bool redIntervalWork;
        private bool greenIntervalWork;

        //Added for light control
        public bool controlLight;
        public GameObject[] greenLights;
        public GameObject[] yellowLights;
        public GameObject[] redLights;


#if (UNITY_EDITOR)
        public List<GameObject> stopBars;
#endif

        public void Activate(float currentInterval)
        {
            bool greenTime = currentInterval >= greenInterval[0] && currentInterval <= greenInterval[1];
            bool yellowTime = currentInterval >= greenInterval[1] && currentInterval <= greenInterval[1] + 3f;
            if (greenTime || yellowTime)
            {
                //green time
                if (greenTime)
                {
                    if (!greenIntervalWork)
                    {
                        pathController.RemoveObstacles(new List<VehicleController>(obstacles));
                        greenIntervalWork = true;
                        redIntervalWork = false;
#if (UNITY_EDITOR)
                        foreach (var go in stopBars)
                        {
                            go.GetComponent<Renderer>().material.color = Color.green;
                        }
#endif
                        if (controlLight)
                        {
                            ActivateLights(greenLights);
                        }
                    }
                }
                else
                {
                    //yellow time
#if (UNITY_EDITOR)
                    foreach (var go in stopBars)
                    {
                        go.GetComponent<Renderer>().material.color = Color.yellow;
                    }
#endif
                    if (controlLight)
                    {
                        ActivateLights(yellowLights);
                    }
                }
            }
            else
            {
                //red time
                if (!redIntervalWork)
                {
                    pathController.AddObstacles(new List<VehicleController>(obstacles));
                    redIntervalWork = true;
                    greenIntervalWork = false;
#if (UNITY_EDITOR)
                    foreach (var go in stopBars)
                    {
                        go.GetComponent<Renderer>().material.color = Color.red;
                    }
#endif
                    if (controlLight)
                    {
                        ActivateLights(redLights);
                    }
                }


            }
        }

        public void ActivateYield()
        {
            var otherPathVehicles = targetPath.GetTargetNeighbourhood(0, targetPath.path.pathLength, 0);

            Debug.Log(otherPathVehicles.Count);

            if (otherPathVehicles.Count <= 4)
            {
                pathController.RemoveObstacles(new List<VehicleController>(obstacles));
            }

            else
            {
                pathController.AddObstacles(new List<VehicleController>(obstacles));
            }
        }

        public void ActivateLights(GameObject[] activeLights)
        {
            //disable all lights
            foreach (var go in greenLights)
            {
                go.SetActive(false);
            }
            foreach (var go in yellowLights)
            {
                go.SetActive(false);
            }
            foreach (var go in redLights)
            {
                go.SetActive(false);
            }

            //enable back the active lights
            foreach (var go in activeLights)
            {
                go.SetActive(true);
            }
        }
    }

    public class IntersectionController : MonoBehaviour
    {
        public GameObject stopBarPrefab;
        public SignalType signalType;
        public int interval;
        public SignalPath[] signalPaths;
        public float currentTime;

        public void Start()
        {
            GameObject so = new GameObject("Dummy");
            so.gameObject.transform.SetParent(gameObject.transform);

#if (UNITY_EDITOR)

#endif

            //
            foreach (var signalPath in signalPaths)
            {
                //call here to initialy disable all lights
                signalPath.ActivateLights(new GameObject[0]);

                signalPath.obstacles = new VehicleController[signalPath.stoppedPoints.Length];
                var stopBars = new List<GameObject>();
                for (int i = 0; i < signalPath.obstacles.Length; i++)
                {
                    var obstacle = so.AddComponent<VehicleController>();
                    obstacle.vehicleType = VehicleType.TrafficObstacle;
                    obstacle.curvePos = signalPath.stoppedPoints[i];
                    obstacle.currentLane = i;
                    obstacle.vehicleLength = 1;
                    obstacle.longModel = Models.GetVirtualLongModel();
                    obstacle.LCModel = Models.GetVirtualLCModel();
                    signalPath.obstacles[i] = obstacle;

#if (UNITY_EDITOR)
                    GameObject stopGO = GameObject.Instantiate(stopBarPrefab);
                    stopGO.transform.SetParent(gameObject.transform);
                    stopGO.transform.position = signalPath.pathController.GetWorldPositionFromLaneIndex(obstacle.curvePos, i);
                    stopGO.transform.LookAt(signalPath.pathController.GetWorldPositionFromLaneIndex(obstacle.curvePos + 1, i));
                    stopBars.Add(stopGO);
#endif
                }
#if (UNITY_EDITOR)
                signalPath.stopBars = stopBars;
#endif
            }
        }

        private void Update()
        {
            currentTime += Time.deltaTime;
            if (currentTime >= interval)
            {
                currentTime = 0;
            }

            foreach (var signalPath in signalPaths)
            {
                signalPath.Activate(currentTime);
            }
        }

    }
}
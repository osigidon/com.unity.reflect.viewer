using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CivilFX.TrafficV5
{
    [System.Serializable]
    public class ConflictPath
    {
        public TrafficPathController pathController;
        public float umin;
        public float umax;
        public int[] lanes;

        public float GetMaxSpeed()
        {
            float maxSpeed = 0;   
            foreach (var lane in lanes) {
                var targets = pathController.GetTargetNeighbourhood(umin, umax, lane);    
                if (targets.Count > 0) {
                    if (targets[0].speed > maxSpeed) {
                        maxSpeed = targets[0].speed;
                    }
                }
            }
            return maxSpeed;
        }
    }

    [System.Serializable]
    public class ConflictZone
    {
        public ConflictPath[] highPriorities;
        public TrafficPathController lowPriority;
        public float lowPriorityYield;
        public float lowPriorityStop;

        private List<VehicleController> yieldObstacles;
        private List<VehicleController> stopObstacles;

        public void Init(GameObject parent, CarFollowingModel longModel, LaneChangingModel LCModel)
        {
            //yield
            var lanesCount = lowPriority.path.lanesCount;
            yieldObstacles = new List<VehicleController>(lanesCount);
            for (int i=0; i<lanesCount; i++) {
                var yield = parent.AddComponent<VehicleController>();
                yield.vehicleType = VehicleType.Yield;
                yield.Renew(lowPriorityYield, i, 0, longModel, LCModel);
                yieldObstacles.Add(yield);           
            }

            //
            stopObstacles = new List<VehicleController>(lowPriority.path.lanesCount);
            for (int i=0; i<lanesCount; i++) {
                var stop = parent.AddComponent<VehicleController>();
                stop.vehicleType = VehicleType.TrafficObstacle;
                stop.Renew(lowPriorityStop, i, 0, longModel, LCModel);
                stopObstacles.Add(stop);
            }
        }

        public void SetYield(bool active)
        {
            if (active) {
                lowPriority.AddObstacles(yieldObstacles);
            } else {
                lowPriority.RemoveObstacles(yieldObstacles);
            }
        }

        public void SetStop(bool active)
        {
            if (active) {
                lowPriority.AddObstacles(stopObstacles, true);
            } else {
                lowPriority.RemoveObstacles(stopObstacles);
            }
        }

        public bool IsClearConflict()
        {
            float maxSpeed = 0;
            foreach (var conflictPath in highPriorities) {
                var pathSpeed = conflictPath.GetMaxSpeed();
                if (pathSpeed > maxSpeed) {
                    maxSpeed = pathSpeed;
                }
            }
            return maxSpeed < 5f;
        }

    }

    public class ConflictZoneController : MonoBehaviour
    {
        public ConflictZone[] conflictZones;

        private void Awake()
        {
            //create dummy
            foreach (var zone in conflictZones) {
                var go = new GameObject("zone");
                go.transform.SetParent(gameObject.transform);
                zone.Init(go, Models.GetLongModel(), Models.GetLCModel());
                zone.SetYield(true);
            }
        }

        private void Update()
        {
            foreach (var zone in conflictZones) {
                zone.SetStop(!zone.IsClearConflict());
            }
        }

    }
}
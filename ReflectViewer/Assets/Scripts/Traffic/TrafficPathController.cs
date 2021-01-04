using System.Collections.Generic;
using UnityEngine;

namespace CivilFX.TrafficV5
{

    #region helper classes
    [System.Serializable]
    public class Ramp
    {

        public enum RampMode
        {
            Auto,
            Manual
        }
        public enum RampType
        {
            OnRamp,
            OffRamp,
            Merged,
            Diverged
        }

        public enum RampDirection
        {
            ToRight,
            ToLeft
        }

        public TrafficPathController newPath;
        public RampMode mode;
        public RampType type;
        public RampDirection direction;
        public float uminNewPath;
        public float umin;
        public float umax;

        //editor only variable
#if UNITY_EDITOR
        public bool isExpanded;
#endif
        //lanes merging
        public int[] fromLanes;
        public int[] toLanes;

    }

    [System.Serializable]
    public class Obstacle
    {
        public float u;
        public int lane;
    }

    [System.Serializable]
    public class PartitionSegment
    {
        public float start;
        public float end;
        public int lane;

        public bool IsVehicleInPartitionSegment(float u, int newLane)
        {
            return (newLane == lane && u >= start && u <= end);
        }
    }

    [System.Serializable]
    public class Yield
    {
        public float u;
    }
    #endregion

    public class TrafficPathController : MonoBehaviour
    {
        public TrafficPath path;

        public Ramp[] ramps;
        public Obstacle[] obstacles;
        public PartitionSegment[] partitionSegments;
        public Yield[] yieldPoints;
        public VehicleType excludeType = VehicleType.Shadow;

#if UNITY_EDITOR
        public bool showRampHandles = true;
        public bool showObstacleHandles = true;
        public bool showPartitionHandles = true;
        public bool showYieldHandles = true;
#endif


        public int initialVehiclesCount;
        public bool allowRespawning;
        public int inflowCount;
        public bool allowDespawning;

        [HideInInspector]
        public List<VehicleController> vehicles;

        private float pathLength;
        private SplineBuilder pathSpline;
        private int iTargetFirst;
        private int vehiclesCount;

        //respawning control
        private int vehiclesToRespawnInOneSecond;
        private int vehiclesToRespawn;
        private float timeSinceLastRespawning;
        private float durationToRespawnVehicle;

        private GameObject[] prefabs;

        private VehicleController dummyLeader;
        private VehicleController dummyFollower;



        public void Awake()
        {
            pathSpline = path.GetSplineBuilder(true);
            pathLength = pathSpline.pathLength;
        }

        public void Init(GameObject[] _prefabs, int _vehiclesCount, List<VehicleController> waitingVehicles)
        {
            prefabs = _prefabs;
            //Debug.Log("Init vehicles for path: " + gameObject.name);
            //construct dummy vehicles
            dummyLeader = transform.gameObject.AddComponent<VehicleController>();
            dummyLeader.curvePos = 10000;
            dummyLeader.longModel = Models.GetLongModel();
            dummyLeader.LCModel = Models.GetLCModel();
            dummyLeader.vehicleType = VehicleType.Shadow;

            dummyFollower = transform.gameObject.AddComponent<VehicleController>();
            dummyFollower.curvePos = -10000;
            dummyFollower.longModel = Models.GetLongModel();
            dummyFollower.LCModel = Models.GetLCModel();
            dummyFollower.vehicleType = VehicleType.Shadow;

            //
            pathSpline = path.GetSplineBuilder(true);
            pathLength = pathSpline.pathLength;
            vehiclesCount = _vehiclesCount;
            if (vehicles == null)
            {
                vehicles = new List<VehicleController>(_vehiclesCount);
            }
            while (_vehiclesCount > 0)
            {
                var go = GameObject.Instantiate(_prefabs[UnityEngine.Random.Range(0, _prefabs.Length)]);
                go.transform.SetParent(transform);
                vehicles.Add(go.GetComponent<VehicleController>());
                _vehiclesCount--;
            }
            //Debug.Log(pathLength);
            var uSegment = (pathLength - (pathLength * 0.05f)) / vehicles.Count;


            var i = 0;
            foreach (var vehicle in vehicles)
            {
                if (vehicle.IsVirtual)
                {
                    continue;
                }
                vehicle.longModel = Models.GetLongModel(vehicle.vehicleType, path.pathType);
                vehicle.LCModel = Models.GetLCModel(vehicle.vehicleType);
                var newU = uSegment * (i + 1);
                var newLane = UnityEngine.Random.Range(0, path.lanesCount);
                vehicle.Renew(newU, newLane, 30);
                ++i;
            }

            List<VehicleController> invalidVehicles = new List<VehicleController>(vehiclesCount);
            //check if vehicles in cutSegments
            //if so, move them all out of path
            foreach (var vehicle in vehicles)
            {
                foreach (var segment in partitionSegments)
                {
                    if (segment.IsVehicleInPartitionSegment(vehicle.curvePos, vehicle.currentLane))
                    {
                        vehicle.curvePos = pathLength + 1f;
                        invalidVehicles.Add(vehicle);
                        break;
                    }
                }
            }
            if (invalidVehicles.Count > 0)
            {
                foreach (var vehicle in invalidVehicles)
                {
                    vehicles.Remove(vehicle);
                    vehicle.SetVisible(false);
                }
                waitingVehicles.AddRange(invalidVehicles);
            }

            //allowDespawning
            allowDespawning = true;
            foreach (var ramp in ramps)
            {
                if (ramp.type == Ramp.RampType.Merged)
                {
                    allowDespawning = false;
                    break;
                }
            }

            //respawning control
            vehiclesToRespawnInOneSecond = Mathf.CeilToInt(inflowCount / 3600f);
            durationToRespawnVehicle = (3600f / inflowCount);

            //obstacles
            List<VehicleController> newObstacles = new List<VehicleController>(obstacles.Length);
            foreach (var obs in obstacles)
            {
                var vehicleController = gameObject.AddComponent<VehicleController>();
                vehicleController.vehicleType = VehicleType.RoadObstacle;
                vehicleController.vehicleLength = 1;
                vehicleController.Renew(obs.u, obs.lane, 0, Models.GetLongModel(), Models.GetLCModel());
                newObstacles.Add(vehicleController);
            }
            AddObstacles(newObstacles);
        }

        public void AddObstacles(List<VehicleController> obstacles, bool requiredModels = false, bool updateVisual = false)
        {
            if (obstacles != null && obstacles.Count > 0)
            {
                Debug.Log($"Adding {obstacles.Count}");
                vehicles.AddRange(obstacles);
                if (requiredModels)
                {
                    foreach (var obs in obstacles)
                    {
                        obs.longModel = Models.GetLongModel();
                        obs.LCModel = Models.GetLCModel();
                    }
                }
                UpdateEnvironment();

                if (updateVisual)
                {
                    foreach (var obs in obstacles)
                    {
                        if (obs.gameObject == gameObject)
                        {
                            obs.SetPosition(GetWorldPosition(in obs, 0));
                            obs.SetRotation(pathSpline.GetOrientation(obs.curvePos / pathLength));
                        }
                    }
                }

            }
        }
        /// <summary>
        /// Remove obstacles on path
        /// </summary>
        /// <param name="obstacles">Obstacles to remove</param>
        public void RemoveObstacles(List<VehicleController> obstacles)
        {
            if (obstacles != null)
            {
                vehicles.RemoveAll(item => obstacles.Contains(item));
                UpdateEnvironment();
            }
        }

        public void SetLCMandatory(float umin, float umax, bool toRight)
        {
            foreach (var vehicle in vehicles)
            {
                if (!vehicle.IsVirtual)
                {
                    var curvePos = vehicle.curvePos;
                    if ((curvePos > umin) && (curvePos < umax))
                    {
                        vehicle.LCModel = Models.GetMandatoryLCModel(toRight);
                    }
                }
            }
        }

        public Vector3 GetWorldPositionFromLaneIndex(float curvePos, int lane)
        {
            var centerStart = pathSpline.GetPointOnPathSegment(curvePos);
            var dir = pathSpline.GetTangentOnPathSegment(curvePos).normalized;
            var right = Vector3.Cross(Vector3.up, dir) * path.calculatedWidth;
            var left = -right;
            var seg = (2f * lane + 1f) / (2f * path.lanesCount); //lerp value based on lane number               
            var pos = Vector3.Lerp(centerStart + left, centerStart + right, seg); //actualy position based on lane number
            return pos;
        }

        public Quaternion GetWorldRotation(float curvePos)
        {
            return pathSpline.GetOrientation(Mathf.Clamp01(curvePos / pathLength));
        }

        public void CalcAcceleration()
        {
            UpdateEnvironment();
            for (int i = 0; i < vehicles.Count; i++)
            {
                var vehicle = vehicles[i];
                float speed = vehicle.speed;
                int iLead = vehicle.iLead;
                float s;
                float iLeadSpeed;
                float iLeadAcc;

                if (iLead >= 0 && vehicles[iLead].vehicleType == VehicleType.Yield && vehicle.speed <= 5f)
                {
                    vehicle.yieldingReference = vehicles[iLead];
                }

                if (iLead == VehicleController.EXTERNAL_LEAD_ID)
                {
                    s = vehicle.externalLead.curvePos - vehicle.externalLead.vehicleLength + (vehicle.externalLeadCurvePos - vehicle.curvePos);
                    iLeadSpeed = vehicle.externalLead.speed;
                    iLeadAcc = vehicle.externalLead.accelerate;
                }
                else if (vehicle.yieldingReference != null)
                {
                    if (vehicle.yieldingReference.iLead == VehicleController.EXTERNAL_LEAD_ID)
                    {
                        var iLeadYield = vehicle.yieldingReference.externalLead;
                        s = iLeadYield.curvePos - iLeadYield.vehicleLength + (vehicle.yieldingReference.externalLeadCurvePos - vehicle.curvePos);
                        iLeadSpeed = iLeadYield.speed;
                        iLeadAcc = iLeadYield.accelerate;
                    }
                    else
                    {
                        var iLeadYield = vehicle.yieldingReference.iLead;
                        s = vehicles[iLeadYield].curvePos - vehicles[iLeadYield].vehicleLength - vehicle.curvePos;
                        iLeadSpeed = vehicles[iLeadYield].speed;
                        iLeadAcc = vehicles[iLeadYield].accelerate;
                    }
                    vehicle.yieldingReference = null;
                }
                else
                {
                    s = vehicles[iLead].curvePos - vehicles[iLead].vehicleLength - vehicle.curvePos;
                    iLeadSpeed = vehicles[iLead].speed;
                    iLeadAcc = vehicles[iLead].accelerate;
                }

                if (iLead >= i)
                {
                    s = 10000;
                    iLeadAcc = 0;
                }

                vehicle.accelerate = vehicle.IsVirtual ? 0 : vehicle.longModel.CalculateAcceleration(s, speed, iLeadSpeed, iLeadAcc);


                if (iLead >= 0 && vehicles[iLead].vehicleType == VehicleType.Yield && vehicle.speed < 1f)
                {
                    vehicle.accelerate += 0.5f;
                }
            }

        }

        public void UpdateGeneralDeltaTime(float dt)
        {
            foreach (var vehicle in vehicles)
            {
                vehicle.UpdateDeltaTime(dt);
            }
        }

        public void ChangeLane()
        {
            for (int i = 0; i < vehicles.Count; i++)
            {
                if (vehicles[i].IsVirtual)
                {
                    continue;
                }
                int iLead = vehicles[i].iLead;
                if (iLead >= 0 && vehicles[iLead].vehicleType == VehicleType.RoadObstacle)
                {
                    DoLaneChange(i, true);
                    DoLaneChange(i, false);
                }
                else if (vehicles[i].shouldDoLC)
                {
                    DoLaneChange(i, false);
                    DoLaneChange(i, true);
                }
            }
        }

        private void DoLaneChange(int i, bool toRight)
        {


            float uminLC = vehicles[i].vehicleLength * 4;
            float umaxLC = pathLength - vehicles[i].vehicleLength * 4;

            var vehicle = vehicles[i];
            int currentLane = vehicle.currentLane;
            bool isDuringLC = vehicle.dt_lane_tweening < VehicleController.GetLaneTweeningDuration(vehicles[i].vehicleType);



            if (!isDuringLC && vehicle.curvePos > uminLC && vehicle.curvePos < umaxLC)
            {
                if (vehicle.debug)
                {
                    Debug.Log($"Doing lane change {uminLC} {umaxLC}");
                }
                int newLane = toRight ? currentLane + 1 : currentLane - 1;
                bool isNewLaneValid = newLane >= 0 && newLane < path.lanesCount;
                int iLead = vehicle.iLead;

                if (!isNewLaneValid)
                {
                    if (vehicle.debug)
                    {
                        Debug.Log($"Lane is not valid");
                    }
                    return;
                }

                //check if the newLane is in the forbid lane changing segment
                foreach (var segment in partitionSegments)
                {
                    if (segment.IsVehicleInPartitionSegment(vehicle.curvePos, newLane))
                    {
                        if (vehicle.debug)
                        {
                            Debug.Log($"Lane is in partition");
                        }
                        return;
                    }
                }
                if (iLead >= 0 && vehicles[iLead].vehicleType == VehicleType.TrafficObstacle)
                {
                    if (vehicle.debug)
                    {
                        Debug.Log($"Leader is trafficobstacle");
                    }
                    return;
                }
                int newILead = toRight ? vehicle.iLeadRight : vehicle.iLeadLeft;
                int newIFollow = toRight ? vehicle.iFollowRight : vehicle.iFollowLeft;

                if (!VehicleController.ValidateInRoadVehicleID(newILead))
                {
                    if (vehicle.debug)
                    {
                        Debug.Log($"Leader is not valid");
                    }
                    return;
                }

                if (vehicles[newILead].vehicleType == VehicleType.RoadObstacle)
                {
                    if (vehicle.debug)
                    {
                        Debug.Log("New Leader is road obstacle");
                    }
                    //return;
                }

                float acc = vehicle.accelerate;
                float newILeadAcc = vehicles[newILead].accelerate;
                float speed = vehicle.speed;
                float newILeadSpeed = vehicles[newILead].speed;
                float sNew = vehicles[newILead].curvePos - vehicles[newILead].vehicleLength - vehicle.curvePos;
                float sFollowNew = vehicle.curvePos - vehicle.vehicleLength - vehicles[newIFollow].curvePos;

                if (newILead >= i)
                {
                    sNew = 10000;
                }
                if (newIFollow <= i)
                {
                    sFollowNew = 10000;
                }

                float vrel = speed / vehicle.longModel.v0;
                float newAcc = vehicle.longModel.CalculateAcceleration(sNew, speed, newILeadSpeed, newILeadAcc);

                if (vehicle.debug)
                {
                    Debug.Log($"New Acc: {newAcc}");
                }

                float newIFollowSpeed = vehicles[newIFollow].speed;
                float newIFollowAcc = vehicles[newIFollow].longModel.CalculateAcceleration(sFollowNew, newIFollowSpeed, speed, newAcc);

                bool isMOBILOK = vehicle.LCModel.RealizeLaneChange(vrel, acc, newAcc, newIFollowAcc, toRight);

                bool isChangingSuccess = sNew > 0 && sFollowNew > 0 && isMOBILOK;
                if (isChangingSuccess)
                {
                    vehicle.dt_lane_tweening = 0;
                    vehicle.timeSinceLastLC = 0;
                    vehicle.oldLane = vehicle.currentLane;
                    vehicle.currentLane = newLane;
                    vehicle.accelerate = newAcc;
                    vehicles[newIFollow].accelerate = newIFollowAcc;
                    vehicles[newIFollow].passiveLCTime = 0;
                    UpdateEnvironment();
                }
            }


        }

        #region UpdateEnviroment
        public void UpdateEnvironment()
        {
            SortVehicles();
            for (int i = 0; i < vehicles.Count; i++)
            {
                UpdateILead(i);
                UpdateIFollow(i);
                UpdateILeadRight(i);
                UpdateIFollowRight(i);
                UpdateILeadLeft(i);
                UpdateIFollowLeft(i);
            }
        }

        private void UpdateILead(int i)
        {
            vehicles[i].externalLead = null;
            vehicles[i].externalLeadCurvePos = 0;

            var n = vehicles.Count;
            var iLead = (i == 0) ? n - 1 : i - 1;  //!! also for non periodic BC
            var success = (vehicles[iLead].currentLane == vehicles[i].currentLane);
            while (!success)
            {
                iLead = (iLead == 0) ? n - 1 : iLead - 1;
                success = ((i == iLead) || (vehicles[iLead].currentLane == vehicles[i].currentLane));
            }

            //update exteral ilead
            //in case of merge/diverge
            var curvePos = vehicles[i].curvePos;
            var currentLane = vehicles[i].currentLane;
            var uMax = 0f;
            var uNewPath = 0f;
            var newLane = 0;
            TrafficPathController newPathController = null;
            foreach (var ramp in ramps)
            {
                if (curvePos <= ramp.umax)
                {
                    if (ramp.type == Ramp.RampType.Merged ||
                        (ramp.type == Ramp.RampType.Diverged && vehicles[i].allowLeavingPath))
                    {
                        var toLaneIndex = GetCompatibleMergeDivergeLane(currentLane, ramp.fromLanes);
                        if (toLaneIndex != -1)
                        {
                            newPathController = ramp.newPath;
                            uNewPath = ramp.uminNewPath;
                            uMax = ramp.umax;
                            newLane = ramp.toLanes[toLaneIndex];
                            break;
                        }
                    }
                }
            }
            if (newPathController != null)
            {
                var newTarget = newPathController.GetLastVehicle(newLane, uNewPath);
                if (newTarget != null)
                {
                    var currentTarget = vehicles[iLead];
                    if (iLead >= i || (currentTarget.curvePos - vehicles[i].curvePos > newTarget.curvePos + uMax - vehicles[i].curvePos))
                    {
                        iLead = VehicleController.EXTERNAL_LEAD_ID;
                        vehicles[i].externalLead = newTarget;
                        vehicles[i].externalLeadCurvePos = uMax;
                    }
                }
            }
            vehicles[i].iLead = iLead;
        }
        private void UpdateIFollow(int i)
        {
            var n = vehicles.Count;
            var iFollow = (i == n - 1) ? 0 : i + 1;
            var success = (vehicles[iFollow].currentLane == vehicles[i].currentLane);
            while (!success)
            {
                iFollow = (iFollow == n - 1) ? 0 : iFollow + 1;
                success = ((i == iFollow) || (vehicles[iFollow].currentLane == vehicles[i].currentLane));
            }
            vehicles[i].iFollow = iFollow;
        }

        private void UpdateILeadRight(int i)
        {
            var n = vehicles.Count;
            int iLeadRight;
            if (vehicles[i].currentLane < path.lanesCount - 1)
            {
                iLeadRight = (i == 0) ? n - 1 : i - 1;
                var success = ((i == iLeadRight) || (vehicles[iLeadRight].currentLane == vehicles[i].currentLane + 1));
                while (!success)
                {
                    iLeadRight = (iLeadRight == 0) ? n - 1 : iLeadRight - 1;
                    success = ((i == iLeadRight) || (vehicles[iLeadRight].currentLane == vehicles[i].currentLane + 1));
                }
            }
            else { iLeadRight = -10; }
            vehicles[i].iLeadRight = iLeadRight;
        }

        private void UpdateIFollowRight(int i)
        {
            var n = vehicles.Count;
            int iFollowRight;
            if (vehicles[i].currentLane < path.lanesCount - 1)
            {
                iFollowRight = (i == n - 1) ? 0 : i + 1;
                var success = ((i == iFollowRight) || (vehicles[iFollowRight].currentLane == vehicles[i].currentLane + 1));
                while (!success)
                {
                    iFollowRight = (iFollowRight == n - 1) ? 0 : iFollowRight + 1;
                    success = ((i == iFollowRight) || (vehicles[iFollowRight].currentLane == vehicles[i].currentLane + 1));
                }
            }
            else { iFollowRight = -10; }
            vehicles[i].iFollowRight = iFollowRight;
        }

        private void UpdateILeadLeft(int i)
        {
            var n = vehicles.Count;

            int iLeadLeft;
            if (vehicles[i].currentLane > 0)
            {
                iLeadLeft = (i == 0) ? n - 1 : i - 1;
                var success = ((i == iLeadLeft) || (vehicles[iLeadLeft].currentLane == vehicles[i].currentLane - 1));
                while (!success)
                {
                    iLeadLeft = (iLeadLeft == 0) ? n - 1 : iLeadLeft - 1;
                    success = ((i == iLeadLeft) || (vehicles[iLeadLeft].currentLane == vehicles[i].currentLane - 1));
                }
            }
            else { iLeadLeft = -10; }
            vehicles[i].iLeadLeft = iLeadLeft;
        }

        private void UpdateIFollowLeft(int i)
        {
            var n = vehicles.Count;
            int iFollowLeft;

            if (vehicles[i].currentLane > 0)
            {
                iFollowLeft = (i == n - 1) ? 0 : i + 1;
                var success = ((i == iFollowLeft) || (vehicles[iFollowLeft].currentLane == vehicles[i].currentLane - 1));
                while (!success)
                {
                    iFollowLeft = (iFollowLeft == n - 1) ? 0 : iFollowLeft + 1;
                    success = ((i == iFollowLeft) || (vehicles[iFollowLeft].currentLane == vehicles[i].currentLane - 1));
                }
            }
            else { iFollowLeft = -10; }
            vehicles[i].iFollowLeft = iFollowLeft;
        }
        #endregion


        public void UpdateCurvePosition(float dt)
        {
            foreach (var vehicle in vehicles)
            {
                if (vehicle.IsVirtual)
                {
                    continue;
                }
                vehicle.curvePos += Mathf.Max(0, vehicle.speed * dt + 0.5f * vehicle.accelerate * dt * dt);
                vehicle.speed = Mathf.Max(vehicle.speed + vehicle.accelerate * dt, 0);
            }
        }

        public void RespawnVehicles(List<VehicleController> waitingVehicles, float dt)
        {

            if (!allowRespawning)
            {
                return;
            }

            timeSinceLastRespawning -= dt;
            if (timeSinceLastRespawning <= 0)
            {
                int currentLane = path.lanesCount - 1;
                float smin = 10f;
                bool success = false;
                float space = 0f;

                if (vehicles.Count == 0)
                {
                    success = true;
                    space = pathLength;
                }
                else
                {
                    float spaceMax = 0f;
                    for (int candidateLane = path.lanesCount - 1; candidateLane >= 0; candidateLane--)
                    {
                        int iLead = vehicles.Count - 1;
                        while ((iLead >= 0) && vehicles[iLead].currentLane != candidateLane)
                        {
                            --iLead;
                        }
                        space = (iLead >= 0) ? vehicles[iLead].curvePos - vehicles[iLead].vehicleLength : path.pathLength + candidateLane;
                        if (space > spaceMax)
                        {
                            currentLane = candidateLane;
                            spaceMax = space;
                        }
                    }
                    success = space >= smin;
                }
                if (success)
                {

                    //get new vehicle
                    VehicleController newVehicle;
                    if (waitingVehicles.Count > 0)
                    {
                        newVehicle = waitingVehicles[0];
                        waitingVehicles.Remove(newVehicle);
                    }
                    else
                    {
                        var go = GameObject.Instantiate(prefabs[Random.Range(0, prefabs.Length)]);
                        go.transform.SetParent(transform);
                        newVehicle = go.GetComponent<VehicleController>();
                        newVehicle.SetVisible(false);
                    }



                    if (newVehicle.vehicleType == excludeType)
                    {
                        waitingVehicles.Add(newVehicle);
                    }
                    else
                    {
                        var longModel = Models.GetLongModel(newVehicle.vehicleType, path.pathType);
                        var newSpeed = Mathf.Min(longModel.v0, longModel.speedLimit, space / longModel.T);
                        newVehicle.SetVisible(true);
                        //newVehicle.Renew(0, currentLane, newSpeed, longModel, GetLCModel());
                        newVehicle.Renew(newVehicle.vehicleLength, currentLane, newSpeed, longModel, Models.GetLCModel(newVehicle.vehicleType));
                        //newVehicle.transform.eulerAngles = pathSpline.GetTangentOnPath(0.1f);
                        vehicles.Add(newVehicle);
                    }
                }
                timeSinceLastRespawning = durationToRespawnVehicle;
            }
        }

        public void DespawnVehicles(List<VehicleController> waitingVehicles)
        {
            if (allowDespawning)
            {
                if (vehicles.Count > 0 && vehicles[0].curvePos > pathLength && !vehicles[0].IsVirtual)
                {
                    var vehicle = vehicles[0];
                    vehicles.Remove(vehicle);
                    vehicle.SetVisible(false);
                    waitingVehicles.Add(vehicle);
                }
            }
        }
        public void MergeDiverge(Ramp ramp)
        {
            var newPathController = ramp.newPath;
            var offset = ramp.uminNewPath - ramp.umax;
            var uEnd = ramp.umax;
            var rampType = ramp.type;
            var fromLanes = ramp.fromLanes;
            var toLanes = ramp.toLanes;

            //Debug.Log("Prepare to changing into");
            //only vehicle to merge at a frame
            VehicleController changingVeh = null;
            int newLane = 999;
            var uDiverge = uEnd + 2f; //increase if vehicle should diverge but unable to
            foreach (var vehicle in vehicles)
            {
                if (!vehicle.IsVirtual)
                {
                    if (rampType == Ramp.RampType.Merged && vehicle.curvePos >= uEnd)
                    {
                        changingVeh = vehicle;
                    }
                    else if (rampType == Ramp.RampType.Diverged && vehicle.allowLeavingPath && vehicle.curvePos >= uEnd && vehicle.curvePos <= uDiverge)
                    {
                        changingVeh = vehicle;
                    }

                    //second pass
                    if (changingVeh != null)
                    {
                        var index = GetCompatibleMergeDivergeLane(changingVeh.currentLane, fromLanes);
                        if (index != -1)
                        {
                            newLane = toLanes[index];
                            break;
                        }
                        else
                        {
                            changingVeh = null;
                        }
                    }

                }
            }

            if (changingVeh != null)
            {
                changingVeh.UpdateLastPathController(this, changingVeh.oldLane, changingVeh.currentLane, ramp.umax);
                changingVeh.curvePos += offset;
                changingVeh.oldLane -= changingVeh.currentLane - newLane;
                changingVeh.currentLane = newLane;
                changingVeh.ReRoute();
                changingVeh.longModel = Models.GetLongModel(changingVeh.vehicleType, newPathController.path.pathType);
                vehicles.Remove(changingVeh);
                UpdateEnvironment();
                newPathController.vehicles.Add(changingVeh);
                newPathController.UpdateEnvironment();
            }
        }

        public void OnRampOffRamp(Ramp ramp)
        {
            float offset = ramp.uminNewPath - ramp.umin;
            float uBegin = ramp.umin;
            float uEnd = ramp.umax;
            bool isMerge = ramp.type == Ramp.RampType.OnRamp;
            bool toRight = ramp.direction == Ramp.RampDirection.ToRight;
            bool ignoreRoute = false;
            bool prioOther = false;
            bool prioOwn = false;
            TrafficPathController newPath = ramp.newPath;

            var padding = 20; // visib. extension for orig drivers to target vehs
            var paddingLTC =           // visib. extension for target drivers to orig vehs
            (isMerge && prioOwn) ? 20 : 0;

            var loc_ignoreRoute = ignoreRoute; // default: routes  matter at diverges
            if (isMerge) loc_ignoreRoute = true;  // merging must be always possible

            var loc_prioOther = prioOther;

            var loc_prioOwn = prioOwn;
            if (loc_prioOwn && loc_prioOther)
            {
                Debug.Log("road.mergeDiverge: Warning: prioOther and prioOwn" +
                        " cannot be true simultaneously; setting prioOwn=false");
                loc_prioOwn = false;
            }

            // (1) get neighbourhood
            // GetTargetNeighbourhood also sets [this|newPath].iTargetFirst

            var uNewBegin = uBegin + offset;
            var uNewEnd = uEnd + offset;
            var originLane = (toRight) ? path.lanesCount - 1 : 0;
            var targetLane = (toRight) ? 0 : newPath.path.lanesCount - 1;
            var originVehicles = GetTargetNeighbourhood(
            uBegin - paddingLTC, uEnd, originLane); // padding only for LT coupling!

            var targetVehicles = newPath.GetTargetNeighbourhood(
            uNewBegin - padding, uNewEnd + padding, targetLane);
            var iMerge = 0; // candidate of the originVehicles neighbourhood
            var uTarget = 0f;  // long. coordinate of this vehicle on the orig road

            // (2) select changing vehicle (if any): 
            // only one at each calling; the first vehicle has priority!

            // (2a) immediate success if no target vehicles in neighbourhood
            // and at least one (real) origin vehicle: the first one changes

            //Debug.Log("targetVehicles.Count: " + targetVehicles.Count);
            //Debug.Log("originVehicles.Count: " + originVehicles.Count);
            var success = ((targetVehicles.Count == 0) && (originVehicles.Count > 0)
                  && !originVehicles[0].IsVirtual
                  && (originVehicles[0].curvePos >= uBegin)); // otherwise only LT coupl
            /*&& (loc_ignoreRoute || originVehicles[0].divergeAhead));*/

            //Debug.Log("success: " + success);
            if (success)
            {
                iMerge = 0; uTarget = originVehicles[0].curvePos + offset;
            }

            // (2b) otherwise select the first suitable candidate of originVehicles
            else if (originVehicles.Count > 0)
            {

                // initializing of interacting partners with virtual vehicles
                // having no interaction because of their positions
                // default models also initialized in the constructor

                var duLeader = 1000f; // initially big distances w/o interaction
                var duFollower = -1000f;
                var leaderNew = dummyLeader;
                var followerNew = dummyFollower;

                leaderNew.Renew(uNewBegin + 10000, targetLane, 0, null, null);
                followerNew.Renew(uNewBegin - 10000, targetLane, 0, null, null);

                // loop over originVehicles for merging veh candidates
                for (var i = 0; (i < originVehicles.Count) && (!success); i++)
                {
                    if (!originVehicles[i].IsVirtual)
                    /*&& (loc_ignoreRoute || originVehicles[i].divergeAhead))*/
                    {

                        //inChangeRegion can be false for LTC since then paddingLTC>0
                        var inChangeRegion = (originVehicles[i].curvePos > uBegin);

                        uTarget = originVehicles[i].curvePos + offset;

                        // inner loop over targetVehicles: search prospective 
                        // new leader leaderNew and follower followerNew and get the gaps
                        // notice: even if there are >0 target vehicles 
                        // (that is guaranteed because of the inner-loop conditions),
                        //  none may be eligible
                        // therefore check for jTarget==-1
                        var jTarget = -1; ;
                        for (var j = 0; j < targetVehicles.Count; j++)
                        {
                            var du = targetVehicles[j].curvePos - uTarget;
                            if ((du > 0) && (du < duLeader))
                            {
                                duLeader = du; leaderNew = targetVehicles[j];
                            }
                            if ((du < 0) && (du > duFollower))
                            {
                                jTarget = j; duFollower = du; followerNew = targetVehicles[j];
                            }
                        }

                        // get input variables for MOBIL
                        // qualifiers for state var s,acc: 
                        // [nothing] own vehicle before LC
                        // vehicles: leaderNew, followerNew
                        // subscripts/qualifiers:
                        //   New=own vehicle after LC
                        //   LeadNew= new leader (not affected by LC but acc needed)
                        //   Lag new lag vehicle before LC (only relevant for accLag)
                        //   LagNew=new lag vehicle after LC (for accLagNew)

                        var sNew = duLeader - leaderNew.vehicleLength;
                        var sLagNew = -duFollower - originVehicles[i].vehicleLength;
                        var speedLeadNew = leaderNew.speed;
                        var accLeadNew = leaderNew.accelerate; // leaders=exogen. to MOBIL
                        var speedLagNew = followerNew.speed;
                        var speed = originVehicles[i].speed;

                        var LCModel = Models.GetMandatoryLCModel(toRight);

                        var vrel = originVehicles[i].speed / originVehicles[i].longModel.v0;

                        var acc = originVehicles[i].accelerate;
                        var accNew = originVehicles[i].longModel.CalculateAcceleration(
                        sNew, speed, speedLeadNew, accLeadNew);
                        var accLag = followerNew.accelerate;
                        var accLagNew = originVehicles[i].longModel.CalculateAcceleration(
                        sLagNew, speed, speedLagNew, accNew);


                        // MOBIL decisions
                        var prio_OK = (!loc_prioOther) || loc_prioOwn
                        || (!LCModel.RespectPriority(accLag, accLagNew));

                        var MOBILOK = LCModel.RealizeLaneChange(
                        vrel, acc, accNew, accLagNew, toRight);
                        success = prio_OK && inChangeRegion && MOBILOK
                        && (!originVehicles[i].IsVirtual)
                        && (sNew > 0) && (sLagNew > 0);

                        if (success)
                        {
                            iMerge = i;
                        }

                    } // !obstacle

                }// merging veh loop
            }// else branch (there are target vehicles)


            //(3) realize longitudinal-transversal coupling (LTC)
            // exerted onto target vehicles if merge and loc_prioOwn
            if (isMerge && loc_prioOwn)
            {
                Debug.Log("Enter here");
                // (3a) determine stop line such that there cannot be a grid lock for any
                // merging vehicle, particularly the longest vehicle

                var vehLenMax = 9;
                var stopLinePosNew = uNewEnd - vehLenMax - 2;
                var bSafe = 4;

                // (3b) all target vehs stop at stop line if at least one origin veh
                // is follower and 
                // the deceleration to do so is less than bSafe
                // if the last orig vehicle is a leader and interacting decel is less,
                // use it

                for (var j = 0; j < targetVehicles.Count; j++)
                {
                    var sStop = stopLinePosNew - targetVehicles[j].curvePos; // gap to stop for target veh
                    var speedTarget = targetVehicles[j].speed;
                    var accTargetStop = targetVehicles[j].longModel.CalculateAcceleration(sStop, speedTarget, 0, 0);

                    var iLast = -1;
                    for (var i = originVehicles.Count - 1; (i >= 0) && (iLast == -1); i--)
                    {
                        if (!originVehicles[i].IsVirtual)
                        {
                            iLast = i;
                        }
                    }

                    if ((iLast > -1) && !targetVehicles[j].IsVirtual)
                    {
                        var du = originVehicles[iLast].curvePos + offset - targetVehicles[j].curvePos;
                        var lastOrigIsLeader = (du > 0);
                        if (lastOrigIsLeader)
                        {
                            var s = du - originVehicles[iLast].vehicleLength;
                            var speedOrig = originVehicles[iLast].speed;
                            var accLTC
                            = targetVehicles[j].longModel.CalculateAcceleration(s, speedTarget, speedOrig, 0);
                            var accTarget = Mathf.Min(targetVehicles[j].accelerate,
                                       Mathf.Max(accLTC, accTargetStop));
                            if (accTarget > -bSafe)
                            {
                                targetVehicles[j].accelerate = accTarget;
                            }
                        }
                        else
                        { // if last orig not leading, stop always if it can be done safely
                            if (accTargetStop > -bSafe)
                            {
                                var accTarget = Mathf.Min(targetVehicles[j].accelerate, accTargetStop);
                                targetVehicles[j].accelerate = accTarget;
                            }
                        }
                    }

                }
            }

            //Debug.Log("Merging success: " + success);
            //(4) if success, do the actual merging!
            if (success)
            {// do the actual merging              
                //originVehicles[iMerge]=veh[iMerge+this.iTargetFirst] 
                var iOrig = iMerge + iTargetFirst;
                //Debug.Log(vehicles.Count + ":" + iOrig);
                var changingVeh = vehicles[iOrig]; //originVehicles[iMerge];


                if (isMerge || changingVeh.allowLeavingPath && changingVeh.dt_lane_tweening >= VehicleController.GetLaneTweeningDuration(changingVeh.vehicleType))
                {
                    var vOld = (toRight) ? targetLane - 1 : targetLane + 1; // rel. to NEW road
                    changingVeh.isMergingFromRight = !toRight;
                    changingVeh.isMerging = true;
                    changingVeh.curvePos += offset;
                    changingVeh.currentLane = targetLane;
                    changingVeh.oldLane = vOld; // following for  drawing purposes
                    //changingVeh.dt_afterLC = 0;             // just changed
                    changingVeh.dt_lane_tweening = 0;
                    //changingVeh.divergeAhead = false; // reset mandatory LC behaviour
                    changingVeh.longModel = Models.GetLongModel(changingVeh.vehicleType, newPath.path.pathType);
                    changingVeh.ReRoute();





                    //####################################################################
                    vehicles.Remove(changingVeh);// removes chg veh from orig.
                    newPath.vehicles.Add(changingVeh); // appends changingVeh at last pos;
                                                       //####################################################################
                                                       //newPath.nveh=newPath.veh.length;
                    newPath.UpdateEnvironment(); // and provide updated neighbors
                    this.UpdateEnvironment();
                    //Debug.Log("Scuees merging");
                }
            }// end do the actual merging

        }
        public List<VehicleController> GetTargetNeighbourhood(float umin, float umax, int targetLane)
        {
            //Debug.Log(gameObject.name + " Geting Target Vehicles on: [" + umin + "] to [" + umax + "] for targetLane: " + targetLane);
            List<VehicleController> targets = new List<VehicleController>();
            var firstTime = true;
            iTargetFirst = 0;
            for (int i = 0; i < vehicles.Count; i++)
            {
                if ((vehicles[i].currentLane == targetLane) && (vehicles[i].curvePos >= umin) && (vehicles[i].curvePos <= umax))
                {
                    if (firstTime)
                    {
                        iTargetFirst = i;
                        firstTime = false;
                    }
                    targets.Add(vehicles[i]);
                }
            }
            return targets;
        }

        public void UpdateWorldPosition()
        {
            foreach (var vehicle in vehicles)
            {
                if (vehicle.IsVirtual)
                {
                    continue;
                }

                float curvePos = vehicle.curvePos - vehicle.frontOffset;
                Vector3 pos = default;
                Vector3 backPos = default; //for truck
                Quaternion rot = default;

                //position and rotation when the vehicle first respawn on the path
                TrafficPathController lastPathController = vehicle.lastPathControllerInfo.pathController;
                if (vehicle.curvePos - vehicle.vehicleLength == 0 && lastPathController == null)
                {
                    vehicle.SetPosition(GetWorldPositionFromLaneIndex(vehicle.curvePos, vehicle.currentLane));
                    vehicle.SetRotation(GetWorldRotation(vehicle.curvePos));
                    //vehicle.transform.eulerAngles = pathSpline.GetTangentOnPathSegment(vehicle.curvePos);
                    continue;
                }

                //case for vehicle is in the middle of a merge/diverse
                //when front part of vehicle is on new path but
                // rear part is still on old part
                if (curvePos < 0)
                {
                    if (lastPathController != null)
                    {

                        VehicleController tempVehicle = lastPathController.dummyLeader;
                        tempVehicle.Mirror(vehicle);

                        tempVehicle.curvePos = curvePos + vehicle.lastPathControllerInfo.offset;
                        tempVehicle.currentLane = vehicle.lastPathControllerInfo.currentLane;
                        tempVehicle.oldLane = vehicle.lastPathControllerInfo.oldLane;
                        pos = lastPathController.GetWorldPosition(in tempVehicle, 0);
                        rot = lastPathController.GetWorldRotation(tempVehicle.curvePos);
                        //TODO: reset tempdummyLeader
                    }
                }

                bool isDuringLC = vehicle.dt_lane_tweening < VehicleController.GetLaneTweeningDuration(vehicle.vehicleType);
                if (pos.Equals(default))
                {
                    pos = GetWorldPosition(in vehicle, vehicle.frontOffset);
                }
                if (rot.Equals(default))
                {
                    rot = GetWorldRotation(vehicle.curvePos);
                }
                //set rotation
                if (isDuringLC)
                {
                    vehicle.SetRotation(pos);
                }
                else
                {
                    vehicle.SetRotation(rot);
                }
                //set position
                vehicle.SetPosition(pos);
                //correct trailer for truck
                if (vehicle.isTruck)
                {

                    if (vehicle.curvePos - vehicle.vehicleLength >= 0)
                    {
                        backPos = GetWorldPosition(vehicle, vehicle.vehicleLength);
                    }
                    else if (lastPathController != null)
                    {
                        VehicleController tempVehicle = lastPathController.dummyLeader;
                        tempVehicle.Mirror(vehicle);
                        tempVehicle.curvePos = vehicle.curvePos - vehicle.vehicleLength + vehicle.lastPathControllerInfo.offset;
                        tempVehicle.currentLane = vehicle.lastPathControllerInfo.currentLane;
                        tempVehicle.oldLane = vehicle.lastPathControllerInfo.oldLane;
                        backPos = lastPathController.GetWorldPosition(tempVehicle, 0);
                    }
                    vehicle.CorrectTrailerRotation(pos, backPos);
                }
                /*
                if (vehicle.isTruck) {
                    Vector3 backPos = GetWorldPosition(in vehicle, vehicle.vehicleLength);
                    if (backPos.Equals(default)) {
                        vehicle.CorrectTrailerRotation(GetWorldRotation(vehicle.frontOffset));
                    } else {
                        vehicle.CorrectTrailerRotation(pos, backPos);
                    }
                }
                */
            }
        }
        private Vector3 GetWorldPosition(in VehicleController vehicle, float offset)
        {
            float curvePos = vehicle.curvePos - offset;
            if (curvePos < 0)
            {
                return default;
            }
            Vector3 centerStart = pathSpline.GetPointOnPathSegment(curvePos);
            Vector3 dir = pathSpline.GetTangentOnPathSegment(curvePos);
            Vector3 right = Vector3.Cross(Vector3.up, dir) * path.calculatedWidth;
            Vector3 left = -right;
            float seg = (2f * vehicle.currentLane + 1f) / (2f * path.lanesCount);
            Vector3 pos = Vector3.Lerp(centerStart + left, centerStart + right, seg);
            bool isDuringLC = vehicle.dt_lane_tweening < VehicleController.GetLaneTweeningDuration(vehicle.vehicleType);

            //handle lane changing
            if (isDuringLC)
            {
                if (vehicle.isMerging)
                {
                    //merging from different road
                    float newWidth = path.widthPerLane * (path.lanesCount + 2);
                    right = Vector3.Cross(Vector3.up, dir) * newWidth;
                    left = -right;
                    float oldSeg;
                    if (vehicle.isMergingFromRight)
                    {
                        oldSeg = (2f * (vehicle.oldLane + 1) + 1f) / (2f * (path.lanesCount + 2));
                    }
                    else
                    {
                        oldSeg = (2f * vehicle.currentLane + 1f) / (2f * (path.lanesCount + 2));
                    }
                    Vector3 oldPos = Vector3.Lerp(centerStart + left, centerStart + right, oldSeg);
                    pos = Vector3.Lerp(oldPos, pos, Mathf.SmoothStep(0, 1, vehicle.dt_lane_tweening / VehicleController.GetLaneTweeningDuration(vehicle.vehicleType)));
                }
                else
                {
                    //regular/within road lane chaging
                    float oldSeg = (2f * vehicle.oldLane + 1f) / (2f * path.lanesCount);
                    Vector3 oldPos = Vector3.Lerp(centerStart + left, centerStart + right, oldSeg);
                    pos = Vector3.Lerp(oldPos, pos, Mathf.SmoothStep(0, 1, vehicle.dt_lane_tweening / VehicleController.GetLaneTweeningDuration(vehicle.vehicleType)));
                }
            }
            else
            {
                vehicle.isMerging = false;
            }
            return pos;
        }



        private int GetCompatibleMergeDivergeLane(int currentLane, int[] fromLanes)
        {
            int index = -1;
            for (int i = 0; i < fromLanes.Length; i++)
            {
                if (currentLane == fromLanes[i])
                {
                    index = i;
                    break;
                }
            }
            return index;
        }

        private VehicleController GetLastVehicle(int lane, float curvePos, bool debug = false)
        {
            for (int i = vehicles.Count - 1; i >= 0; i--)
            {
                var vehicle = vehicles[i];
                if (vehicle.currentLane == lane && vehicle.curvePos >= curvePos)
                {
                    if (debug)
                    {
                        Debug.Log($"requested lane: {lane} current lane: {vehicle.currentLane}");
                    }
                    return vehicle;
                }
            }
            return dummyLeader;
        }

        //Sort vehicles in decending order
        private void SortVehicles()
        {
            vehicles.Sort();
        }


        #region EDITOR ONLY
#if UNITY_EDITOR
        public void OnDrawGizmos()
        {
            Color gizmosColor = Gizmos.color;

            pathSpline = path.GetSplineBuilder(true);
            List<int> lanesWithNoMerged = new List<int>(path.lanesCount);
            for (int i = 0; i < path.lanesCount; ++i)
            {
                lanesWithNoMerged.Add(i);
            }
            foreach (Ramp ramp in ramps)
            {
                if (ramp.type == Ramp.RampType.Merged)
                {
                    foreach (int lane in ramp.fromLanes)
                    {
                        lanesWithNoMerged.RemoveAll(item => item == lane);
                        DrawStraightArrow(GetWorldPositionFromLaneIndex(ramp.umax - path.widthPerLane, lane), pathSpline.GetTangentOnPathSegment(ramp.umax - path.widthPerLane), path.widthPerLane * 2);
                    }
                }
                else if (ramp.type == Ramp.RampType.Diverged)
                {
                    foreach (int lane in ramp.fromLanes)
                    {
                        DrawTurnArrow(GetWorldPositionFromLaneIndex(ramp.umax - path.widthPerLane, lane), pathSpline.GetTangentOnPathSegment(ramp.umax - path.widthPerLane), path.widthPerLane * 2, ramp.direction == Ramp.RampDirection.ToRight);
                    }
                }
            }

            //draw lanes with no merging

            Gizmos.color = Color.red;
            foreach (int lane in lanesWithNoMerged)
            {
                Gizmos.DrawSphere(GetWorldPositionFromLaneIndex(path.pathLength, lane), path.widthPerLane);
            }


            //draw obstacle bar
            Matrix4x4 gizmosMatrix = Gizmos.matrix;
            foreach (var obstacle in obstacles)
            {
                Vector3 pos = GetWorldPositionFromLaneIndex(obstacle.u, obstacle.lane);
                Vector3 dir = pathSpline.GetTangentOnPathSegment(obstacle.u);
                Vector3 left = Vector3.Cross(dir, Vector3.up);
                Gizmos.DrawLine(pos + left * path.widthPerLane, pos + (-left * path.widthPerLane));
                Quaternion rotation = Quaternion.LookRotation(dir, Vector3.up);
                Matrix4x4 rotationMat = Matrix4x4.TRS(pos, rotation, transform.lossyScale);
                Gizmos.matrix = rotationMat;
                Gizmos.DrawCube(Vector3.zero, new Vector3(path.widthPerLane * 2, 0, path.widthPerLane));
                //Gizmos.DrawCube(Vector3.zero, Vector3.one * 100);

            }
            Gizmos.matrix = gizmosMatrix;

            //draw partition lanes
            Gizmos.color = Color.yellow;
            foreach (var partition in partitionSegments)
            {
                //start line
                Vector3 pos = GetWorldPositionFromLaneIndex(partition.start, partition.lane);
                Vector3 dir = pathSpline.GetTangentOnPathSegment(partition.start);
                Vector3 left = Vector3.Cross(dir, Vector3.up);
                Gizmos.DrawLine(pos + left * path.widthPerLane, pos + (-left * path.widthPerLane));

                bool drawOnRight = true;
                //draw zigzag lines
                for (float zigzag = partition.start; zigzag <= partition.end; zigzag += path.widthPerLane * 2)
                {

                    Vector3 zigzagStart = GetWorldPositionFromLaneIndex(zigzag, partition.lane);
                    Vector3 zigzagStartDir = pathSpline.GetTangentOnPathSegment(zigzag);
                    Vector3 zigzagStartLeft = Vector3.Cross(zigzagStartDir, Vector3.up);
                    zigzagStartLeft = drawOnRight ? -zigzagStartLeft : zigzagStartLeft;

                    float zigzagEndLength = Mathf.Min(partition.end, zigzag + path.widthPerLane * 2);
                    Vector3 zigzagEnd = GetWorldPositionFromLaneIndex(zigzagEndLength, partition.lane);
                    Vector3 zigzagEndDir = pathSpline.GetTangentOnPathSegment(zigzagEndLength);
                    Vector3 zigzagEndLeft = Vector3.Cross(zigzagEndDir, Vector3.up);
                    zigzagEndLeft = drawOnRight ? zigzagEndLeft : -zigzagEndLeft;

                    Gizmos.DrawLine(zigzagStart + zigzagStartLeft * path.widthPerLane, zigzagEnd + zigzagEndLeft * path.widthPerLane);


                    drawOnRight = !drawOnRight;
                }

                //end line
                pos = GetWorldPositionFromLaneIndex(partition.end, partition.lane);
                dir = pathSpline.GetTangentOnPathSegment(partition.end);
                left = Vector3.Cross(dir, Vector3.up);
                Gizmos.DrawLine(pos + left * path.widthPerLane, pos + (-left * path.widthPerLane));
            }

            //done
            Gizmos.color = gizmosColor;
        }


        public void AddMeshCollider()
        {
            /*
            MeshFilter mf = gameObject.GetComponent<MeshFilter>();
            if (mf == null) {
                mf = gameObject.AddComponent<MeshFilter>();
            }

            MeshRenderer mr = gameObject.GetComponent<MeshRenderer>();
            if (mr == null) {
                mr = gameObject.AddComponent<MeshRenderer>();
            }
            */

            Mesh m = new Mesh();
            m.name = "dummyMesh";
            List<Vector3> nodes = path.nodes;
            var parentPosition = transform.position;
            var width = path.calculatedWidth;

            List<Vector3> vertices = new List<Vector3>();
            List<Vector2> uvs = new List<Vector2>();
            List<int> triangles = new List<int>();

            for (int i = 1; i < nodes.Count - 2; i++)
            {
                var point1 = nodes[i] - parentPosition;
                var point2 = nodes[i + 1] - parentPosition;
                var dir = (point2 - point1).normalized;
                var left = Vector3.Cross(dir, Vector3.up);
                var right = -left;

                var drawnPoint1 = point1 + left * width;
                var drawnPoint2 = point1 + (right * width);
                var drawnPoint3 = point2 + left * width;
                var drawnPoint4 = point2 + (right * width);

                vertices.Add(drawnPoint3);
                vertices.Add(drawnPoint4);
                vertices.Add(drawnPoint2);
                vertices.Add(drawnPoint1);

                uvs.Add(new Vector2(0, 0));
                uvs.Add(new Vector2(0, 1));
                uvs.Add(new Vector2(1, 1));
                uvs.Add(new Vector2(1, 0));
            }

            //one side
            for (int i = 0; i < vertices.Count - 2; i += 4)
            {
                triangles.Add(i);
                triangles.Add(i + 1);
                triangles.Add(i + 2);
                triangles.Add(i);
                triangles.Add(i + 2);
                triangles.Add(i + 3);
            }
            m.vertices = vertices.ToArray();
            m.uv = uvs.ToArray();
            m.triangles = triangles.ToArray();
            //mf.mesh = m;
            m.RecalculateBounds();
            m.RecalculateNormals();

            var mc = gameObject.GetComponent<MeshCollider>();
            if (mc == null)
            {
                mc = gameObject.AddComponent<MeshCollider>();
            }
            mc.sharedMesh = m;
        }

        public void RemoveMeshCollider()
        {
            DestroyImmediate(gameObject.GetComponent<MeshCollider>());
        }


        private static void DrawStraightArrow(Vector3 tailPos, Vector3 forward, float size)
        {
            Color gizmosColor = Gizmos.color;
            Gizmos.color = Color.white;
            float shaftLength = size;
            float arrowHeadLength = shaftLength / 2;

            //draw shaft
            Gizmos.DrawLine(tailPos, tailPos + forward * shaftLength);

            //draw arrowhead
            Vector3 leftDir = Vector3.Cross(forward, Vector3.up).normalized;
            Vector3 arrowHeadLeftEnd = tailPos + (forward * arrowHeadLength) + (leftDir * arrowHeadLength);
            Vector3 arrowHeadRightEnd = tailPos + (forward * arrowHeadLength) + (-leftDir * arrowHeadLength);
            Gizmos.DrawLine(arrowHeadLeftEnd, tailPos + forward * shaftLength);
            Gizmos.DrawLine(arrowHeadRightEnd, tailPos + forward * shaftLength);

            Gizmos.color = gizmosColor;
        }

        private static void DrawTurnArrow(Vector3 tailPos, Vector3 forward, float size, bool toRight)
        {
            Color gizmosColor = Gizmos.color;
            Gizmos.color = Color.white;
            float shaftLength = size;
            float arrowHeadLength = shaftLength / 2;
            float turnShaftLength = arrowHeadLength;

            //draw shaft
            Vector3 shaftTail = tailPos + forward * shaftLength;
            Gizmos.DrawLine(tailPos, shaftTail);

            //draw turn shaft
            Vector3 dir = Vector3.Cross(forward, Vector3.up).normalized;
            dir = toRight ? -dir : dir;
            Vector3 turnShaftTail = shaftTail + dir * turnShaftLength;
            Gizmos.DrawLine(shaftTail, turnShaftTail);

            //draw turnshaft arrow head
            Gizmos.DrawLine(shaftTail + forward * arrowHeadLength, turnShaftTail);
            Gizmos.DrawLine(shaftTail - forward * arrowHeadLength, turnShaftTail);

            Gizmos.color = gizmosColor;
        }



#endif
        #endregion
    }
}
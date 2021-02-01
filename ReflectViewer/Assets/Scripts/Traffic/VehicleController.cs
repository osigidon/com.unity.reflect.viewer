using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CivilFX.TrafficV5
{
    public enum VehicleType
    {
        //real vehicle
        Motorcycle,
        Car,
        Truck,
        Biker,
        Pedestrian,

        //obstacle
        TrafficObstacle,
        RoadObstacle,
        SpeedReduce,
        Yield,

        //shadow
        Shadow //used for lane changing
    }


    public class VehicleController : MonoBehaviour, System.IComparable<VehicleController>
    {
        public enum WheelAxis
        {
            X,
            Y,
            Z
        }


        public struct LastPathControllerInfo
        {
            public TrafficPathController pathController;
            public int oldLane;
            public int currentLane;
            public float offset;
            public void SetInfo(TrafficPathController _pathController, int _oldLane, int _currentLane, float _offset)
            {
                pathController = _pathController;
                oldLane = _oldLane;
                currentLane = _currentLane;
                offset = _offset;
            }
        }

        public float vehicleLength;
        public float frontOffset;
        public float rearOffset;

        /// <summary>
        /// Front end curve position of a vehicle
        /// </summary>
        public float curvePos;
        public int currentLane;
        public int oldLane;
        public float speed;
        public float accelerate;

        //merging
        public bool isMerging;
        public bool isMergingFromRight;

        public VehicleType vehicleType;
        public bool IsVirtual
        {
            get
            {
                return !(vehicleType == VehicleType.Car || vehicleType == VehicleType.Motorcycle || vehicleType == VehicleType.Truck || vehicleType == VehicleType.Biker || vehicleType == VehicleType.Pedestrian);
            }
        }


        public bool allowLeavingPath;
        public float dt_lane_tweening;
        public float timeSinceLastLC;
        public float passiveLCTime;
        public bool shouldDoLC
        {
            get { return (timeSinceLastLC > LC_TIME_LIMIT) && (passiveLCTime > LC_TIME_LIMIT); }
        }

        public bool isTruck
        {
            get { return vehicleType == VehicleType.Truck; }
        }

        public int iLead;
        public int iFollow;
        public int iLeadRight;
        public int iLeadLeft;
        public int iFollowRight;
        public int iFollowLeft;

        //models
        public CarFollowingModel longModel;
        public LaneChangingModel LCModel;

        //external leader
        public VehicleController externalLead;
        public float externalLeadCurvePos;


        //extra references
        public VehicleController yieldingReference;

        public LastPathControllerInfo lastPathControllerInfo;

        //transform
        [SerializeField]
        private Transform vehicleTrans;
        [SerializeField]
        private Transform trailerTrans; //for truck only

        //wheels
        [SerializeField]
        private Transform[] wheels;
        [SerializeField]
        private WheelAxis wheelAxis;
        private float rotationValue;

        public bool debug;

        //static variables
        public static readonly float LANE_TWEENING_DURATION_CAR = 3f;
        public static readonly float LANE_TWEENING_DURATION_TRUCK = 3f;
        public static readonly float LANE_TWEENING_DURATION_MOTORCYCLE = 1.5f;
        public static float GetLaneTweeningDuration(VehicleType type)
        {
            switch (type)
            {
                case VehicleType.Car:
                    return LANE_TWEENING_DURATION_CAR;
                case VehicleType.Truck:
                    return LANE_TWEENING_DURATION_TRUCK;
                case VehicleType.Motorcycle:
                    return LANE_TWEENING_DURATION_MOTORCYCLE;
                default:
                    return LANE_TWEENING_DURATION_CAR;
            }
        }
        public static readonly int EXTERNAL_LEAD_ID = -999;
        public static readonly int INVALID_VEHICLE_ID = -100;
        public static readonly float LC_TIME_LIMIT = 4f;

        /// <summary>
        /// Return true if the "id" is valid
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static bool ValidateInRoadVehicleID(int id)
        {
            return id >= 0;
        }

        public void UpdateDeltaTime(float dt)
        {
            dt_lane_tweening += dt;
            timeSinceLastLC += dt;
            passiveLCTime += dt;
        }
        public void UpdateLastPathController(TrafficPathController pathController, int oldLane, int currentLane, float offset)
        {
            lastPathControllerInfo.SetInfo(pathController, oldLane, currentLane, offset);
        }
        public int CompareTo(VehicleController other)
        {
            return other.curvePos.CompareTo(curvePos);
        }

        public void Renew(float _curvePos, int _lane, float _speed, CarFollowingModel _longModel = null, LaneChangingModel _LCModel = null)
        {
            curvePos = _curvePos;
            currentLane = _lane;
            oldLane = _lane;
            speed = _speed;
            accelerate = 0;

            iLead = INVALID_VEHICLE_ID;
            iFollow = INVALID_VEHICLE_ID;
            iLeadRight = INVALID_VEHICLE_ID;
            iLeadLeft = INVALID_VEHICLE_ID;
            iFollowRight = INVALID_VEHICLE_ID;
            iFollowLeft = INVALID_VEHICLE_ID;

            externalLead = null;
            externalLeadCurvePos = 0;

            yieldingReference = null;
            UpdateLastPathController(null, -1, -1, -1);

            dt_lane_tweening = 0;
            timeSinceLastLC = 0;
            passiveLCTime = 0;

            isMerging = false;
            isMergingFromRight = false;

            if (_longModel != null)
            {
                longModel = _longModel;
            }
            if (_LCModel != null)
            {
                LCModel = _LCModel;
            }
            ReRoute();
        }

        public void ReRoute()
        {
            allowLeavingPath = Random.Range(0, 100) < 50;
        }

        public void SetPosition(in Vector3 pos)
        {
            vehicleTrans.position = pos;
            //rotate wheels
            if (wheels != null)
            {
                foreach (var wheel in wheels)
                {
                    Vector3 angle = Vector3.zero;
                    switch (wheelAxis)
                    {
                        case WheelAxis.X:
                            angle.x = rotationValue;
                            break;
                        case WheelAxis.Y:
                            angle.y = rotationValue;
                            break;
                        case WheelAxis.Z:
                            angle.z = rotationValue;
                            break;
                    }
                    wheel.localEulerAngles = angle;
                    rotationValue -= 90f * (360f / 60f) * 0.002f * speed * 10;
                }
            }
        }

        public void SetVisible(bool visible)
        {
            gameObject.SetActive(visible);
        }

        public void SetRotation(in Quaternion rot)
        {
            vehicleTrans.rotation = rot;
        }

        public void SetRotation(in Vector3 targetPos)
        {
            if ((targetPos - vehicleTrans.position).sqrMagnitude < 0.0001f)
            {
                return;
            }
            var targetRotation = Quaternion.LookRotation(targetPos - vehicleTrans.position, Vector3.up);
            vehicleTrans.rotation = Quaternion.Slerp(vehicleTrans.rotation, targetRotation, Time.deltaTime * 20f);
        }

        public void CorrectTrailerRotation(in Vector3 pos, in Vector3 backPos)
        {
            Vector3 dir = (pos - backPos).normalized;
            trailerTrans.rotation = Quaternion.LookRotation(dir, Vector3.up);
        }
        public void CorrectTrailerRotation(in Quaternion rot)
        {
            trailerTrans.rotation = rot;
        }

        public void Mirror(in VehicleController other)
        {
            curvePos = other.curvePos;
            currentLane = other.currentLane;
            oldLane = other.oldLane;
            isMerging = other.isMerging;
            isMergingFromRight = other.isMergingFromRight;
            dt_lane_tweening = other.dt_lane_tweening;
        }

        private void OnDrawGizmos()
        {
            if (allowLeavingPath)
            {
                Gizmos.DrawWireSphere(transform.position, 2f);
            }
        }
    }
}
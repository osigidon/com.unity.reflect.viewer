using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace CivilFX.TrafficV5
{

    public enum EncourageLCDirection
    {
        None,
        ToRight,
        ToLeft,
    }

    #region Models static class
    public static class Models
    {
        private static List<CarFollowingModel> longModelsCarUrban;
        private static List<CarFollowingModel> longModelsTruckUrban;
        private static List<CarFollowingModel> longModelsMotorcycleUrban;
        private static List<CarFollowingModel> longModelsCarFreeway;
        private static List<CarFollowingModel> longModelsTruckFreeway;
        private static List<CarFollowingModel> longModelsMotorcycleFreeway;


        private static List<LaneChangingModel> LCModelsCar;
        private static List<LaneChangingModel> LCModelsTruck;
        private static List<LaneChangingModel> LCModelsMotorcycle;

        private static CarFollowingModel longModelBiker;
        private static CarFollowingModel longModelPed;

        private static LaneChangingModel LCMandatoryRight;
        private static LaneChangingModel LCMandatoryLeft;

        private static CarFollowingModel virtualLong;
        private static LaneChangingModel virtualLC;

        private static bool isModelsAlreadyBuilt = false;
        private static int MODELS_COUNT = 5;

        public static void BuildModels()
        {
            if (isModelsAlreadyBuilt)
            {
                return;
            }
            Debug.Log("Building Models");
            /*
             * Long Models
             */
            longModelsCarUrban = new List<CarFollowingModel>(MODELS_COUNT);
            longModelsTruckUrban = new List<CarFollowingModel>(MODELS_COUNT);
            longModelsMotorcycleUrban = new List<CarFollowingModel>(MODELS_COUNT);

            longModelsCarFreeway = new List<CarFollowingModel>(MODELS_COUNT);

            //car models
            for (int i = 0; i < MODELS_COUNT; i++)
            {
                float v0 = Random.Range(10f, 17f); //44.7mph to 55.9234mph
                float t = Random.Range(1f, 2f); //1m to 2m
                float s0 = Random.Range(1f, 2f); //gap: 1m to 2s
                float a = Random.Range(4f, 5.3f); //acceleration: 1mps^2 to 2mps^2 (meter per second^2)
                float b = Random.Range(3f, 5f); //comfortable breaking (will be treat as negative)
                ACC longModel = ScriptableObject.CreateInstance<ACC>();
                longModel.SetModel(v0, t, s0, a, b);
                longModelsCarUrban.Add(longModel);
            }

            for (int i = 0; i < MODELS_COUNT; i++)
            {
                float v0 = Random.Range(29f, 35f); //44.7mph to 55.9234mph
                float t = Random.Range(1f, 2f); //1m to 2m
                float s0 = Random.Range(1f, 2f); //gap: 1m to 2s
                float a = Random.Range(4f, 5.3f); //acceleration: 1mps^2 to 2mps^2 (meter per second^2)
                float b = Random.Range(3f, 5f); //comfortable breaking (will be treat as negative)
                ACC longModel = ScriptableObject.CreateInstance<ACC>();
                longModel.SetModel(v0, t, s0, a, b);
                longModelsCarFreeway.Add(longModel);
            }

            //truck models
            for (int i = 0; i < MODELS_COUNT; i++)
            {
                float v0 = Random.Range(15f, 20f); //33.554mph to 44.7mph
                float t = Random.Range(2f, 2.5f); //1m to 2m
                float s0 = Random.Range(1.8f, 2f); //gap: 1m to 2s
                float a = Random.Range(1f, 2f); //acceleration: 1mps^2 to 2mps^2 (meter per second^2)
                float b = Random.Range(2f, 2.5f); //comfortable breaking
                ACC longModel = ScriptableObject.CreateInstance<ACC>();
                longModel.SetModel(v0, t, s0, a, b);
                longModelsTruckUrban.Add(longModel);
            }
            longModelsTruckFreeway = longModelsCarFreeway;

            //motorycle
            for (int i = 0; i < MODELS_COUNT; i++)
            {
                float v0 = Random.Range(10f, 17f);
                float t = Random.Range(0.5f, 1f);
                float s0 = Random.Range(0.5f, 1f);
                float a = Random.Range(5f, 6f); //acceleration: 1mps^2 to 2mps^2 (meter per second^2)
                float b = Random.Range(5f, 6f); //comfortable breaking
                ACC longModel = ScriptableObject.CreateInstance<ACC>();
                longModel.SetModel(v0, t, s0, a, b);
                longModelsMotorcycleUrban.Add(longModel);
            }
            longModelsMotorcycleFreeway = longModelsCarFreeway;

            longModelBiker = longModelsCarUrban[0].Clone();
            longModelBiker.v0 = 6.7f; //15mph

            longModelPed = longModelBiker.Clone();
            longModelPed.v0 = 1.341f; //3mph




            /*
             * Lane Changing Models
             */
            //cars
            LCModelsCar = new List<LaneChangingModel>(MODELS_COUNT);
            LCModelsTruck = new List<LaneChangingModel>(MODELS_COUNT);
            LCModelsMotorcycle = new List<LaneChangingModel>(MODELS_COUNT);

            //cars
            for (int i = 0; i < MODELS_COUNT; i++)
            {
                float bSafe = Random.Range(4f, 4f);
                float bSafeMax = Random.Range(8f, 8f);
                var p = Random.Range(-0.2f, 0.5f); //lower = more changing default: -0.2 - 1
                var bThr = Random.Range(-.1f, 0.2f); //lower = more changing also

                var LCModel = ScriptableObject.CreateInstance<MOBIL>();
                LCModel.name = "LC " + i.ToString();
                LCModel.SetModel(bSafe, bSafeMax, p, bThr);
                LCModelsCar.Add(LCModel);
            }

            //trucks
            for (int i = 0; i < MODELS_COUNT; i++)
            {
                float bSafe = Random.Range(1.5f, 1.5f);
                float bSafeMax = Random.Range(8f, 8f);
                var p = Random.Range(0, 0.5f); //lower = more changing 
                var bThr = Random.Range(0f, 0.2f); //lower = more changing also

                var LCModel = ScriptableObject.CreateInstance<MOBIL>();
                LCModel.name = "LC " + i.ToString();
                LCModel.SetModel(bSafe, bSafeMax, p, bThr);
                LCModelsTruck.Add(LCModel);
            }

            //motorcycles
            for (int i = 0; i < MODELS_COUNT; i++)
            {
                float bSafe = Random.Range(4f, 4f);
                float bSafeMax = Random.Range(8f, 8f);
                var p = Random.Range(-0.2f, 0.5f); //lower = more changing 
                var bThr = Random.Range(-0.1f, 0.5f); //lower = more changing also

                var LCModel = ScriptableObject.CreateInstance<MOBIL>();
                LCModel.name = "LC " + i.ToString();
                LCModel.SetModel(bSafe, bSafeMax, p, bThr);
                LCModelsMotorcycle.Add(LCModel);
            }

            //mandatory models
            MOBIL LCRight = ScriptableObject.CreateInstance<MOBIL>();
            LCRight.SetModel(15f, 20f, -0.2f, 0, 30f);
            LCMandatoryRight = LCRight;

            MOBIL LCLeft = ScriptableObject.CreateInstance<MOBIL>();
            LCLeft.SetModel(15f, 20f, -0.2f, 0, -30f);
            LCMandatoryLeft = LCLeft;

            //virtual models
            ACC newVirtualLong = ScriptableObject.CreateInstance<ACC>();
            newVirtualLong.SetModel(0, 1.4f, 2, 0, 3);
            virtualLong = newVirtualLong;

            MOBIL newVirtualLC = ScriptableObject.CreateInstance<MOBIL>();
            newVirtualLC.SetModel(4, 20, 0.1f, 0.2f, 0.3f);
            virtualLC = newVirtualLC;

            isModelsAlreadyBuilt = true;
        }


        public static CarFollowingModel GetLongModel(VehicleType vehicleType = VehicleType.Car, PathType pathType = PathType.Urban)
        {
            if (!isModelsAlreadyBuilt)
            {
                BuildModels();
            }
            switch (vehicleType)
            {
                case VehicleType.Car:
                    if (pathType == PathType.Urban)
                    {
                        return longModelsCarUrban[Random.Range(0, longModelsCarUrban.Count)];
                    }
                    else
                    {
                        return longModelsCarFreeway[Random.Range(0, longModelsCarFreeway.Count)];
                    }
                case VehicleType.Truck:
                    if (pathType == PathType.Urban)
                    {
                        return longModelsTruckUrban[Random.Range(0, longModelsTruckUrban.Count)];
                    }
                    else
                    {
                        return longModelsTruckFreeway[Random.Range(0, longModelsTruckFreeway.Count)];
                    }
                case VehicleType.Motorcycle:
                    if (pathType == PathType.Urban)
                    {
                        return longModelsMotorcycleUrban[Random.Range(0, longModelsMotorcycleUrban.Count)];
                    }
                    else
                    {
                        return longModelsMotorcycleFreeway[Random.Range(0, longModelsMotorcycleFreeway.Count)];
                    }
                case VehicleType.Biker:
                    return longModelBiker;
                case VehicleType.Pedestrian:
                    return longModelPed;
                default:
                    return null;
            }
        }

        public static LaneChangingModel GetLCModel(VehicleType vehicleType = VehicleType.Car)
        {
            if (!isModelsAlreadyBuilt)
            {
                BuildModels();
            }
            switch (vehicleType)
            {
                case VehicleType.Car:
                    return LCModelsCar[Random.Range(0, LCModelsCar.Count)];
                case VehicleType.Truck:
                    return LCModelsTruck[Random.Range(0, LCModelsTruck.Count)];
                case VehicleType.Motorcycle:
                    return LCModelsMotorcycle[Random.Range(0, LCModelsMotorcycle.Count)];
                default:
                    return LCModelsMotorcycle[Random.Range(0, LCModelsCar.Count)]; //fix for ped and biker
                    //return null;
            }
        }


        public static LaneChangingModel GetMandatoryLCModel(bool toRight)
        {
            if (!isModelsAlreadyBuilt)
            {
                BuildModels();
            }
            return toRight ? LCMandatoryRight : LCMandatoryLeft;
        }

        public static CarFollowingModel GetVirtualLongModel()
        {
            if (!isModelsAlreadyBuilt)
            {
                BuildModels();
            }
            return virtualLong;
        }

        public static LaneChangingModel GetVirtualLCModel()
        {
            if (!isModelsAlreadyBuilt)
            {
                BuildModels();
            }
            return virtualLC;
        }

    }
    #endregion



    public class TrafficController : MonoBehaviour
    {



        public GameObject[] prefabs;
        public GameObject[] carPrefabs;
        public GameObject[] motorcyclePrefabs;
        public GameObject[] truckPrefabs;

        public TrafficPathController[] allPathControllers;

        private List<VehicleController> waitingVehicles;

        void Awake()
        {
            Models.BuildModels();
            Debug.Log(Random.Range(1.0f, 1.0f));

            List<GameObject> temp = new List<GameObject>();
            temp.AddRange(carPrefabs);
            temp.AddRange(motorcyclePrefabs);
            temp.AddRange(truckPrefabs);
            prefabs = temp.ToArray();


            waitingVehicles = new List<VehicleController>();
            foreach (var pathController in allPathControllers)
            {
                var isUrban = pathController.path.pathType == PathType.Urban;
                int initCount = Mathf.CeilToInt(pathController.path.pathLength / 50);
                initCount = pathController.initialVehiclesCount;
                //pathController.Init(prefabs, pathController.initialVehiclesCount, waitingVehicles);
                pathController.Init(prefabs, initCount, waitingVehicles);
            }

            //allocate reserved vehicles
            for (int i = 0; i < 0; i++)
            {
                var go = GameObject.Instantiate(prefabs[UnityEngine.Random.Range(0, prefabs.Length)]);
                var vehicleController = go.GetComponent<VehicleController>();
                vehicleController.Renew(0, 0, 0, Models.GetLongModel(vehicleController.vehicleType, PathType.Urban), Models.GetLCModel(vehicleController.vehicleType));
                go.transform.SetParent(transform);
                waitingVehicles.Add(vehicleController);
            }
        }

        private void Update()
        {
            var dt = Time.deltaTime;
            foreach (var pathController in allPathControllers)
            {

                foreach (var ramp in pathController.ramps)
                {
                    if (ramp.type == Ramp.RampType.OnRamp)
                    {
                        pathController.SetLCMandatory(ramp.umin, ramp.umax, ramp.direction == Ramp.RampDirection.ToRight);
                    }
                }

                //calculate acceleration
                pathController.CalcAcceleration();

                //update delta time for some variables
                pathController.UpdateGeneralDeltaTime(dt);

                //changing lane
                pathController.ChangeLane();

                //update curve position
                pathController.UpdateCurvePosition(dt);

                pathController.DespawnVehicles(waitingVehicles);
                pathController.RespawnVehicles(waitingVehicles, dt);

                //merge
                foreach (var ramp in pathController.ramps)
                {
                    if (ramp.type == Ramp.RampType.Merged || ramp.type == Ramp.RampType.Diverged)
                    {
                        pathController.MergeDiverge(ramp);
                    }
                    else
                    {
                        pathController.OnRampOffRamp(ramp);
                    }
                }

                //update world position
                pathController.UpdateWorldPosition();

            }
        }

    }
}
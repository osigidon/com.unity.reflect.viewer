using CivilFX.Generic2;
using CivilFX.TrafficV5;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace CivilFX.UI2
{
    public class SettingsPanelController : MainPanelController
    {
        [Header("Camera Panel:")]
        public Slider rotation;
        public Slider flythrough;
        public Slider height;
        public Slider fov;
        public CustomButton resetFOV;
        public CameraDirector cameraDirector;

        [Space()]
        [Header("Traffic Panel:")]
        public Toggle modifyInflow;
        public Slider simSpeed;
        public CustomButton resetTraffic;

        [Space()]
        [Header("Others Panel:")]
        public Toggle compassToggle;
        GameObject compass;
        private bool compassOff = false;


        void Start()
        {
            compass = GameObject.FindGameObjectWithTag("Compass");
        }


        private void Awake()
        {
            var camController = GameManager.Instance.cameraController;

            /*
            * Camera
            */
            rotation.onValueChanged.AddListener((v) => {
                camController.SetRotationSpeed(v);
            });

            flythrough.onValueChanged.AddListener((v) => {
                camController.SetFlythroughSpeed(v);
            });

            height.onValueChanged.AddListener((v) => {
                cameraDirector.SetHeightOffset(v - 60);
            });

            fov.value = camController.ResetFOV();
            fov.onValueChanged.AddListener((v) => {
                camController.SetFOV(v);
            });
            resetFOV.RegisterMainButtonCallback(() => {
                fov.value = camController.ResetFOV();
                resetFOV.RestoreInternalState();
            });

            /*
             * Traffic
             */
            modifyInflow.onValueChanged.AddListener((v) => {
                foreach (var trafficController in Resources.FindObjectsOfTypeAll<TrafficController>()) {
                    //trafficController.SetRuntimeVisualInflowCount(v);
                }
            });
            simSpeed.onValueChanged.AddListener((v) => {
                Time.timeScale = v;
            });
            resetTraffic.RegisterMainButtonCallback(() => {
                foreach (var trafficController in Resources.FindObjectsOfTypeAll<TrafficController>()) {
                    //trafficController.ResetSimulation();
                    resetTraffic.RestoreInternalState();
                }              
            });


            /*
            * Others
               */
            compassToggle.onValueChanged.AddListener(delegate {
                ToggleCompass();
            });
        }


        void ToggleCompass()
        {
            compass.SetActive(compassOff);

            compassOff = !compassOff;
        }
    }
}

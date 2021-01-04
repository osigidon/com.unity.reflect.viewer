using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace CivilFX.UI2 {
    public class ExportCustomVideoController : MonoBehaviour
    {
        public TMP_InputField widthInput;
        public TMP_InputField heightInput;
        public TMP_InputField bitrateInput;
        public CustomButton export;
        public CustomButton stop;
        public ExportVideoController exportVideoController;

        // Start is called before the first frame update
        void Awake()
        {
            export.RegisterMainButtonCallback(() => { 
                if (int.TryParse(widthInput.text, out int width)
                    && int.TryParse(heightInput.text, out int height)
                    && int.TryParse(bitrateInput.text, out int bitrate)) {
                    Debug.Log($"Exporting {width}x{height}@{bitrate}");


                    exportVideoController.StartRecording(ExportVideoController.MovieType.Custom, width, height, bitrate);
                    exportVideoController.OnBeginRecording();
                    stop.gameObject.SetActive(true);
                    export.RestoreInternalState();
                    export.gameObject.SetActive(false);
                }

            });


            stop.RegisterMainButtonCallback(() => {
                exportVideoController.OnEndRecording();
                export.gameObject.SetActive(true);
                stop.RestoreInternalState();
                stop.gameObject.SetActive(false);
            });

        }


        public void SetActive(bool active)
        {
            gameObject.SetActive(active);
        }
    }



}
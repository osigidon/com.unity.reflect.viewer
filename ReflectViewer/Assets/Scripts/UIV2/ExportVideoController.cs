using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UTJ.FrameCapturer;
using CivilFX.Generic2;
using UnityEngine.UI;
using System;
using System.IO;

namespace CivilFX.UI2
{
    public class ExportVideoController : MonoBehaviour, IRecorderListener
    {

        public enum MovieType
        {
            FourK,
            TenEightyP,
            Custom
        }

        public CustomButton FourK;
        public CustomButton TenEightyP;
        public CustomButton Custom;
        public CustomButton stop;
        public TextMeshProUGUI infoText;

        [Header("Preview:")]
        public RawImage previewSurface;

        [Header("Float:")]
        public GameObject recorderCallbackObject;
        private IRecorderListener recorderListener;

        [Header("Export Custom")]
        public GameObject exportCustomPanel;

        private CustomButton lastSelectedButton;
        private Camera mainCamera;
        private MovieRecorder movieRecorder;
        private Coroutine updateInfoRoutine;


        void Awake()
        {
            infoText.text = "";
            stop.gameObject.SetActive(false);
            movieRecorder = GameManager.Instance.movieRecorder;
            mainCamera = GameManager.Instance.mainCamera;
            if (movieRecorder == null) {
                Debug.LogError("Failed to find \"MovieRecorder\" script");
                this.gameObject.SetActive(false);
                return;
            }
            recorderListener = recorderCallbackObject.GetComponent<IRecorderListener>();

            FourK.RegisterMainButtonCallback(() => { 
                //
                if (lastSelectedButton != null) {
                    lastSelectedButton.RestoreInternalState();
                }
                lastSelectedButton = FourK;

                //interactable
                FourK.SetInteractable(false);

                //enable stop button
                stop.gameObject.SetActive(true);
                stop.RestoreInternalState();

                //hide other buttons
                TenEightyP.gameObject.SetActive(false);
                Custom.gameObject.SetActive(false);
                exportCustomPanel.SetActive(false);

                //start recording
                StartRecording(MovieType.FourK);
            });

            //2k button
            TenEightyP.RegisterMainButtonCallback(() => {
                //
                if (lastSelectedButton != null) {
                    lastSelectedButton.RestoreInternalState();
                }
                lastSelectedButton = TenEightyP;

                //interactable
                TenEightyP.SetInteractable(false);

                //enable stop button
                stop.gameObject.SetActive(true);
                stop.RestoreInternalState();

                //hide other buttons
                FourK.gameObject.SetActive(false);
                Custom.gameObject.SetActive(false);
                exportCustomPanel.SetActive(false);

                //start recording
                StartRecording(MovieType.TenEightyP);
            });

            //1080p button
            Custom.RegisterMainButtonCallback(() => {
                //
                if (lastSelectedButton != null) {
                    lastSelectedButton.RestoreInternalState();
                }
                if (lastSelectedButton == Custom) {
                    lastSelectedButton = null;
                    exportCustomPanel.SetActive(false);
                } else {
                    lastSelectedButton = Custom;
                    exportCustomPanel.SetActive(true);
                }
            });

            //stop button
            stop.RegisterMainButtonCallback(() => {
                //reset main camera
                mainCamera.targetTexture = null;

                //reset movie recorder
                movieRecorder.targetRT = null;
                movieRecorder.EndRecording();
                //reset preview
                previewSurface.texture = null;
                previewSurface.enabled = false;

                //
                if (lastSelectedButton != null && lastSelectedButton != Custom) {
                    lastSelectedButton.SetInteractable(true);
                    lastSelectedButton.RestoreInternalState();
                    lastSelectedButton = null;
                }

                //stop routine
                UpdateRecordingInfo(false);

                //enable all buttons
                FourK.gameObject.SetActive(true);
                TenEightyP.gameObject.SetActive(true);
                Custom.gameObject.SetActive(true);
                recorderListener.OnEndRecording();

                //open folder
                OpenSavedFolder();

                //hide stop button
                stop.gameObject.SetActive(false);
            });
        }

        public void StartRecording(MovieType type, int customWidth=0, int customHeight=0, int customBitrate=0)
        {
            recorderListener.OnBeginRecording();
            RenderTexture rt = null;
            int bitrate = 0;
            switch (type) {
                case MovieType.FourK:
                    rt = new RenderTexture(3840, 2160, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
                    bitrate = 100000000;              
                    break;
                case MovieType.TenEightyP:
                    rt = new RenderTexture(1920, 1080, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
                    bitrate = 25000000;
                    break;
                case MovieType.Custom:
                    rt = new RenderTexture(customWidth, customHeight, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
                    bitrate = customBitrate;
                    break;
            }
            rt.antiAliasing = 8;
            rt.wrapMode = TextureWrapMode.Clamp;
            rt.filterMode = FilterMode.Bilinear;

            //set camera texture
            mainCamera.targetTexture = rt;

            movieRecorder.enabled = true;
            movieRecorder.targetRT = rt;
            movieRecorder.m_encoderConfigs.mp4EncoderSettings.videoTargetBitrate = bitrate;

            //set preview
            previewSurface.enabled = true;
            previewSurface.texture = rt;

            //record
            movieRecorder.BeginRecording();

            //update text info
            UpdateRecordingInfo(true);
        }

        public void UpdateRecordingInfo(bool enable)
        {
            if (enable) {
                if (updateInfoRoutine != null) {
                    StopCoroutine(updateInfoRoutine);
                }
                updateInfoRoutine = StartCoroutine(UpdateFrameInfoRoutine());
            } else {
                if (updateInfoRoutine != null) {
                    StopCoroutine(updateInfoRoutine);
                    updateInfoRoutine = null;
                }
                infoText.text = "";
            }
        }
        private IEnumerator UpdateFrameInfoRoutine()
        {
            var fps = movieRecorder.targetFramerate;

            while (true) {
                var frames = movieRecorder.GetRecordedFrame();
                TimeSpan t = TimeSpan.FromSeconds((float)frames / fps);
                infoText.text = $"Recorded: {movieRecorder.GetRecordedFrame()} frames. Duration: {t.ToString(@"mm\:ss\:fff")}";
                yield return null;
            }
        }

        private void OpenSavedFolder()
        {
            var savedPath = Path.GetFullPath(string.Format(@"{0}/", "Capture"));
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo() {
                FileName = savedPath,
                UseShellExecute = true,
                Verb = "open"
            });
        }

        private void OnEnable()
        {
            if (movieRecorder.isRecording) {
                UpdateRecordingInfo(true);
            }
        }

        private void OnDisable()
        {
            UpdateRecordingInfo(false);
        }

        public void OnBeginRecording()
        {
            recorderListener.OnBeginRecording();
            FourK.gameObject.SetActive(false);
            TenEightyP.gameObject.SetActive(false);
            Custom.SetInteractable(false);
        }

        public void OnEndRecording()
        {
            recorderListener.OnEndRecording();
            FourK.gameObject.SetActive(true);
            TenEightyP.gameObject.SetActive(true);
            Custom.SetInteractable(true);
            stop.InvokeMainButton();
        }
    }
}
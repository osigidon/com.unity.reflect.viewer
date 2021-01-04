using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using CivilFX.Generic2;
using System.Runtime.Serialization.Formatters.Binary;
using System;
using System.Linq;

namespace CivilFX.UI2
{
    public class NavPanelController : MainPanelController
    {
        #region animated helper classes
        [System.Serializable]
        public class AnimatedCameraAsset
        {
            public PhaseType phase;
            public AnimatedCameraPathData[] cameraDatas;
        }
        #endregion

        #region still helper classes
        [System.Serializable]
        public class StillNode
        {
            public StillNode(string name, Vector3 pos, Vector3 rot, float _fov)
            {
                cameraName = name;
                position = pos;
                rotation = rot;
                fov = _fov;
            }
            public string cameraName;
            public SerializableVector3 position;
            public SerializableVector3 rotation;
            public float fov;
            public override string ToString()
            {
                return cameraName + ":" + position + ":" + rotation + ":" + fov;
            }
        }
        [System.Serializable]
        public class StillNodeHandler : IEnumerable<StillNode>
        {
            public List<StillNode> stillNodes = new List<StillNode>();
            public int Count {
                get {
                    return stillNodes.Count;
                }
            }
            //overload array accessor
            public StillNode this[int index] {
                get { return stillNodes[index]; }
            }
            public void Add(StillNode node)
            {
                stillNodes.Add(node);
            }
            public void Remove(int index)
            {
                stillNodes.RemoveAt(index);
            }

            public void RemoveAll()
            {
                stillNodes.Clear();
            }

            public IEnumerator<StillNode> GetEnumerator()
            {
                foreach (var node in stillNodes) {
                    yield return node;
                }
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }
        #endregion


        [Header("Animated Cameras")]
        public Button animatedCameraAdderButton;
        public GameObject animatedButtonPrefab;
        public RectTransform animatedCamerasContent;
        public Image animatedTopIndicatior;
        public Image animatedBottomIndicatior;
        public AnimatedCameraPanelController animatedCameraPanelController;
        private List<GameObject> animatedCameraPrefabs;
        [Space()]
        public PhaseType defaultPhase;
        public AnimatedCameraAsset[] cameraAssets;
        private Coroutine checkAnimatedRoutine;

        [Header("Still Cameras")]
        public Button stillCameraAdderButton;
        public GameObject stillButtonPrefab;
        public RectTransform stillCamerasContent;
        public Image stillTopIndicator;
        public Image stillBottomIndicator;
        private List<GameObject> stillCamerasPrefabs;
        private int initialStillCamerasCount;
        private CustomButton lastSelectedStillButton;
        private Coroutine checkStillRoutine;

        [Header("Others")]
        public NotificationPanelController notificationPanelController;
        public CameraDirector cameraDirector;
        public CheveronController cheveronController;

        //private variables
        private DirectoryInfo folder;
        private string stillConfig;
        private StillNodeHandler stillNodeHandler;

        //indicator color
        private Color indicatorColor = Color.white;
        private float alpha = 1.1f;
        private float step;

        //
        private new Camera camera;

        //pathway
        private CustomButton lastSelectedPreviewButton;

        public void Awake()
        {
            stillCamerasPrefabs = new List<GameObject>();
            animatedCameraPrefabs = new List<GameObject>();
            initialStillCamerasCount = stillCamerasContent.childCount;
            camera = GameManager.Instance.mainCamera;
            stillNodeHandler = new StillNodeHandler();
            //create data folder
            folder = Directory.CreateDirectory(System.IO.Path.GetFullPath(string.Format(@"{0}/", "Data")));
            stillConfig = "stillConfig.cfx";


            stillCameraAdderButton.onClick.AddListener(() => {
                if (notificationPanelController.isShowing) {
                    return;
                }
                notificationPanelController.SetTitle("Add Still Camera");
                notificationPanelController.SetInputHint("Enter Camera Name...");
                notificationPanelController.RegisterConfirmCallback((inputText) => {
                    stillNodeHandler.Add(new StillNode(inputText, camera.transform.position, camera.transform.eulerAngles, camera.fieldOfView));
                    SaveStillsToDisk();
                    DrawStillCameraPanel();
                });
                notificationPanelController.Show();
            });

            //draw still camera
            DrawStillCameraPanel();

            //draw animated camera
            DrawAnimatedCameraPanel(defaultPhase);

            //set callback when chainging phase
            PhasedManager.RegisterCallback(DrawAnimatedCameraPanel);

            //cleaning up some stuffs
            GameManager.Instance.cameraController.onUnHookView += delegate () {
                if (lastSelectedStillButton != null) {
                    lastSelectedStillButton.RestoreInternalState();
                    lastSelectedStillButton = null;
                }
                cameraDirector.StopCamera();
                animatedCameraPanelController.gameObject.SetActive(false);
            };
        }


        private void DrawStillCameraPanel()
        {
            //Debug.Log("Drawing Still CameraPanel");
            //reading from file
            try {
                var stillConfigStream = File.Open(Path.Combine(folder.FullName, stillConfig), FileMode.Open);
                var bin = new BinaryFormatter();
                stillNodeHandler = (StillNodeHandler)bin.Deserialize(stillConfigStream);
                stillConfigStream.Close();
            } catch (Exception) {; }


            //Debug.Log(stillNodeHandler.Count.ToString() + " dynamic cameras");
            var dynamicCamerasCount = stillNodeHandler.Count;

            var staticCameras = Resources.FindObjectsOfTypeAll<CameraNode>();
            var staticCamerasCount = staticCameras.Length;
            //Debug.Log(staticCamerasCount + " static cameras");

            //create extra objects if need to
            while (stillCamerasPrefabs.Count < dynamicCamerasCount + staticCamerasCount) {
                GameObject go = GameObject.Instantiate(stillButtonPrefab);
                //anchor it somewhere
                go.transform.SetParent(stillCamerasContent.parent.parent, false);
                stillCamerasPrefabs.Add(go);
            }

            //hide visibility
            foreach (var obj in stillCamerasPrefabs) {
                obj.transform.SetParent(stillCamerasContent.parent.parent, false);
                obj.SetActive(false);
            }

            int currentObjIndex = 0;
            //draw dynamic cameras
            for (int i = dynamicCamerasCount - 1; i >= 0; i--) {
                int currentIndex = i;
                var obj = stillCamerasPrefabs[currentObjIndex];
                obj.transform.SetParent(stillCamerasContent, false);
                obj.SetActive(true);
                var buttonScript = obj.GetComponent<CustomButton>();
                buttonScript.ShowSecondaryButton(true);
                buttonScript.SetTitle(stillNodeHandler[i].cameraName);
                buttonScript.RegisterMainButtonCallback(() => {
                    if (buttonScript == lastSelectedStillButton) {
                        //do nothing
                        return;
                    }
                    GameManager.Instance.cameraController.HookView(stillNodeHandler[currentIndex].position, stillNodeHandler[currentIndex].rotation, stillNodeHandler[currentIndex].fov);
                    animatedCameraPanelController.gameObject.SetActive(false);
                    if (lastSelectedStillButton != null) {
                        lastSelectedStillButton.RestoreInternalState();
                    }
                    lastSelectedStillButton = buttonScript;
                });
                buttonScript.RegisterSecondaryButtonCallback(() => {
                    stillNodeHandler.Remove(currentIndex);
                    SaveStillsToDisk();
                    DrawStillCameraPanel();
                });
                ++currentObjIndex;
            }

            //reorder static cameras
            staticCameras = staticCameras.OrderBy(x => x.order).ToArray();
            //draw static cameras
            for (int i = 0; i < staticCameras.Length; i++) {
                //Debug.Log($"name: {staticCameras[i].name} order: {staticCameras[i].order}");
                int currentIndex = i;
                var obj = stillCamerasPrefabs[currentObjIndex];
                obj.transform.SetParent(stillCamerasContent, false);
                obj.SetActive(true);
                var buttonScript = obj.GetComponent<CustomButton>();
                buttonScript.ShowSecondaryButton(false);
                buttonScript.SetTitle(staticCameras[i].gameObject.name);
                buttonScript.RegisterMainButtonCallback(() => {
                    if (buttonScript == lastSelectedStillButton) {
                        //do nothing
                        return;
                    }
                    GameManager.Instance.cameraController.HookView(staticCameras[currentIndex].gameObject.transform);
                    animatedCameraPanelController.gameObject.SetActive(false);
                    if (lastSelectedStillButton != null) {
                        lastSelectedStillButton.RestoreInternalState();
                    }
                    lastSelectedStillButton = buttonScript;
                });
                ++currentObjIndex;
            }
            if (checkStillRoutine != null) {
                StopCoroutine(checkStillRoutine);
            }
            checkStillRoutine = StartCoroutine(CheckVisibleRoutine(stillCamerasContent.GetComponent<RectTransform>(), stillTopIndicator, stillBottomIndicator));
        }

        private void DrawAnimatedCameraPanel(PhaseType phase)
        {
            if (!Application.isPlaying) {
                return;
            }
            //draw static cameras

            //draw dynamic cameras
            AnimatedCameraAsset cameraAsset = null;
            foreach (var asset in cameraAssets) {
                if (asset.phase == phase) {
                    cameraAsset = asset;
                }
            }

            foreach (var button in animatedCameraPrefabs) {
                button.SetActive(false);
            }

            if (cameraAsset == null) {
                return;
            }

            //create button if needed
            while (animatedCameraPrefabs.Count < cameraAsset.cameraDatas.Length) {
                var go = GameObject.Instantiate(animatedButtonPrefab);
                go.transform.SetParent(animatedCamerasContent, false);
                go.SetActive(false);
                animatedCameraPrefabs.Add(go);
            }

            for (int i = 0; i < cameraAsset.cameraDatas.Length; i++) {
                int j = i;
                animatedCameraPrefabs[i].SetActive(true);
                var buttonScript = animatedCameraPrefabs[i].GetComponent<AnimatedCustomButton>();
                buttonScript.SetTitle(cameraAsset.cameraDatas[i].name);
                buttonScript.ShowSecondaryButton(false);
                if (cameraAsset.cameraDatas[j].showPathway) {
                    buttonScript.ShowThirdButton(true);
                    buttonScript.RegisterMainButtonCallback(() => {
                        var pos = new Vector3(221.6129f, 1521.646f, 202.1553f);
                        var angle = new Vector3(68.103f, 59.061f, 0f);
                        var fov = 50;
                        GameManager.Instance.cameraController.HookView(pos, angle, fov);

                        if (lastSelectedStillButton != null) {
                            lastSelectedStillButton.RestoreInternalState();
                        }
                        lastSelectedStillButton = buttonScript;
                    });
                    buttonScript.RegisterThirdButtonCallback(() => {
                        if (lastSelectedPreviewButton == null || lastSelectedPreviewButton != buttonScript.thirdButton) {
                            if (lastSelectedPreviewButton != null) {
                                lastSelectedPreviewButton.RestoreInternalState();
                            }
                            cheveronController.StartCheveron(cameraAsset.cameraDatas[j]);
                            lastSelectedPreviewButton = buttonScript.thirdButton;
                        } else {
                            if (cheveronController.isPlaying) {
                                lastSelectedPreviewButton.RestoreInternalState();
                                cheveronController.Stop();
                                lastSelectedPreviewButton = null;
                            }
                        }
                    });


                } else {
                    buttonScript.ShowThirdButton(false);
                    buttonScript.RegisterMainButtonCallback(() => {
                        if (buttonScript == lastSelectedStillButton) {
                            //do nothing
                            return;
                        }
                        cameraDirector.MoveCamera(cameraAsset.cameraDatas[j]);
                        animatedCameraPanelController.gameObject.SetActive(true);
                        animatedCameraPanelController.RestoreInternalImages();
                        if (lastSelectedStillButton != null) {
                            lastSelectedStillButton.RestoreInternalState();
                        }
                        lastSelectedStillButton = buttonScript;
                    });
                }
            }

            //check visible
            if (checkAnimatedRoutine != null) {
                StopCoroutine(checkAnimatedRoutine);
            }
            checkAnimatedRoutine = StartCoroutine(CheckVisibleRoutine(animatedCamerasContent.GetComponent<RectTransform>(), animatedTopIndicatior, animatedBottomIndicatior));
        }

        private void SaveStillsToDisk()
        {
            var stillConfigStream = File.Open(Path.Combine(folder.FullName, stillConfig), FileMode.Create);
            var bin = new BinaryFormatter();
            bin.Serialize(stillConfigStream, stillNodeHandler);
            stillConfigStream.Close();
        }
        private IEnumerator CheckVisibleRoutine(RectTransform rt, Image up, Image down)
        {
            yield return null;
            yield return null;
            float containerHeight = rt.sizeDelta.y;
            float containerPos;
            float parentHeight = ((RectTransform)rt.parent).sizeDelta.y;

            while (true) {

                //animated
                containerPos = rt.anchoredPosition.y;

                //Debug.Log($"stillContainerHeight {containerHeight}, stillContainerPos {containerPos}, stillParentHeight{parentHeight}");

                //first not seen
                if (containerPos.Compare(0)) {
                    up.enabled = false;
                } else {
                    up.enabled = true;
                }
                //last seen
                if (containerHeight >= parentHeight && !parentHeight.Compare(containerHeight - containerPos)) {
                    down.enabled = true;
                } else {
                    down.enabled = false;
                }

                if (alpha >= 0.75f) {
                    step = -0.01f;
                } else if (alpha <= 0.25f) {
                    step = 0.01f;
                }
                alpha += step;
                indicatorColor.a = alpha;
                up.color = indicatorColor;
                down.color = indicatorColor;


                yield return null;
            }
        }


        private void OnEnable()
        {
            if (checkStillRoutine != null) {
                StopCoroutine(checkStillRoutine);
            }
            checkStillRoutine = StartCoroutine(CheckVisibleRoutine(stillCamerasContent.GetComponent<RectTransform>(), stillTopIndicator, stillBottomIndicator));

            if (checkAnimatedRoutine != null) {
                StopCoroutine(checkAnimatedRoutine);
            }
            checkAnimatedRoutine = StartCoroutine(CheckVisibleRoutine(animatedCamerasContent.GetComponent<RectTransform>(), animatedTopIndicatior, animatedBottomIndicatior));
        }

        private void OnDisable()
        {
            if (checkStillRoutine != null) {
                StopCoroutine(checkStillRoutine);
                checkStillRoutine = null;
            }

            if (checkAnimatedRoutine != null) {
                StopCoroutine(checkAnimatedRoutine);
                checkAnimatedRoutine = null;
            }

        }
    }
}
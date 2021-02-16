using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UTJ.FrameCapturer;

namespace CivilFX.Generic2
{
    [RequireComponent(typeof(Camera))]
    public class CameraController : MonoBehaviour
    {
        public enum CameraState
        {
            Default,
            MouseZoomIn,
            MouseZoomOut,
            TouchZoomIn,
            TouchZoomOut,
            MovingForward,
            MovingBackward,
            Reset,
            Stop,
        }



        //bounding box
        [Header("Bounding Box")]
        public bool useBoundingBox;
        public Vector3 boxSize;
        public Vector3 boxCenter;
        private Vector3 defaultPos;

        //callback when unhook view
        public Action onUnHookView;


        //mouse input variables
        private float mouseZoomSpeed = 50f;
        private Vector3 lastMouseDownPos;
        private float panSpeed = 0.5f;
        private float zAxis;
        private float rotationSpeed = 0.1f;
        private float turnSpeed = 1.0f;

        //touch input variables
        private List<Touch> touchList = new List<Touch>();
        private Vector3 panOrigin;       // Position of cursor when mouse dragging starts
        private Vector3 rotateOrigin;    // Position of cursor when mouse dragging starts
        private Vector3 zoomOrigin;      // Position of cursor when mouse dragging starts
        private float pinchOrigin;       // Original Distance between the two touches

        private float pinchSpeed = 10f;
        private Vector3 panLastPos;      // Position of cursor when mouse dragging starts
        private Vector3 rotateLastPos;   // Position of cursor when mouse dragging starts
        private Vector3 zoomLastPos;     // Position of cursor when mouse dragging starts
        private bool isPanning;     // Is the camera being panned?
        private bool isRotating;    // Is the camera being rotated?
        private bool isZooming;     // Is the camera zooming?
        private bool deviceInverted = false;
        private bool invertForTouch = false;

        //general variables     
        private bool isMouseOverUI;
        private Camera cam;
        private Coroutine cameraMoveRoutine;
        private Transform cameraTransform;
        private Transform dummyTransform;
        private CameraState camState;
        private float defaultFOV;

        void Awake()
        {
            cameraTransform = gameObject.transform;
            cam = GetComponent<Camera>();
            GameManager.Instance.cameraController = this;
            GameManager.Instance.mainCamera = cam;
            GameManager.Instance.movieRecorder = GetComponent<MovieRecorder>();
            Input.simulateMouseWithTouches = false;
            defaultPos = cameraTransform.position;
            defaultFOV = cam.fieldOfView;
        }




        // Update is called once per frame
        void Update()
        {
            Vector3 oldPos;
            if (useBoundingBox) {
                oldPos = cameraTransform.position;
            } else {
                oldPos = defaultPos;
            }

            if (!Input.GetMouseButton(0) && !Input.GetMouseButton(1) && touchList.Count == 0) {
                isMouseOverUI = false;
            }

            if (IsPointerOverUIObject()) {
                isMouseOverUI = true;
            }
            GatherTouches();
            if (!isMouseOverUI) {
                ProcessMouseInput();
                ProcessMobileInput();
            }
            //ProcessKeyboardInput();

            //check bounding box
            if (cameraMoveRoutine == null && useBoundingBox) {
                var newPos = cameraTransform.position;
                Bounds b = new Bounds(boxCenter, boxSize);
                if(!b.Contains(cam.transform.position)) {
                    cameraTransform.position = oldPos;
                }
            }
            
        }

        private void ProcessMouseInput()
        {
            //both mouse to zoom
            if (Input.GetKey(KeyCode.RightShift) || Input.GetKey(KeyCode.LeftShift)) {
                mouseZoomSpeed = 500f;
                panSpeed = 0.5f * 10f;
            } else {
                mouseZoomSpeed = 50f;
                panSpeed = 0.5f;
            }

            if (Input.GetMouseButtonDown(0)) {
                camState = CameraState.MouseZoomOut;
            }
            if (Input.GetMouseButtonDown(1)) {
                camState = CameraState.MouseZoomIn;
            }

            if (Input.GetMouseButton(0) && Input.GetMouseButton(1)) {
                Vector3 zoom = Vector3.zero;
                if (camState == CameraState.MouseZoomIn) {
                    zoom = cameraTransform.forward;
                } else if (camState == CameraState.MouseZoomOut) {
                    zoom = -cameraTransform.forward;

                }
                cameraTransform.Translate(zoom * (Time.deltaTime / Time.timeScale) * mouseZoomSpeed, Space.World);
            } else {
                camState = CameraState.Default;
            }
            //left mouse drag to pan
            if (Input.GetMouseButtonDown(0)) {
                //Unhook both moving and rotating
                UnHookView(true);
                lastMouseDownPos = Input.mousePosition;

            }

            if (Input.GetMouseButton(0)) {
                //left mouse drag
                Vector3 currentMousePos = Input.mousePosition;
                Vector3 panPos = currentMousePos - lastMouseDownPos;

                if (panPos == Vector3.zero) {
                    return;
                }

                Vector3 pan = new Vector3(panPos.x * panSpeed * (Time.deltaTime / Time.timeScale), panPos.y * panSpeed * (Time.deltaTime / Time.timeScale), 0);
                cameraTransform.Translate(pan, Space.Self);
            }
            //right mouse drag to rotate
            if (Input.GetMouseButtonDown(1)) {
                //unhook rotation
                UnHookView(false);
                zAxis = cameraTransform.rotation.eulerAngles.z;
            }

            if (Input.GetMouseButton(1)) {
                //Rote camera
                transform.Rotate(new Vector3(-Input.GetAxis("Mouse Y") * (rotationSpeed + 20f) * (Time.deltaTime / Time.timeScale), Input.GetAxis("Mouse X") * (turnSpeed / 10.0f + 20f) * (Time.deltaTime / Time.timeScale), zAxis));
                float X = transform.rotation.eulerAngles.x;
                float Y = transform.rotation.eulerAngles.y;
                transform.rotation = Quaternion.Euler(X, Y, zAxis);
            }

            //Debug.Log($"turnSpeed: {turnSpeed} rotationSpeed: {rotationSpeed}");

            //Scroll wheel to zoom
            float scrollValue = Input.GetAxis("Mouse ScrollWheel");

            Vector3 pinch = Vector3.zero;
            if (scrollValue > 0) {
                //scroll up
                pinch = cameraTransform.forward * 100f;
            } else if (scrollValue < 0) {
                //scroll down
                pinch = -cameraTransform.forward * 100f;
            }
            if (pinch.magnitude > 0.01f) {
                UnHookView(true);
                cameraTransform.Translate(pinch, Space.World);
            }
        }
        private void ProcessKeyboardInput()
        {
            if (Input.GetKey(KeyCode.D)) {
                UnHookView(true);
                transform.Translate(new Vector3(panSpeed * 100 * (Time.deltaTime / Time.timeScale), 0, 0));
            }
            if (Input.GetKey(KeyCode.A)) {
                UnHookView(true);
                transform.Translate(new Vector3(-panSpeed * 100 * (Time.deltaTime / Time.timeScale), 0, 0));
            }
            if (Input.GetKey(KeyCode.Q)) {
                transform.Translate(new Vector3(0, -panSpeed * 100 * (Time.deltaTime / Time.timeScale), 0));
            }
            if (Input.GetKey(KeyCode.E)) {
                transform.Translate(new Vector3(0, panSpeed * 100 * (Time.deltaTime / Time.timeScale), 0));
            }
            if (Input.GetKey(KeyCode.S)) {
                UnHookView(true);
                var zoom = -cameraTransform.forward;
                cameraTransform.Translate(zoom * (Time.deltaTime / Time.timeScale) * mouseZoomSpeed, Space.World);
            }
            if (Input.GetKey(KeyCode.W)) {
                var zoom = cameraTransform.forward;
                cameraTransform.Translate(zoom * (Time.deltaTime / Time.timeScale) * mouseZoomSpeed, Space.World);
            }
        }
        private void GatherTouches()
        {
            touchList.Clear();
            if (Input.touchCount > 0) {
                for (int i = 0; i < Input.touchCount; i++)
                    AddTouchToListIfActive(Input.GetTouch(i));
            }
        }
        private void AddTouchToListIfActive(Touch touch)
        {
            touchList.Add(touch);

            if (touch.phase == TouchPhase.Began) {
                Vector2 pos = touch.position;
                rotateOrigin = (Vector3)pos;

                if (touchList.Count == 2) {
                    pinchOrigin = (touchList[0].position - touchList[1].position).magnitude;
                    panOrigin = (touchList[0].position + touchList[1].position) * 0.5f;
                }
                isRotating = true;
            }
        }
        private void ProcessMobileInput()
        {
            if (touchList.Count == 0) {
                return;
            }
            if (touchList.Count == 1) {
                if (isRotating) {
                    //unhook rotation
                    UnHookView(false);
                    rotateLastPos = touchList[0].position;
                    // Rotate Camera
                    Vector3 newPos = Camera.main.ScreenToViewportPoint(rotateLastPos - rotateOrigin);
                    if (invertForTouch) newPos = newPos * -1f;
                    cameraTransform.RotateAround(cameraTransform.position, cameraTransform.right, -newPos.y * turnSpeed);
                    cameraTransform.RotateAround(cameraTransform.position, Vector3.up, newPos.x * turnSpeed);
                }
                /*
                if (touchList[0].phase == TouchPhase.Ended) {
                    float currentTime = Time.time;
                    if (currentTime - lastTouchTime < 0.3f) {
                        Debug.Log("DoubleTap");
                        RaycastHit hit = new RaycastHit();
                        Ray ray = cam.ScreenPointToRay(touchList[0].position);
                        if (Physics.Raycast(ray, out hit)) {
                            hit.transform.gameObject.SendMessage("OnMouseUp", SendMessageOptions.DontRequireReceiver);
                        }
                    }
                    lastTouchTime = currentTime;
                }
                */
            }

            if (touchList.Count >= 2) {
                //Unhook both moving and rotating
                UnHookView(true);
                float pinchSeperation = (touchList[0].position - touchList[1].position).magnitude;
                float ratio = (Mathf.Approximately(pinchOrigin, float.Epsilon) ? 1.0f : pinchSeperation / pinchOrigin) - 1.0f; //Mathf.Sign(pinchDelta * (pinchDelta.magnitude / size.magnitude);
                // Move the camera linearly along Z axis
                Vector3 pinch = (1f * ratio) * pinchSpeed * cameraTransform.forward;
                cameraTransform.Translate(pinch, Space.World);
                //Pan
                Vector3 touchPanOffset = (touchList[0].position + touchList[1].position) * 0.5f;
                panLastPos = touchPanOffset;
                Vector3 panPos = (panLastPos - panOrigin) * 0.01f;
                if (invertForTouch) panPos = panPos * -1f;
                Vector3 pan = new Vector3(panPos.x * panSpeed, panPos.y * panSpeed, 0);
                cameraTransform.Translate(pan, Space.Self);
            }


        }

        private bool IsPointerOverUIObject()
        {
            if (EventSystem.current == null) {
                return false;
            }
            PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
            eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
            //consume this event
            EventSystem.current = null;
            return results.Count > 0;
        }


        public void HookView(Transform view)
        {
            //call unhook to cleanup stuffs
            UnHookView(true);

            //lock camera rotation
            if (cameraMoveRoutine != null) {
                StopCoroutine(cameraMoveRoutine);
            }

            // Added to work with Reflect Camera
            UnityEngine.Reflect.FreeFlyCamera flyCam = GetComponent<UnityEngine.Reflect.FreeFlyCamera>();
            flyCam.enabled = false;

            cameraMoveRoutine = StartCoroutine(HookingViewRoutine(view));
        }

        public void HookView(Vector3 position, Vector3 angle, float fov)
        {
            if (dummyTransform == null) {
                var go = new GameObject("DummyCameraNode");
                dummyTransform = go.transform;
            }
            dummyTransform.position = position;
            dummyTransform.eulerAngles = angle;
            cam.fieldOfView = fov;
            HookView(dummyTransform);
        }

        private IEnumerator HookingViewRoutine(Transform view)
        {
            var isDone = false;
            var velocity = Vector3.zero;
            if (view == null) {
                cameraMoveRoutine = null;
                yield break;
            }
            while (!isDone) {
                //yield return new WaitForEndOfFrame();
                yield return null;
                //translate camera
                var newPos = Vector3.SmoothDamp(cameraTransform.position, view.position, ref velocity, 0.5f * Time.timeScale);
                cameraTransform.position = newPos;            
                //rotation
                Quaternion rot = Quaternion.Slerp(cameraTransform.rotation, view.rotation, (3.0f / 1) * (Time.fixedDeltaTime / Time.timeScale));
                cameraTransform.rotation = rot;

                bool isDoneTranslating = Vector3.Distance(newPos, view.position) < 0.01f;
                //https://answers.unity.com/questions/288338/how-do-i-compare-quaternions.html
                bool isDoneRotating = 1.0 - Mathf.Abs(Quaternion.Dot(rot, view.rotation)) < 0.01f;

                isDone = isDoneTranslating && isDoneRotating;
                if (isDone)
                {
                    //doing a final snap to view to account for 0.01f
                    cameraTransform.position = view.position;
                    cameraTransform.rotation = view.rotation;
                }

                //Debug.Log("isDoneTranslating:" + isDoneTranslating);
                //Debug.Log("isDoneRotating:" + isDoneRotating);
            }
            UnHookView(true);
        }


        public void UnHookView(bool unhookCompletely)
        {
            onUnHookView?.Invoke();
            if (cameraMoveRoutine != null) {
                StopCoroutine(cameraMoveRoutine);
                cameraMoveRoutine = null;
            }

            // Added to work with Reflect Camera
            UnityEngine.Reflect.FreeFlyCamera flyCam = GetComponent<UnityEngine.Reflect.FreeFlyCamera>();
            flyCam.ForceStop();
            flyCam.ResetCamera();
            flyCam.enabled = true;
        }

        public void SetRotationSpeed(float f)
        {
            turnSpeed = f;
            rotationSpeed = f * 0.1f;
        }

        public void SetFlythroughSpeed(float f)
        {
            panSpeed = f;
            pinchSpeed = 10.0f + f;
        }

        public void SetFOV(float fov)
        {
            cam.fieldOfView = fov;
        }

        public float ResetFOV()
        {
            cam.fieldOfView = defaultFOV;
            return defaultFOV;
        }

    }
}

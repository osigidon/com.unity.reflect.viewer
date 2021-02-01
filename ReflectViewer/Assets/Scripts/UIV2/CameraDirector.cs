using CivilFX.Generic2;
using CivilFX.TrafficV5;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CivilFX.UI2
{
    public class CameraDirector : MonoBehaviour
    {

        private Coroutine moveRoutine;
        public Transform dummyObjectToMoveCamera;

        private int currentCameraMoveSpeed;
        private int prevCameraMoveSpeed;
        private int moveSpeedMultiplier;
        private int currentIndex;
        private float currentProgress;
        private int splinesCount;
        private Vector3 offset;

        public void MoveCamera(AnimatedCameraPathData asset)
        {
            if (moveRoutine != null) {
                StopCoroutine(moveRoutine);
            }
            GameManager.Instance.cameraController.HookView(dummyObjectToMoveCamera);
            moveRoutine = StartCoroutine(MoveAnimatedCameraRoutine(asset));
            moveSpeedMultiplier = 1;
        }

        public void StopCamera()
        {
            if (moveRoutine != null) {
                StopCoroutine(moveRoutine);
                moveRoutine = null;
            }
        }


        private IEnumerator MoveAnimatedCameraRoutine(AnimatedCameraPathData asset)
        {
            Transform trans = dummyObjectToMoveCamera.transform;
            var positions = asset.positions;
            var rotations = asset.rotations;
            List<int> speeds = new List<int>(asset.linkDatas.Count);
            foreach (var data in asset.linkDatas) {
                speeds.Add(data.translateSpeed);
            }

            var splines = GetSplinesFromVector3(positions);
            splinesCount = splines.Count;
            currentIndex = 0;
            currentProgress = 0f;
            currentCameraMoveSpeed = speeds[0];
            prevCameraMoveSpeed = currentCameraMoveSpeed;
            var progressSegment = splines[0].pathLength / currentCameraMoveSpeed * 0.44704f;
            moveSpeedMultiplier = 1;

            //IEEE 754: divide by 0 yiels infinity which works in this case for moveSpeedMultiplier
            while (true) {
                currentCameraMoveSpeed = speeds[currentIndex];
                progressSegment = splines[currentIndex].pathLength / currentCameraMoveSpeed * (1f / moveSpeedMultiplier) * 0.44704f;
                if (currentProgress > 1f) {
                    ++currentIndex;
                    if (currentIndex >= splines.Count) {
                        currentIndex = 0;
                        currentProgress = 0f;
                        currentCameraMoveSpeed = speeds[0];
                        prevCameraMoveSpeed = speeds[0];
                    } else {
                        currentProgress = 0f;
                    }
                } else if (currentProgress < 0) {
                    --currentIndex;
                    if (currentIndex < 0) {
                        currentIndex = 0;
                        currentProgress = 0f;
                        currentCameraMoveSpeed = speeds[0];
                        prevCameraMoveSpeed = speeds[0];
                    } else {
                        currentProgress = 1f;
                    }
                }
                //Debug.Log($"currentIndex: {currentIndex} currentProgress: {currentProgress} progressSegment: {progressSegment}");
                //translation
                trans.position = splines[currentIndex].GetPointOnPath(currentProgress) + offset;

                //rotation
                trans.rotation = Quaternion.Slerp(rotations[currentIndex], rotations[currentIndex + 1], currentProgress);

                currentProgress += (Time.deltaTime / Time.timeScale) / progressSegment;
                yield return null;
            }
        }

        private List<SplineBuilder> GetSplinesFromVector3(List<Vector3> positions)
        {
            List<SplineBuilder> splines = new List<SplineBuilder>();
            for (int i = 0; i < positions.Count - 1; i++) {
                var anchorBefore = Vector3.zero;
                var anchorAfter = Vector3.zero;
                if (i == 0) {
                    anchorBefore = positions[0] + (positions[0] - positions[1]);
                } else {
                    anchorBefore = positions[i - 1];
                }

                if (i == positions.Count - 2) {
                    anchorAfter = positions[positions.Count - 1] + (positions[positions.Count - 1] - positions[positions.Count - 2]);
                } else {
                    anchorAfter = positions[i + 2];
                }
                List<Vector3> nodes = new List<Vector3>(4);
                nodes.Add(anchorBefore);
                nodes.Add(positions[i]);
                nodes.Add(positions[i + 1]);
                nodes.Add(anchorAfter);
                splines.Add(new SplineBuilder(nodes));
            }
            return splines;
        }

        public void SetHeightOffset(float height)
        {
            offset = new Vector3(0, height, 0);
        }

        public void OnPause()
        {
            prevCameraMoveSpeed = currentCameraMoveSpeed;
            currentCameraMoveSpeed = 0;
            moveSpeedMultiplier = 0;
        }

        public void OnPlay()
        {
            currentCameraMoveSpeed = prevCameraMoveSpeed;
            moveSpeedMultiplier = 1;
        }

        public void OnFastForward(int multiplier)
        {
            moveSpeedMultiplier = multiplier;
        }

        public void OnRewind(int multiplier)
        {
            moveSpeedMultiplier = 1 - multiplier;
            if (moveSpeedMultiplier == 0) {
                moveSpeedMultiplier = 1;
            }
        }

        public void OnRestart()
        {
            currentIndex = 0;
            currentProgress = 0f;
        }
        public void OnEnd()
        {
            currentIndex = splinesCount - 2;
            currentProgress = .8f;
        }

    }
}
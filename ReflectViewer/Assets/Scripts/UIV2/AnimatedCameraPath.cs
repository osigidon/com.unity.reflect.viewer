using UnityEngine;

#if (UNITY_EDITOR)
using CivilFX.TrafficV5;
using System.Collections.Generic;
#endif

namespace CivilFX.UI2
{
    public class AnimatedCameraPath : MonoBehaviour
    {
#if (UNITY_EDITOR)
        public enum MakeCameraMode
        {
            Orbit,
            Eclipse,
            FollowPath,
            Manual,
        }
        public Camera previewCamera;
        public List<Vector3> positions;
        public List<Quaternion> rotations;
        public List<LinkData> linkDatas;
        public MakeCameraMode mode;
        public float height;
        public float radius;

        public float xRadius;
        public float zRadius;
        public Transform originTrans;

        public TrafficPath path;
        public float xTiltAngle;
#endif
        public AnimatedCameraPathData asset;

    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CivilFX.UI2
{
    public class CompassController : MonoBehaviour
    {

        public float compensateValue;
        public bool flipY;
        private Vector3 dir;

        private Transform cameraTrans;
        private void Start()
        {
            cameraTrans = Camera.main.transform;
        }

        // Update is called once per frame
        void Update()
        {
            if (cameraTrans == null) {
                return;
            }

            dir.z = cameraTrans.eulerAngles.y;
            if (flipY) {
                dir.z = -dir.z;
            }
            dir.z += compensateValue;
            transform.localEulerAngles = dir;
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CivilFX
{
    public class TreesModifier : MonoBehaviour
    {

        public bool individual = true;

        public float rotationScale = 1;
        public float minScale = 0.2f;
        public float maxScale = 1.2f;

        public void Apply()
        {
            if (individual)
            {

                Vector3 rot = transform.eulerAngles;
                rot.y = Random.Range(-360.0f * rotationScale, 360.0f * rotationScale);
                transform.eulerAngles = rot;

                float scaleFactor = Random.Range(minScale, maxScale);
                Vector3 scale = Vector3.one * scaleFactor;
                transform.localScale = scale;
            }
            else
            {
                foreach (Transform t in transform)
                {
                    if (t == transform)
                    {
                        continue;
                    }

                    Vector3 rot = t.eulerAngles;
                    rot.y = Random.Range(-360.0f * rotationScale, 360.0f * rotationScale);
                    t.eulerAngles = rot;

                    float scaleFactor = Random.Range(minScale, maxScale);
                    Vector3 scale = Vector3.one * scaleFactor;
                    t.localScale = scale;
                }
            }
        }
    }
}
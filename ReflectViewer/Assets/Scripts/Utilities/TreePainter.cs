using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CivilFX
{


    public class TreePainter : MonoBehaviour
    {
        public enum PaintMode
        {
            Individual,
            Group
        }

        public enum RotationAxis
        {
            X,
            Y,
            Z
        }


        public Transform parentObj;
        public GameObject[] prefabs;

        public float paintInterval = 0.5f;
        public LayerMask layer;

        public PaintMode mode;
        public float groupScale;
        public float groupCount;

        public bool applyRandomRotation = true;
        public RotationAxis axis = RotationAxis.Y;
        public Vector3 angle;
        public bool applyRandomScale = true;
        public float minScale = 0.9f;
        public float maxScale = 1.2f;
    }
}
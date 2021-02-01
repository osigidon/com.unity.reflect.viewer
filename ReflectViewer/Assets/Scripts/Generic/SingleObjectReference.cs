using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CivilFX.Generic2
{
    public class SingleObjectReference : MonoBehaviour
    {
        public GameObject referencedObject;
        public new string name;
        private void Awake()
        {
            if (referencedObject == null) {
                referencedObject = gameObject;
            }
        }
    }
}
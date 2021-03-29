using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CivilFX
{
    public class MaterialsSwapper : MonoBehaviour
    {
        public Renderer rend;
        public Material [] mainMats;

        private Color[] mainMatColors;

        public Material[] tempMats;
        private Color[] tempMatColors;

        public string referencedName;

        [HideInInspector]
        public bool isShowingTemp;

        private float step;
        private float t;

        private void Awake()
        {
            if (rend == null) {
                rend = GetComponent<MeshRenderer>();
            }

            mainMatColors = new Color[mainMats.Length];
            for (int i=0; i< mainMats.Length; i++) {
                mainMatColors[i] = mainMats[i].color;
            }

            tempMatColors = new Color[tempMats.Length];
            for (int i = 0; i < tempMats.Length; i++) {
                tempMatColors[i] = tempMats[i].color;
            }
        }

        public bool SwapMaterial()
        {
            isShowingTemp = !isShowingTemp;
            if (isShowingTemp) {
                rend.sharedMaterials = tempMats;
            } else {
                rend.sharedMaterials = mainMats;
            }
            return isShowingTemp;
        }


        //@params: isMain:  true if set to main
        public void SetMaterials(bool isMain)
        {
            if (rend == null) {
                rend = GetComponent<MeshRenderer>();
            }
            rend.sharedMaterials = isMain ? mainMats : tempMats;
            isShowingTemp = !isMain;
        }

        public void SetColorAlpha(float a)
        {
            if (tempMatColors == null) {
                tempMatColors = new Color[tempMats.Length];
                for (int i = 0; i < tempMats.Length; i++) {
                    tempMatColors[i] = tempMats[i].color;
                }
            }
            for (int i = 0; i < tempMatColors.Length; i++) {
                tempMatColors[i].a = a;
            }
            for (int i = 0; i < tempMats.Length; i++) {
                tempMats[i].color = tempMatColors[i];
            }
        }

        private IEnumerator Pulsing()
        {
            step = 0.1f;
           
            while (true) {
                foreach (var item in tempMats) {

                }
                yield return null;
            }


        }

    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CivilFX.Generic2 {
    public class CustomToggleGroup : MonoBehaviour
    {
        public Toggle[] toggles;
        public int defaultOn;

        private int currentSelected;

        private void Awake()
        {
            //set default toggle
            for (int i = 0; i < toggles.Length; i++) {
                toggles[i].isOn = i == defaultOn;
            }
            currentSelected = defaultOn;

            //set callbacks
            for (int i = 0; i < toggles.Length; i++) {
                int j = i;
                toggles[i].onValueChanged.AddListener((v) => {
                    if (v) {
                        currentSelected = j;
                    }
                });
            }
        }

        public ImageRenderer.ImageType GetSelectedImageType()
        {
            return (ImageRenderer.ImageType)currentSelected;
        }

    }
}
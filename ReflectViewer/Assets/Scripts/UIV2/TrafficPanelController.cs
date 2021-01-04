using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace CivilFX.UI2
{
    public class TrafficPanelController : MainPanelController
    {

        public Slider simSpeed;
        // Start is called before the first frame update
        void Start()
        {
            simSpeed.onValueChanged.AddListener((v) => {
                Time.timeScale = v;
            });
        }


    }
}
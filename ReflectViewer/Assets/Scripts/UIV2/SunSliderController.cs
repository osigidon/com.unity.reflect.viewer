using Entropedia;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SunSliderController : MonoBehaviour
{
    public Slider slider;

    private SunController sunController;

    // Start is called before the first frame update
    void Start()
    {


        slider.onValueChanged.AddListener((v) => { 
            if (sunController == null) {
                sunController = Resources.FindObjectsOfTypeAll<SunController>()[0];
            }
            if (sunController != null) {
                int hour = Mathf.FloorToInt(v);
                float minuteF = Mathf.Repeat(v, 1.0f);
                int minute = Mathf.FloorToInt(minuteF * 60.0f);
                sunController.SetTime(hour, minute);
                sunController.SetPosition();
            } else {
                Debug.LogError("Sun Controller is not found!");
            }
        
        });
    }


}

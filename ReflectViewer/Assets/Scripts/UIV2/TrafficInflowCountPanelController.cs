using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CivilFX.TrafficV5;

namespace CivilFX.UI2
{
    public class TrafficInflowCountPanelController : UIDraggable
    {

        public TextMeshProUGUI pathNameTMP;
        public Slider inflow;
        public TextMeshProUGUI inflowTMP;
        public CustomButton done;

        //private TrafficPathController pathController;

        //public SpawnPointGroupController groupController;

        // Start is called before the first frame update
        void Awake()
        {
            inflow.onValueChanged.AddListener((v) => {
                inflowTMP.text = v.ToString();
                //pathController.inflowCount = (int)v;
            });

            done.RegisterMainButtonCallback(() => {
                done.RestoreInternalState();
                //groupController.OnDone();
                OnHidden();
            });
        }

        public void OnVisible(TrafficPathController _pathController)
        {
            gameObject.SetActive(true);
            //pathController = _pathController;
            //pathNameTMP.text = pathController.name;
           // inflow.value = pathController.inflowCount;
        }
        public void OnHidden()
        {
            gameObject.SetActive(false);
            //pathController = null;
        }


    }
}
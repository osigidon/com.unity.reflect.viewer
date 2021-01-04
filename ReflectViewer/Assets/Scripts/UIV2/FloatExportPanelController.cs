using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

namespace CivilFX.UI2 {
    public class FloatExportPanelController : UIDraggable, IRecorderListener
    {
        public TMP_Dropdown dropdown;
        public GameObject imagePanel;
        public GameObject videoPanel;
        public RenderingPanelController mainRenderPanel;
        public CustomButton dock;
        // Start is called before the first frame update
        void Awake()
        {
            dropdown.onValueChanged.AddListener((v) => {
                imagePanel.SetActive(v == 0);
                videoPanel.SetActive(v == 1);
            });

            dock.RegisterMainButtonCallback(() => {
                dock.RestoreInternalState();
                if (!mainRenderPanel.isVisible) {
                    return;
                }
                mainRenderPanel.OnVisible();
                mainRenderPanel.state = RenderingPanelController.PanelState.Docked;
                gameObject.SetActive(false);
            });
        }

        public void OnBeginRecording()
        {
            dropdown.interactable = false;
            dock.gameObject.SetActive(false);
        }

        public void OnEndRecording()
        {
            dropdown.interactable = true;
            dock.gameObject.SetActive(true);
        }
    }
}

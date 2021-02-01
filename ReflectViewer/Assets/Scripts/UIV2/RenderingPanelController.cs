using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CivilFX.UI2
{
    public class RenderingPanelController : MainPanelController, IRecorderListener
    {
        public enum PanelState
        {
            Docked,
            Floated
        }

        public CustomButton floatButton;
        public GameObject exportImagePanel;
        public GameObject floatExportPanel;
        public PanelState state {
            get;
            set;
        }

        public bool isDocked {
            get { return state == PanelState.Docked; }
        }

        private void Awake()
        {
            state = PanelState.Docked;
            floatButton.RegisterMainButtonCallback(() => {
                state = PanelState.Floated;
                floatExportPanel.SetActive(true);
                //hide the visibible
                //but do not change IsVisible
                gameObject.SetActive(false);
                floatButton.RestoreInternalState();
            });
        }

        public void OnBeginRecording()
        {
            exportImagePanel.SetActive(false);
            floatButton.gameObject.SetActive(false);
        }

        public void OnEndRecording()
        {
            exportImagePanel.SetActive(true);
            floatButton.gameObject.SetActive(true);
        }
    }
}
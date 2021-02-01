using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CivilFX.Generic2;

namespace CivilFX.UI2
{
    public class PhasingPanelController : MonoBehaviour
    {
        public CustomButton proposed;
        public CustomButton existing;

        private CustomButton lastSelected;

        private void Awake()
        {
            proposed.RegisterMainButtonCallback(() => {
                if (proposed == lastSelected)
                {
                    return;
                }
                if (lastSelected != null)
                {
                    lastSelected.RestoreInternalState();
                }

                PhasedManager.Invoke(PhaseType.Proposed);

                lastSelected = proposed;
            });

            existing.RegisterMainButtonCallback(() => {
                if (existing == lastSelected) {
                    return;
                }
                if (lastSelected != null) {
                    lastSelected.RestoreInternalState();
                }

                PhasedManager.Invoke(PhaseType.Existing);

                lastSelected = existing;
            });


            proposed.InvokeMainButton();

        }

    }
}

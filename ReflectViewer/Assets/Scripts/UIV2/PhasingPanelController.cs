using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CivilFX.Generic2;

namespace CivilFX.UI2
{
    public class PhasingPanelController : MonoBehaviour
    {
        public CustomButton proposedFullRSB;
        public CustomButton proposedSBX;

        public CustomButton existing;
        public CustomButton existingMOT1A;

        private CustomButton lastSelected;

        private void Awake()
        {
            proposedFullRSB.RegisterMainButtonCallback(() => {
                if (proposedFullRSB == lastSelected) {
                    return;
                }
                if (lastSelected != null) {
                    lastSelected.RestoreInternalState();
                }

                //invoke phase
                PhasedManager.Invoke(PhaseType.Proposed);
                PhasedManager.Invoke(PhaseType.ProposedFullRSB, PhaseMode.Additive);
                lastSelected = proposedFullRSB;
            });

            proposedSBX.RegisterMainButtonCallback(() => {
                if (proposedSBX == lastSelected)
                {
                    return;
                }
                if (lastSelected != null)
                {
                    lastSelected.RestoreInternalState();
                }

                //invoke phase
                //PhasedManager.Invoke(PhaseType.Proposed);
                //PhasedManager.Invoke(PhaseType.ProposedSBX, PhaseMode.Additive);

                PhasedManager.Invoke(PhaseType.Existing);
                PhasedManager.Invoke(PhaseType.ExistingNotMOT, PhaseMode.Additive);
                PhasedManager.Invoke(PhaseType.ProposedSBX, PhaseMode.Additive);

                lastSelected = proposedSBX;
            });

            existing.RegisterMainButtonCallback(() => {
                if (existing == lastSelected) {
                    return;
                }
                if (lastSelected != null) {
                    lastSelected.RestoreInternalState();
                }
                //invoke phase
                PhasedManager.Invoke(PhaseType.Existing);
                PhasedManager.Invoke(PhaseType.ExistingNotMOT, PhaseMode.Additive);
                lastSelected = existing;
            });


            existingMOT1A.RegisterMainButtonCallback(() => {
                if (existingMOT1A == lastSelected)
                {
                    return;
                }
                if (lastSelected != null)
                {
                    lastSelected.RestoreInternalState();
                }
                //invoke phase
                PhasedManager.Invoke(PhaseType.Existing);
                PhasedManager.Invoke(PhaseType.ExistingMOT1A, PhaseMode.Additive);
                lastSelected = existingMOT1A;
            });

            proposedFullRSB.InvokeMainButton();

        }

    }
}
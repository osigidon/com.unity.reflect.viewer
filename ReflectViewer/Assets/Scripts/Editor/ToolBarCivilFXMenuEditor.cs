using UnityEditor;
using UnityEngine;

namespace CivilFX.Generic2
{
    public class ToolBarCivilFXMenuEditor : Editor
    {
        [MenuItem("CivilFX/Phase/Proposed Full RSB", false, 100)]
        static void ToggleProposedRSB()
        {
            PhasedManager.Invoke(PhaseType.Proposed);
            PhasedManager.Invoke(PhaseType.ProposedFullRSB, PhaseMode.Additive);
        }

        [MenuItem("CivilFX/Phase/Proposed SBX", false, 100)]
        static void ToggleProposedSBX()
        {
            PhasedManager.Invoke(PhaseType.Existing);
            PhasedManager.Invoke(PhaseType.ExistingNotMOT, PhaseMode.Additive);
            PhasedManager.Invoke(PhaseType.ProposedSBX, PhaseMode.Additive);
        }


        [MenuItem("CivilFX/Phase/Existing", false, 200)]
        static void ToggleExisting()
        {
            PhasedManager.Invoke(PhaseType.Existing);
            PhasedManager.Invoke(PhaseType.ExistingNotMOT, PhaseMode.Additive);
        }

        [MenuItem("CivilFX/Phase/Existing MOT 1A", false, 200)]
        static void ToggleExistingMOT1A()
        {
            PhasedManager.Invoke(PhaseType.Existing);
            PhasedManager.Invoke(PhaseType.ExistingMOT1A, PhaseMode.Additive);
        }

        [MenuItem("CivilFX/Phase/Existing MOT 1B", false, 200)]
        static void ToggleExistingMOT1B()
        {
            PhasedManager.Invoke(PhaseType.Existing);
            PhasedManager.Invoke(PhaseType.ExistingMOT1B, PhaseMode.Additive);
        }
    }
}

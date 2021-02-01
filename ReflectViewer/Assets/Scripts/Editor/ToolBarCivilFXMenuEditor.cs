using UnityEditor;
using UnityEngine;

namespace CivilFX.Generic2
{
    public class ToolBarCivilFXMenuEditor : Editor
    {
        [MenuItem("CivilFX/Phase/Proposed", false, 100)]
        static void ToggleProposedRSB()
        {
            PhasedManager.Invoke(PhaseType.Proposed);
        }


        [MenuItem("CivilFX/Phase/Existing", false, 200)]
        static void ToggleExisting()
        {
            PhasedManager.Invoke(PhaseType.Existing);
        }

    }
}

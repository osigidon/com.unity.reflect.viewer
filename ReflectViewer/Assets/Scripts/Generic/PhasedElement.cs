using System.Collections;
using System.Collections.Generic;
#if (UNITY_EDITOR)
using UnityEditor.SceneManagement;
#endif
using UnityEngine;

namespace CivilFX.Generic2
{

    [ExecuteInEditMode]
    public class PhasedElement : MonoBehaviour
    {
        public PhaseType[] phasedTypes;
        public bool reverse;

        public void DoPhase(PhaseType type, PhaseMode mode)
        {
            if (mode == PhaseMode.Single) {
                gameObject.SetActive(reverse);
            }

            for (int i = 0; i < phasedTypes.Length; i++) {
                if (type == phasedTypes[i]) {
                    gameObject.SetActive(!reverse);
                    break;
                }
            }
#if UNITY_EDITOR
            if (!Application.isPlaying) {
                EditorSceneManager.MarkSceneDirty(gameObject.scene);
            }
#endif
        }


        public void Awake()
        {
            PhasedManager.Elements = null;
        }

        public void OnDestroy()
        {
            PhasedManager.Elements = null;
        }

    }

}
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CivilFX.Generic2
{
    public enum PhaseType
    {
        Proposed,
        Existing,
        ExistingNotMOT,
        ExistingMOT1A,
        ExistingMOT1B,
        ProposedFullRSB,
        ProposedSBX
    }

    public enum PhaseMode
    {
        Single,
        Additive
    }
    public sealed class PhasedManager
    {
        private static PhaseType currentPhase;
        private static PhasedElement[] phasedElements;
        private static Action<PhaseType> invokeCallback;
        public static PhasedElement[] Elements {
            get {
                if (phasedElements == null) {
                    phasedElements = Resources.FindObjectsOfTypeAll<PhasedElement>();
                }
                return phasedElements;
            }
            set {
                phasedElements = value;
            }
        }

        private PhasedManager() { }
        static PhasedManager() { }
        public static void Invoke(PhaseType type, PhaseMode mode=PhaseMode.Single)
        {
            if (mode == PhaseMode.Single) {
                currentPhase = type;
            }

            for (int i = 0; i < Elements.Length; i++) {
                Elements[i].DoPhase(type, mode);
            }

            //callback
            invokeCallback?.Invoke(type);
        }

        public static void RegisterCallback(Action<PhaseType> cb)
        {
            invokeCallback += cb;
        }
    }
}
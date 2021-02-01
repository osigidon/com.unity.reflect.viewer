using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CivilFX {

    [ExecuteInEditMode]
    public class TreesModifierPurger : MonoBehaviour
    {

        private void Awake()
        {
            Debug.Log("Destroying all tree modifier scripts...");
            TreesModifier[] treesModifiers = GetComponentsInChildren<TreesModifier>();

            if (treesModifiers != null)
            {
                for (int i=0; i<treesModifiers.Length; i++)
                {
                    DestroyImmediate(treesModifiers[i]);
                    treesModifiers[i] = null;
                }

            }
            Debug.Log("Done: Scripts destroyed: " + treesModifiers.Length);
            treesModifiers = null;
        }
    }
}
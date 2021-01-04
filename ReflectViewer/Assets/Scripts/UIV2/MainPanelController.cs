using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CivilFX.UI2
{
    public abstract class MainPanelController : MonoBehaviour
    {
        public bool isVisible {
            get;
            set;
        }

        public void OnVisible()
        {
            isVisible = true;
            gameObject.SetActive(true);
        }
        public void OnHidden()
        {
            isVisible = false;
            gameObject.SetActive(false);
        }

    }
}
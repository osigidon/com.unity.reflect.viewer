using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace CivilFX.UI2
{
    public class AnimatedCustomButton : CustomButton
    {
        public CustomButton thirdButton;

        public new void Awake()
        {
            base.Awake();
        }

        public void ShowThirdButton(bool active)
        {
            thirdButton.gameObject.SetActive(active);
        }

        public void RegisterThirdButtonCallback(UnityAction cb)
        {
            thirdButton.RegisterMainButtonCallback(cb);
        }
    }
}
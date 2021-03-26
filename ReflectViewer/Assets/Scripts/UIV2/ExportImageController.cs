using CivilFX.Generic2;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
namespace CivilFX.UI2
{
    public class ExportImageController : MonoBehaviour
    {
        public CustomButton EightK;
        public CustomButton FourK;
        public CustomButton TwoK;

        public CustomToggleGroup imageTypeGroup;

        private void Awake()
        {
            EightK.RegisterMainButtonCallback(() => {
                var type = imageTypeGroup.GetSelectedImageType();
                GameManager.Instance.imageRenderer.RequestCapture(ImageRenderer.CapturedSize.EightK, type);
                EightK.RestoreInternalState();
            });
            FourK.RegisterMainButtonCallback(() => {
                var type = imageTypeGroup.GetSelectedImageType();
                GameManager.Instance.imageRenderer.RequestCapture(ImageRenderer.CapturedSize.FourK, type);
                FourK.RestoreInternalState();
            });
            TwoK.RegisterMainButtonCallback(() => {
                var type = imageTypeGroup.GetSelectedImageType();
                GameManager.Instance.imageRenderer.RequestCapture(ImageRenderer.CapturedSize.TwoK, type);
                TwoK.RestoreInternalState();
            });
        }

    }
}
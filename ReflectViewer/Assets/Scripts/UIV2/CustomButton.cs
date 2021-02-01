using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.Events;

namespace CivilFX.UI2
{
    public class CustomButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public enum ButtonState
        {
            Deselected,
            Selected
        }
    
        public Button mainButton;
        public Button secondaryButton;
        public TextMeshProUGUI title;

        public Image mainImage;
        public Color hoveringColor = Color.white;
        public Color selectedColor = Color.white;
        public Color deselectedColor = Color.white;

        public ButtonState state { get; private set; } = ButtonState.Deselected;

        private bool restore;
        public void Awake()
        {
            //internal callback for main button
            mainButton.onClick.AddListener(() => {
                state = ButtonState.Selected;
                mainImage.color = selectedColor;
            });
        }

        public void InvokeMainButton()
        {
            mainButton.onClick.Invoke();
        }

        public void RegisterMainButtonCallback(UnityAction cb, bool unregisterFirst=true)
        {
            if (unregisterFirst) {
                UnRegisterMainButtonCallback();
            }
            mainButton.onClick.AddListener(cb);
        }

        public void UnRegisterMainButtonCallback()
        {
            mainButton.onClick.RemoveAllListeners();
            //add back internal callback
            mainButton.onClick.AddListener(() => {
                state = ButtonState.Selected;
                mainImage.color = selectedColor;
            });
        }

        public void SetInteractable(bool interactable)
        {
            mainButton.interactable = interactable;
        }

        public void ShowSecondaryButton(bool active)
        {
            secondaryButton.gameObject.SetActive(active);
        }
        public void RegisterSecondaryButtonCallback(UnityAction cb, bool unregisterFirst=true)
        {
            if (unregisterFirst) {
                UnRegisterSecondaryButtonCallback();
            }
            secondaryButton.onClick.AddListener(cb);
        }
        public void UnRegisterSecondaryButtonCallback()
        {
            secondaryButton.onClick.RemoveAllListeners();
        }

        public void RestoreInternalState()
        {
            restore = true;
            if (gameObject.activeInHierarchy) {
                StartCoroutine(RestoreRoutine());
            }
        }

        public void SetTitle(string name)
        {
            title.text = name;
        }


        private IEnumerator RestoreRoutine()
        {
            yield return null;
            state = ButtonState.Deselected;
            mainImage.color = deselectedColor;
            restore = false;
        }

        public void OnEnable()
        {
            if (restore) {
                StartCoroutine(RestoreRoutine());
            }
        }

        #region interface methods
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (state == ButtonState.Deselected) {
                mainImage.color = hoveringColor;
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (state == ButtonState.Deselected) {
                mainImage.color = deselectedColor;
            }
        }
        #endregion
    }
}
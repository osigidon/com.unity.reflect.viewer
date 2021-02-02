using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.Events;
using UnityEngine.Reflect;


namespace CivilFX.UI2
{
    public class NotificationPanelController : UIDraggable
    {
        public TextMeshProUGUI title;
        public TMP_InputField inputField;
        public Button confirmButton;
        public Button cancelButton;

        public RectTransform Panel;

        public delegate void ConfirmedCallback(string inputText);
        private event ConfirmedCallback confirmedCallback;

        public bool isShowing {
            get;
            private set;
        }

        private void Awake()
        {
            Hide();

            Debug.Log(Panel.rect.width);
            Debug.Log(Panel.rect.height);

            confirmButton.onClick.AddListener(() => {
                
                confirmedCallback?.Invoke(inputField.textComponent.text);
                confirmedCallback = null;
                Hide();
            });

            cancelButton.onClick.AddListener(() => {
                Hide();
            });
        }

        private void Update()
        {
            // Stop camera while typing camera name, else controlled by WASD
            if(inputField.isFocused)
            {
                GameObject.FindGameObjectWithTag("MainCamera").GetComponent<FreeFlyCamera>().ForceStop();
            }
        }

        public void SetTitle(string name)
        {
            title.text = name;
        }

        public void SetInputHint(string hint)
        {
            inputField.textViewport.GetComponentInChildren<TextMeshProUGUI>().text = hint;
        }

        public void Show()
        {
            gameObject.SetActive(true);
            isShowing = true;
        }

        public void Hide()
        {
            gameObject.SetActive(false);
            isShowing = false;
        }

        public void RegisterConfirmCallback(ConfirmedCallback cb)
        {
            confirmedCallback = cb;
        }
    }
}

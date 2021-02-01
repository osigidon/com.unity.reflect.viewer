using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

using CivilFX.Generic2;
namespace CivilFX.UI2
{
    public class MainMenuController : MonoBehaviour
    {
        public enum MenuState
        {
            Close,
            Open
        }


        [Header("Main Button")]
        public Button mainButton;
        public Sprite openImage;
        public Sprite closeImage;
        public MenuState defaultState;
        public TextMeshProUGUI titleText;
        private MenuState currentState;

        [Header("Navigation")]
        public Button navButton;
        public MainPanelController navPanel;

        [Header("Rendering")]
        public Button rendButton;
        public MainPanelController rendPanel;

        [Header("Settings")]
        public Button settButton;
        public MainPanelController settPanel;

        [Header("Traffic")]
        public Button trafficButton;
        public MainPanelController trafficPanel;

        [Header("Exit")]
        public CustomButton exit;

        private Button lastSelectedButton;
        private MainPanelController lastSelectedPanel;

        private Animator menuAnim;
        void Awake()
        {
            menuAnim = GetComponent<Animator>();

            menuAnim.enabled = true;
            currentState = MenuState.Close;

            //main button
            mainButton.onClick.AddListener(() => {
                if (currentState == MenuState.Close) {
                    //open the menu
                    mainButton.GetComponent<Image>().sprite = closeImage;

                    menuAnim.Play("MainMenuOpen");
                    currentState = MenuState.Open;
                } else {
                    //close the menu
                    mainButton.GetComponent<Image>().sprite = openImage;
                    menuAnim.Play("MainMenuClose");
                    currentState = MenuState.Close;
                }
                StartCoroutine(WaitForFinishAnimation());
            });

            if (defaultState == MenuState.Open) {
                mainButton.onClick.Invoke();
            }

            //nav button
            navButton.onClick.AddListener(() => {
                if (navButton == lastSelectedButton) {
                    return;
                }
                titleText.text = "NAVIGATION";
                ButtonClickHandler(navButton, navPanel);
            });

            //sett button
            settButton.onClick.AddListener(() => {
                if (settButton == lastSelectedButton) {
                    return;
                }
                titleText.text = "SETTINGS";
                ButtonClickHandler(settButton, settPanel);
            });

            //REND button
            rendButton.onClick.AddListener(() => {
                if (rendButton == lastSelectedButton) {
                    return;
                }
                titleText.text = "RENDERING";
                ButtonClickHandler(rendButton, rendPanel);
                if (!((RenderingPanelController)rendPanel).isDocked) {
                    //hide the panel if it is floating
                    //but do not change the visibility
                    //visibility == true here
                    rendPanel.gameObject.SetActive(false);
                }

            });

            //about button
            trafficButton.onClick.AddListener(() => {
                if (trafficButton == lastSelectedButton) {
                    return;
                }
                titleText.text = "ABOUT";
                ButtonClickHandler(trafficButton, trafficPanel);
            });

            //exit
            exit.RegisterMainButtonCallback(() => {
                Application.Quit();
            });

            //default button
            navButton.onClick.Invoke();
            rendPanel.OnHidden();
            settPanel.OnHidden();
            trafficPanel.OnHidden();


            //disable render panel if platform is webgl
            if (Application.platform == RuntimePlatform.WebGLPlayer) {
                rendButton.interactable = false;
            }
        }

        private IEnumerator WaitForFinishAnimation()
        {
            mainButton.interactable = false;
            yield return new WaitForSeconds(0.66f);
            mainButton.interactable = true;
        }

        private void ButtonClickHandler(Button button, MainPanelController panel)
        {
            if (lastSelectedButton != null) {
                lastSelectedButton.image.color = Color.white;
                lastSelectedPanel.OnHidden();
            }

            button.image.color = Utilities.CustomColor.civilGreen;
            panel.OnVisible();

            lastSelectedButton = button;
            lastSelectedPanel = panel;
        }

    }
}
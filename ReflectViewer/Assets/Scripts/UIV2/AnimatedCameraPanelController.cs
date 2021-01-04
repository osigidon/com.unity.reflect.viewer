using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CivilFX.UI2
{
    public class AnimatedCameraPanelController : MonoBehaviour
    {
        private enum BState
        {
            Selected,
            Deselected
        }

        private enum BType
        {
            Reset,
            Rewind,
            Pause,
            FastForward,
            End
        }

        public Button resetButton;
        public Sprite resetSelectedImage;
        public Sprite resetDeslectedImage;

        public Button rewindButton;
        public Sprite rewindSelectedImage;
        public Sprite rewindDeslectedImage;

        public Button pauseButton;
        public Sprite pauseSelectedImage;
        public Sprite pauseDeslectedImage;

        public Button fastForwardButton;
        public Sprite fastForwardSelectedImage;
        public Sprite fastForwardDeslectedImage;

        public Button endButton;
        public Sprite endSelectedImage;
        public Sprite endDeslectedImage;

        public CameraDirector director;

        private BType currentButtonType;
        private BState currentButtonState;

        public void Awake()
        {
            resetButton.onClick.AddListener(() => {
                director.OnRestart();
            });

            endButton.onClick.AddListener(() => {
                director.OnEnd();
            });

            pauseButton.onClick.AddListener(() => {
                //for pause button:
                //deselected == playing
                if (currentButtonType == BType.Pause) {
                    if (currentButtonState == BState.Selected) {
                        //play
                        director.OnPause();
                        pauseButton.image.sprite = pauseDeslectedImage;
                        currentButtonState = BState.Deselected;
                    } else {
                        //pause
                        director.OnPlay();
                        pauseButton.image.sprite = pauseSelectedImage;
                        currentButtonState = BState.Selected;
                    }
                } else {
                    currentButtonType = BType.Pause;
                    currentButtonState = BState.Deselected;
                    pauseButton.image.sprite = pauseDeslectedImage;
                    director.OnPause();

                    //set others buttons to default
                    fastForwardButton.image.sprite = fastForwardDeslectedImage;
                    rewindButton.image.sprite = rewindDeslectedImage;
                }
            });

            rewindButton.onClick.AddListener(() => {
                if (currentButtonType == BType.Rewind) {
                    if (currentButtonState == BState.Selected) {
                        currentButtonState = BState.Deselected;
                        rewindButton.image.sprite = rewindDeslectedImage;
                        director.OnRewind(1);
                    } else {
                        currentButtonState = BState.Selected;
                        rewindButton.image.sprite = rewindSelectedImage;
                        director.OnRewind(2);
                    }
                } else {
                    currentButtonType = BType.Rewind;
                    currentButtonState = BState.Selected;
                    rewindButton.image.sprite = rewindSelectedImage;
                    director.OnRewind(2);

                    //set other buttons to default
                    fastForwardButton.image.sprite = fastForwardDeslectedImage;
                    pauseButton.image.sprite = pauseSelectedImage;
                }
            });


            fastForwardButton.onClick.AddListener(() => { 
                if (currentButtonType == BType.FastForward) {
                    if (currentButtonState == BState.Selected) {
                        currentButtonState = BState.Deselected;
                        fastForwardButton.image.sprite = fastForwardDeslectedImage;
                        director.OnFastForward(1);
                    } else {
                        currentButtonState = BState.Selected;
                        fastForwardButton.image.sprite = fastForwardSelectedImage;
                        director.OnFastForward(2);
                    }
                } else {
                    currentButtonType = BType.FastForward;
                    currentButtonState = BState.Selected;
                    fastForwardButton.image.sprite = fastForwardSelectedImage;
                    director.OnFastForward(2);

                    //set other buttons to defulat
                    rewindButton.image.sprite = rewindDeslectedImage;
                    pauseButton.image.sprite = pauseSelectedImage;
                }      
            });


        }

        public void RestoreInternalImages()
        {
            rewindButton.image.sprite = rewindDeslectedImage;
            fastForwardButton.image.sprite = fastForwardDeslectedImage;
            pauseButton.image.sprite = pauseSelectedImage;
        }

    }
}
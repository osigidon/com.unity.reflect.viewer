using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UTJ.FrameCapturer;

namespace CivilFX.Generic2
{
    public sealed class GameManager
    {
        private static readonly GameManager instance = new GameManager();
        static GameManager()
        {
        }

        private GameManager()
        {

        }

        public static GameManager Instance
        {
            get { return instance; }
        }

        public Camera mainCamera
        {
            get;
            set;
        }
        public CameraController cameraController
        {
            get;
            set;
        }

        public ImageRenderer imageRenderer
        {
            get;
            set;
        }

        public MovieRecorder movieRecorder
        {
            get;
            set;
        }

    }
}

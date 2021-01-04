
using System.Collections;
using System.IO;
using UnityEngine;


namespace CivilFX.Generic2 {
    public class ImageRenderer : MonoBehaviour
    {
        public enum ImageType
        {
            JPG,
            PNG,
            TGA
        }

        public enum CapturedSize
        {
            EightK,
            FourK,
            TwoK,
        }

        private string savedPath;
        private bool capture;

        private RenderTexture tempRenderTexture;

        private int width;
        private int height;
        private ImageType imageType;
        private Camera mainCamera;

        // Start is called before the first frame update
        void Awake()
        {
            //create folder
            savedPath = Path.GetFullPath(string.Format(@"{0}/", "ScreenShots"));
            Directory.CreateDirectory(savedPath);

            capture = false;
            GameManager.Instance.imageRenderer = this;
            mainCamera = GetComponent<Camera>();
        }


        public void RequestCapture(CapturedSize size, ImageType type)
        {
            switch (size) {
                case CapturedSize.EightK:
                    width = 7680;
                    height = 4320;
                    break;
                case CapturedSize.FourK:
                    width = 3840;
                    height = 2160;
                    break;
                case CapturedSize.TwoK:
                    width = 2048;
                    height = 1080;
                    break;
            }
            tempRenderTexture = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
            tempRenderTexture.antiAliasing = 8;
            tempRenderTexture.wrapMode = TextureWrapMode.Clamp;
            tempRenderTexture.filterMode = FilterMode.Bilinear;
           
            capture = true;
            imageType = type;
            mainCamera.targetTexture = tempRenderTexture;
        }

        IEnumerator OnPostRender()
        {
            if (capture) {
                Debug.Log("Capture");
                yield return new WaitForEndOfFrame();

                var tex = new Texture2D(tempRenderTexture.width, tempRenderTexture.height, TextureFormat.RGB24, false);
                var currentRT = RenderTexture.active;
                RenderTexture.active = tempRenderTexture;
                tex.ReadPixels(new Rect(0, 0, tex.width, tex.height), 0, 0, false);
                tex.Apply();

                string extention = null;
                switch (imageType) {
                    case ImageType.JPG:
                        extention = ".jpg";
                        break;
                    case ImageType.PNG:
                        extention = ".png";
                        break;
                    case ImageType.TGA:
                        extention = ".tga";
                        break;
                }

                //save image
                var imgPath = string.Format("{0}/{1}{2:D05}{3}", savedPath, "Image", Time.frameCount, extention);
                byte[] rawBytes = null;
                switch (imageType) {
                    case ImageType.JPG:
                        rawBytes = tex.EncodeToJPG(100);
                        break;
                    case ImageType.PNG:
                        rawBytes = tex.EncodeToPNG();
                        break;
                    case ImageType.TGA:
                        rawBytes = tex.EncodeToTGA();
                        break;
                }
                File.WriteAllBytes(imgPath, rawBytes);
                capture = false;

                Destroy(tex);

                //reset main camera
                mainCamera.targetTexture = null;
                RenderTexture.active = currentRT;
                OpenSavedFolder();
            }
            //

        }
        private void OpenSavedFolder()
        {
            var savedPath = Path.GetFullPath(string.Format(@"{0}/", "ScreenShots"));
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo() {
                FileName = savedPath,
                UseShellExecute = true,
                Verb = "open"
            });
        }
    }
}
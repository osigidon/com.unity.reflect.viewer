using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Unity.Collections;
using TMPro;
#if (UNITY_EDITOR)
using UnityEditor;
#endif

namespace CivilFX.Generic2
{
    public class ScenesLoader : MonoBehaviour
    {

#if (UNITY_EDITOR)
        public SceneAsset[] stepScenes;
        public SceneAsset[] concurrenceScenes;
#endif
        public string[] stepSceneNames;
        public string[] concurrenceSceneNames;

        public Image barProgress;
        public Image circularProgress;
        public TextMeshProUGUI textProgress;

        private int scenesCount;
        private int scenesDoneCount;
        private bool isDone;
        // Use this for initialization
        void Awake()
        {
            Debug.Log(stepSceneNames.Length);
            Debug.Log(concurrenceSceneNames.Length);
            if (!Application.isEditor) {
                isDone = false;
                scenesCount = SceneManager.sceneCountInBuildSettings;
                scenesDoneCount = 1;
                StartCoroutine(LoadScenesStep());
                StartCoroutine(LoadScenesConcurrence());
                StartCoroutine(UpdateUI());
            }
        }

        private IEnumerator LoadScenesStep()
        {
            foreach (var sceneName in stepSceneNames) {
                Debug.Log(sceneName);
                var asyncLoad = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
                while (!asyncLoad.isDone) {
                    yield return null;
                }
                ++scenesDoneCount;
            }
            yield return null;
            isDone = true;

        }

        private IEnumerator LoadScenesConcurrence()
        {
            //wait until LoadScenesStep() is done
            while (!isDone) {
                yield return null;
            }

            List<AsyncOperation> handles = new List<AsyncOperation>(concurrenceSceneNames.Length);

            //load left over scences
            foreach (var sceneName in concurrenceSceneNames) {
                var asyncLoad = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
                handles.Add(asyncLoad);
            }

            while (handles.Count > 0) {
                yield return null;
                AsyncOperation doneHanle = null;
                foreach (var handle in handles) {
                    if (handle.isDone) {
                        doneHanle = handle;
                        ++scenesDoneCount;
                        break;
                    }
                }
                //remove done handle from handles
                if (doneHanle != null) {
                    handles.Remove(doneHanle);
                }
            }

            //all done
            SceneManager.SetActiveScene(SceneManager.GetSceneByName(stepSceneNames[0]));

            yield return null;

            //unload this scene
            SceneManager.UnloadSceneAsync(0);

        }
        private IEnumerator UpdateUI()
        {
            float zAngle = 0;
            Vector3 eulerAngle = Vector3.zero;

            if (barProgress == null || circularProgress == null || textProgress == null) {
                yield break;
            }

            while (true) {
                yield return new WaitForEndOfFrame();
                var progress = (float)scenesDoneCount / scenesCount;

                //update UI bar progress
                barProgress.fillAmount = progress;

                //update UI circular progress
                zAngle += 10f;
                eulerAngle.z = zAngle;
                circularProgress.transform.eulerAngles = eulerAngle;

                //update text
                textProgress.text = $"{(progress * 100).ToString("F0")}%";
            }
        }

    }
}
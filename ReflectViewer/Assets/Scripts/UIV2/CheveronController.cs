using CivilFX.Generic2;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CivilFX.UI2 {
    public class CheveronController : MonoBehaviour
    {

        private Coroutine routine;
        private GameObject lastObj;

        public bool isPlaying {
            get; private set;
        }
        public void StartCheveron(AnimatedCameraPathData asset)
        {
            if (routine != null) {
                Stop();
            }
            routine = StartCoroutine(CheveronRoutine(asset));
            isPlaying = true;
        }

        public void Stop()
        {
            if (routine != null) {
                StopCoroutine(routine);
            }
            routine = null;

            if (lastObj != null) {
                lastObj.SetActive(false);
            }
            lastObj = null;
            isPlaying = false;
        }


        private IEnumerator CheveronRoutine(AnimatedCameraPathData asset)
        {
            //find the object
            GameObject go = null;
            foreach (var obj in Resources.FindObjectsOfTypeAll<SingleObjectReference>()) {
                if (obj.name.Equals(asset.objName)) {
                    go = obj.referencedObject;
                    break;
                }
            }

            if (go == null) {
                yield break;
            }
            go.SetActive(true);
            lastObj = go;
            //find mat
            var mat = asset.sequence.material;
            mat.color = asset.color;
            var sequences = asset.sequence.sprites;
            var currentIndex = 0;
            var timeStep = .1f;
            var currentTime = 0f;
            while (true) {
                if (currentTime <= 0) {
                    mat.mainTexture = sequences[currentIndex++];
                    currentIndex %= sequences.Length;
                    currentTime = timeStep;
                }
                currentTime -= Time.deltaTime;
                yield return null;
            }
        }
    }
}
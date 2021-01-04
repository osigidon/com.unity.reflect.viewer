using System.Collections;
using System.Collections.Generic;
using System.Text;
#if (UNITY_EDITOR)
using UnityEditor;
#endif
using UnityEngine;

namespace CivilFX.UI2
{

    [System.Serializable]
    public class LinkData
    {
        public enum TranslateType
        {
            Constant,
            RampUp,
            RampDown,
        }

        public enum RotationType
        {
            Interpolate,
            Constant,
            LookatNext,
            LookatTarget
        }

        public TranslateType translateType = TranslateType.Constant;
        public int translateSpeed = 30;

        public RotationType rotationType = RotationType.Interpolate;
        public Quaternion constantRotation = Quaternion.identity;
        public Vector3 lookatTarget = Vector3.zero;


    }

    public class AnimatedCameraPathData : ScriptableObject
    {
        public List<Vector3> positions = new List<Vector3>();
        public List<Quaternion> rotations = new List<Quaternion>();
        public List<LinkData> linkDatas = new List<LinkData>();

        //pathway
        public bool showPathway;
        public string objName;
        public Color color = Color.green;
        public CheveronSequence sequence;

#if UNITY_EDITOR
        public AnimatedCameraPathData SaveAssetToDisk(string savedPath, string assetName)
        {

            StringBuilder sb = new StringBuilder();
            sb.Append(savedPath);
            sb.Append("/");
            sb.Append(assetName);
            sb.Append(".asset");
            var asset = AssetDatabase.LoadAssetAtPath(sb.ToString(), typeof(AnimatedCameraPathData));
            if (asset == null) {
                AssetDatabase.CreateAsset(this, sb.ToString());
                return (AnimatedCameraPathData)AssetDatabase.LoadAssetAtPath(sb.ToString(), typeof(AnimatedCameraPathData));
            }
            sb.Clear();
            sb.Append(savedPath);
            sb.Append("/");
            sb.Append(assetName);
            int index = 1;
            do {
                var sb2 = new StringBuilder(sb.ToString());
                sb2.Append(" ");
                sb2.Append(index++);
                sb2.Append(".asset");
                asset = AssetDatabase.LoadAssetAtPath(sb2.ToString(), typeof(AnimatedCameraPathData));
            } while (asset != null);
            sb.Append(" ");
            sb.Append(--index);
            sb.Append(".asset");
            AssetDatabase.CreateAsset(this, sb.ToString());
            return (AnimatedCameraPathData)AssetDatabase.LoadAssetAtPath(sb.ToString(), typeof(AnimatedCameraPathData));
        }
        public void SaveAssetToDisk()
        {
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
#endif


    }
}
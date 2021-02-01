using System.Collections;
using System.Collections.Generic;
#if (UNITY_EDITOR)
using UnityEditor;
#endif
using UnityEngine;

namespace CivilFX.UI2
{
    public static class Utilities
    {
        public static float Map(float value, float lowerLimit, float uperLimit, float lowerValue, float uperValue)
        {
            return lowerValue + ((uperValue - lowerValue) / (uperLimit - lowerLimit)) * (value - lowerLimit);
        }

        public static class CustomColor
        {
            public static Color civilGreen = new Color(0.5960785f, 0.7490196f, 0.2901961f, 1.0f);
        }


        public static class MagicNumber
        {
            public static float MPHTOKMETERPH = 0.44704f;
        }

        #region EDITOR ONLY
#if (UNITY_EDITOR)
        public static GUIStyle genericGUIStyle
        {
            get {
                var labelStyle = new GUIStyle();
                labelStyle.fontStyle = FontStyle.Bold;
                labelStyle.normal.textColor = Color.white;
                labelStyle.fontSize = 16;
                return labelStyle;
            }
        }

        public static int LocateNearestNode(List<Vector3> nodes, Vector2 mousePos)
        {
            int index = -1;
            float minDistance = float.MaxValue;

            for (int i = 0; i < nodes.Count; i++) {
                var nodeToGUI = HandleUtility.WorldToGUIPoint(nodes[i]);
                var dis = Vector2.Distance(nodeToGUI, mousePos);
                if (dis < minDistance) {
                    minDistance = dis;
                    index = i;
                }
            }
            return index;
        }

#endif
        #endregion

    }
}

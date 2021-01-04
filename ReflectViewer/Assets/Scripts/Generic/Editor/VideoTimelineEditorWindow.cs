using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class VideoTimelineEditorWindow : EditorWindow
{
    [MenuItem("Window/Video Timeline")]
    public static void ShowWindow()
    {
        var timelineWindow = EditorWindow.GetWindow(typeof(VideoTimelineEditorWindow), false, "Video Clip Create", true);
        timelineWindow.Show();
    }


    private Vector2 scrollPosition;
    private string longString = "This is a long-ish string";
    private VideoClip clip;

    //popup properties
    private int popupSelectedIndex;


    private void OnGUI()
    {
        

        Rect windowRect = position;
        float width = position.width;
        float height = position.height;

        float widthPortion = width / 3;

        //create button area
        Texture2D texture2D = new Texture2D(1, 1);
        texture2D.SetPixel(0, 0, Color.grey * 0.05f);
        texture2D.Apply();
        GUIStyle style = new GUIStyle();
        style.normal.background = texture2D;
        GUILayout.BeginArea(new Rect(0, 0, widthPortion, height), style); //one-third of width

        //serialized object section
        string[] allAssetNames = AssetDatabase.GetAllAssetPaths();
        List<string> clipDisplayNames = new List<string>();
        List<string> clipAssetNames = new List<string>();
        List<VideoClip> clipAssets = new List<VideoClip>();
        foreach (var assetName in allAssetNames) {
            if (assetName.EndsWith(".asset")) {
                VideoClip clip = AssetDatabase.LoadAssetAtPath<VideoClip>(assetName);
                if (clip != null) {
                    clipAssetNames.Add(assetName);
                    clipDisplayNames.Add(GetAssetDisplayName(assetName));
                    clipAssets.Add(clip);
                }
            }
        }
        clip = (VideoClip)EditorGUILayout.ObjectField("", clip, typeof(VideoClip), false);
        EditorGUILayout.BeginHorizontal();
        popupSelectedIndex  = EditorGUILayout.Popup(popupSelectedIndex, clipDisplayNames.ToArray());
        if (GUILayout.Button("<-", GUILayout.MaxWidth(50))) {        
            EditorGUIUtility.PingObject(clipAssets[popupSelectedIndex]);
        }
        EditorGUILayout.EndHorizontal();
        //button section
        float buttonWidth = height / 2;
        float buttonHeight = buttonWidth;
        GUILayout.BeginArea(new Rect((widthPortion / 2) - (buttonWidth / 2), (height / 2) - (buttonHeight / 2), buttonWidth, buttonHeight));
        GUILayout.Button("OK", GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));
        GUILayout.EndArea();

   
        //end area
        GUILayout.EndArea();


        //clip timeline area
        GUILayout.BeginArea(new Rect(0, 0, width - widthPortion, height)); //all left-over width

        GUILayout.EndArea();

    }

    private static string GetAssetDisplayName(string assetPath)
    {
        string result = "";

        //remove all leading directory
        result = Path.GetFileName(assetPath);

        //remove ".asset"
        result = result.Substring(0, result.LastIndexOf('.')) ;

        return result;
    }
}

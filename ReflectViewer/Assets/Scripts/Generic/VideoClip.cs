using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class ClipSnapshot
{
    public Vector3 cameraPosition;
    public Quaternion cameraRotation;
}

[CreateAssetMenu(menuName ="CivilFX/Video Asset", fileName = "New Video Clip")]
public class VideoClip : ScriptableObject
{
    public ClipSnapshot[] clipSnapshots;

#if (UNITY_EDITOR)
    [NonSerialized]
    public Image[] clipThumbnails;
#endif
}

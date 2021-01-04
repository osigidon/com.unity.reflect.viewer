using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

static class HotKeysExt
{

    [MenuItem("Tools/Toggle Inspector Lock %q")] // Ctrl + q
    static void ToggleInspectorLock()
    {
        //http://answers.unity3d.com/questions/282959/set-inspector-lock-by-code.html
        ActiveEditorTracker.sharedTracker.isLocked = !ActiveEditorTracker.sharedTracker.isLocked;
        ActiveEditorTracker.sharedTracker.ForceRebuild();
    }

    [MenuItem("Tools/Deselect Empty GameObject %#d")] // Ctrl + v
    static void DeselectNonMeshes()
    {
        var selectedObjs = new List<GameObject>(Selection.gameObjects);
        var deselectedObjs = new List<GameObject>();
        foreach (var item in selectedObjs) {
            if (item.GetComponents<Component>().Length == 1) {
                deselectedObjs.Add(item);
            }
        }
        selectedObjs.RemoveAll(i => deselectedObjs.Contains(i));
        Selection.objects = selectedObjs.ToArray();
    }


}

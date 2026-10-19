using UnityEngine;
using UnityEditor;

public static class CenterPivot
{
    [MenuItem("Tools/Center Child On Parent")]
    static void Center()
    {
        foreach (var go in Selection.gameObjects)
        {
            var r = go.GetComponentInChildren<MeshRenderer>();
            if (r == null) continue;
            Undo.RecordObject(r.transform, "Center Pivot");
            Vector3 offset = r.bounds.center - go.transform.position;
            r.transform.position -= offset;
        }
    }
}
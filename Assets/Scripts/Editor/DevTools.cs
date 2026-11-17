using UnityEditor;
using UnityEngine;

public static class DevTools
{
    [MenuItem("Tools/Unlock All Levels")]
    static void UnlockAll() { PlayerPrefs.SetInt("unlockedLevel", 5); PlayerPrefs.Save(); }

    [MenuItem("Tools/Reset Progress")]
    static void ResetProgress() { PlayerPrefs.DeleteAll(); PlayerPrefs.Save(); }
}
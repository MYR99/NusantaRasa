using UnityEngine;

public static class DisplaySettings
{
    public static bool IsFullscreen => Screen.fullScreenMode != FullScreenMode.Windowed;

    public static void Apply()
    {
#if !UNITY_EDITOR
        if (SaveSystem.Fullscreen)
        {
            Screen.SetResolution(Display.main.systemWidth, Display.main.systemHeight,
                                 FullScreenMode.FullScreenWindow);
        }
        else
        {
            int w = Mathf.Min(1600, Mathf.RoundToInt(Display.main.systemWidth * 0.8f));
            Screen.SetResolution(w, w * 9 / 16, FullScreenMode.Windowed);
        }
#endif
    }

    public static void SetFullscreen(bool on)
    {
        SaveSystem.Fullscreen = on;
        Apply();
    }
}
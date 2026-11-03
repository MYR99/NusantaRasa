using UnityEngine;

public static class SaveSystem
{
    const string UnlockedKey = "unlockedLevel";
    const string BgmKey = "bgmVolume";
    const string SfxKey = "sfxVolume";
    const string FullscreenKey = "fullscreen";

    public static int UnlockedLevel => PlayerPrefs.GetInt(UnlockedKey, 0);

    public static void Unlock(int index)
    {
        if (index <= UnlockedLevel) return;
        PlayerPrefs.SetInt(UnlockedKey, index);
        PlayerPrefs.Save();
    }

    public static float BgmVolume
    {
        get => PlayerPrefs.GetFloat(BgmKey, 0.8f);
        set => PlayerPrefs.SetFloat(BgmKey, Mathf.Clamp01(value));
    }

    public static float SfxVolume
    {
        get => PlayerPrefs.GetFloat(SfxKey, 0.8f);
        set => PlayerPrefs.SetFloat(SfxKey, Mathf.Clamp01(value));
    }

    public static bool Fullscreen
    {
        get => PlayerPrefs.GetInt(FullscreenKey, 1) == 1;
        set => PlayerPrefs.SetInt(FullscreenKey, value ? 1 : 0);
    }
}
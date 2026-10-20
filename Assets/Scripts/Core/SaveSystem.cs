using UnityEngine;

public static class SaveSystem
{
    const string UnlockedKey = "unlockedLevel";
    const string VolumeKey = "volume";

    // indeks level tertinggi yang sudah terbuka (0 = hanya level 1)
    public static int UnlockedLevel => PlayerPrefs.GetInt(UnlockedKey, 0);

    public static void Unlock(int index)
    {
        if (index <= UnlockedLevel) return;
        PlayerPrefs.SetInt(UnlockedKey, index);
        PlayerPrefs.Save();
    }

    public static float Volume
    {
        get => PlayerPrefs.GetFloat(VolumeKey, 1f);
        set => PlayerPrefs.SetFloat(VolumeKey, Mathf.Clamp01(value));
    }
}
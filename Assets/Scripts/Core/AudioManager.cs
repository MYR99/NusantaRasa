using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    [Header("BGM")]
    public AudioClip menuBgm;
    public AudioClip gameBgm;

    [Header("SFX")]
    public AudioClip click, place, invalid, rotate, refresh, win, lose;

    public void PlayBgm(AudioClip clip)
    {
        if (clip == null) return;
        if (bgmSource.clip == clip && bgmSource.isPlaying) return;   // jangan restart kalau sama
        bgmSource.clip = clip;
        bgmSource.loop = true;
        bgmSource.Play();
    }

    void OnEnable()  => SceneManager.sceneLoaded += OnSceneLoaded;
    void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

    // setiap scene dimuat: semua Button (termasuk yang nonaktif) otomatis dapat suara klik
    void OnSceneLoaded(Scene s, LoadSceneMode m)
    {
        // musik per scene
        if (s.name == SceneNames.MainMenu || s.name == SceneNames.LevelSelect) PlayBgm(menuBgm);
        else if (s.name != SceneNames.Loading) PlayBgm(gameBgm);

        // semua Button di scene ini otomatis dapat suara klik
        foreach (var b in FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            b.onClick.AddListener(PlayClick);
    }

    void PlayClick() => PlaySfx(click);
 
    public static AudioManager Instance;

    public AudioMixer mixer;
    public AudioSource bgmSource;
    public AudioSource sfxSource;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Boot()
    {
        if (Instance == null)
        {
            var prefab = Resources.Load<AudioManager>("AudioManager");
            if (prefab != null) Instantiate(prefab);
            else Debug.LogWarning("Prefab AudioManager tidak ketemu di Assets/Resources.");
        }
        DisplaySettings.Apply();
    }

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        SetBgm(SaveSystem.BgmVolume);
        SetSfx(SaveSystem.SfxVolume);
    }

    static float ToDb(float v) => v <= 0.0001f ? -80f : Mathf.Log10(v) * 20f;

    public void SetBgm(float v)
    {
        SaveSystem.BgmVolume = v;
        if (!mixer.SetFloat("BgmVol", ToDb(v)))
            Debug.LogWarning("Parameter BgmVol belum di-expose di AudioMixer.");
    }

    public void SetSfx(float v)
    {
        SaveSystem.SfxVolume = v;
        if (!mixer.SetFloat("SfxVol", ToDb(v)))
            Debug.LogWarning("Parameter SfxVol belum di-expose di AudioMixer.");
    }

    public void PlaySfx(AudioClip clip)
    {
        if (clip != null) sfxSource.PlayOneShot(clip);
    }
}
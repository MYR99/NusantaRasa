using UnityEngine;
using UnityEngine.UI;

public class SettingsPanel : MonoBehaviour
{
    public Slider bgmSlider;
    public Slider sfxSlider;
    public Toggle fullscreenToggle;

    [Header("Hanya untuk popup (kosongkan di pause panel)")]
    public Button closeButton;
    public GameObject closeTarget;   // objek yang ditutup saat Close ditekan

    void Awake()
    {
        bgmSlider.minValue = 0f; bgmSlider.maxValue = 1f;
        sfxSlider.minValue = 0f; sfxSlider.maxValue = 1f;

        bgmSlider.onValueChanged.AddListener(v => AudioManager.Instance?.SetBgm(v));
        sfxSlider.onValueChanged.AddListener(v => AudioManager.Instance?.SetSfx(v));
        fullscreenToggle.onValueChanged.AddListener(DisplaySettings.SetFullscreen);

        if (closeButton != null)
            closeButton.onClick.AddListener(() =>
                (closeTarget != null ? closeTarget : gameObject).SetActive(false));
    }

    void OnEnable()
    {
        bgmSlider.SetValueWithoutNotify(SaveSystem.BgmVolume);
        sfxSlider.SetValueWithoutNotify(SaveSystem.SfxVolume);
        fullscreenToggle.SetIsOnWithoutNotify(DisplaySettings.IsFullscreen);
    }

    void OnDisable() => PlayerPrefs.Save();
}
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameUI : MonoBehaviour
{
    public GameManager game;

    [Header("HUD")]
    public TMP_Text levelText;
    public RectTransform progressFill;   // anak ProgressBar, anchor min (0,0) max (0,1)
    public TMP_Text progressText;
    public Button refreshButton;
    public TMP_Text refreshCountText;
    public Button pauseButton;

    [Header("Panel Pause")]
    public GameObject pausePanel;
    public TMP_Text pauseLevelText;      // teks di dalam CurrentLevel (BUKAN Title)
    public Button resumeButton;
    public Button pauseRestartButton;
    public Button pauseMainMenuButton;
    public Slider volumeSlider;
    public Button soundButton;       // objek "Sound" (ikon musik)
    public Image soundIcon;           // Image pada objek yang sama
    public Sprite iconSoundOn;
    public Sprite iconSoundOff;

    [Header("Panel Menang")]
    public GameObject winPanel;
    public TMP_Text sotoNameText;
    public TMP_Text compositionText;
    public TMP_Text recipeText;
    public Button nextButton;
    public TMP_Text nextButtonLabel;

    [Header("Panel Kalah")]
    public GameObject losePanel;
    public Button retryButton;

    void Awake()
    {
        // pemeriksaan dasar: kalau salah satu hilang, UI tidak akan bisa diklik
        if (FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
            Debug.LogError("Tidak ada EventSystem di scene ini: UI tidak akan bisa diklik.");
        if (GetComponent<GraphicRaycaster>() == null)
            Debug.LogError("Canvas tidak punya Graphic Raycaster: UI tidak akan bisa diklik.");

        // berlangganan di Awake supaya event pertama dari GameManager.Start tidak terlewat
        game.OnChanged += Redraw;

        refreshButton.onClick.AddListener(game.Refresh);
        pauseButton.onClick.AddListener(game.TogglePause);
        resumeButton.onClick.AddListener(() => game.SetPaused(false));
        pauseRestartButton.onClick.AddListener(game.StartLevel);
        pauseMainMenuButton.onClick.AddListener(game.ReturnToMenu);
        nextButton.onClick.AddListener(game.NextLevel);
        retryButton.onClick.AddListener(game.StartLevel);

                // suara
        volumeSlider.minValue = 0f;
        volumeSlider.maxValue = 1f;
        volumeSlider.wholeNumbers = false;
        volumeSlider.SetValueWithoutNotify(SaveSystem.Volume);
        volumeSlider.onValueChanged.AddListener(v =>
        {
            SaveSystem.Volume = v;
            if (v > 0.001f) SaveSystem.Muted = false;   // geser ke atas 0 = suara nyala lagi
            RefreshAudio();
        });

        if (soundButton != null)
            soundButton.onClick.AddListener(() =>
            {
                SaveSystem.Muted = !SaveSystem.Muted;
                if (!SaveSystem.Muted && SaveSystem.Volume <= 0.001f)
                {
                    SaveSystem.Volume = 0.5f;               // dinyalakan dari volume 0: beri nilai wajar
                    volumeSlider.SetValueWithoutNotify(0.5f);
                }
                RefreshAudio();
            });

        RefreshAudio();

        // semua panel tersembunyi di awal
        pausePanel.SetActive(false);
        winPanel.SetActive(false);
        losePanel.SetActive(false);
    }

        void RefreshAudio()
    {
        SaveSystem.ApplyAudio();
        bool off = SaveSystem.Muted || SaveSystem.Volume <= 0.001f;
        if (soundIcon != null) soundIcon.sprite = off ? iconSoundOff : iconSoundOn;
    }

    void OnDestroy()
    {
        if (game != null) game.OnChanged -= Redraw;
        PlayerPrefs.Save();
    }

    void Redraw()
    {
        var cfg = game.CurrentConfig;
        var state = game.state;

        pausePanel.SetActive(state == GameManager.State.Playing && game.IsPaused);
        winPanel.SetActive(state == GameManager.State.Won);
        losePanel.SetActive(state == GameManager.State.Lost);

        levelText.text = $"Level {game.LevelNumber}/{game.LevelTotal}";
        pauseLevelText.text = $"Level {game.LevelNumber}";

        float p = Mathf.Clamp01(game.Progress);
        progressFill.anchorMax = new Vector2(p, 1f);
        progressText.text = Mathf.RoundToInt(p * 100f) + "%";

        refreshCountText.text = game.RefreshesLeft.ToString();
        refreshButton.interactable = game.CanRefresh;
        pauseButton.interactable = state == GameManager.State.Playing;

        if (state == GameManager.State.Won && cfg != null)
        {
            sotoNameText.text = cfg.sotoName;
            compositionText.text = cfg.composition;
            recipeText.text = cfg.recipe;
            nextButtonLabel.text = game.HasNextLevel ? "Level berikutnya" : "Pilih level";
        }
    }
}
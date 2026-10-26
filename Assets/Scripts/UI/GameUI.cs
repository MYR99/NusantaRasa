using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameUI : MonoBehaviour
{
    public GameManager game;

    [Header("HUD")]
    public GameObject hudGroup;
    public TMP_Text levelText;
    public RectTransform progressFill;   // anak ProgressBar, anchor min (0,0) max (0,1)
    public TMP_Text progressText;
    public Button refreshButton;
    public TMP_Text refreshCountText;
    public Button pauseButton;

    [Header("Panel Pause")]
    public GameObject pausePanel;
    public TMP_Text pauseLevelText;
    public Button resumeButton;          // tombol "Next" (panah kanan) di PausePanel
    public Button pauseRestartButton;
    public Button pauseMainMenuButton;
    public Slider volumeSlider;

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

    [Header("Menu Utama")]
    public GameObject mainMenuPanel;
    public Button playButton;
    public Button quitButton;

    [Header("Pilih Level")]
    public GameObject levelSelectPanel;
    public Button[] levelButtons;        // urut: level 1 sampai 7
    public Button levelBackButton;

    bool inLevelSelect;

    void Awake()
    {
        // berlangganan di Awake supaya event pertama dari GameManager.Start tidak terlewat
        game.OnChanged += Redraw;

        // HUD dan panel
        refreshButton.onClick.AddListener(game.Refresh);
        pauseButton.onClick.AddListener(game.TogglePause);
        resumeButton.onClick.AddListener(() => game.SetPaused(false));
        pauseRestartButton.onClick.AddListener(game.StartLevel);
        pauseMainMenuButton.onClick.AddListener(game.ReturnToMenu);
        nextButton.onClick.AddListener(game.NextLevel);
        retryButton.onClick.AddListener(game.StartLevel);

        // menu utama dan pemilih level
        playButton.onClick.AddListener(() => { inLevelSelect = true; Redraw(); });
        levelBackButton.onClick.AddListener(() => { inLevelSelect = false; Redraw(); });
        quitButton.onClick.AddListener(Quit);

        for (int i = 0; i < levelButtons.Length; i++)
        {
            int index = i;   // salin nilai i untuk lambda
            levelButtons[i].onClick.AddListener(() => game.StartLevelAt(index));
            var label = levelButtons[i].GetComponentInChildren<TMP_Text>();
            if (label != null) label.text = (i + 1).ToString();
        }

        // volume
        volumeSlider.minValue = 0f;
        volumeSlider.maxValue = 1f;
        volumeSlider.wholeNumbers = false;
        volumeSlider.SetValueWithoutNotify(SaveSystem.Volume);
        AudioListener.volume = SaveSystem.Volume;
        volumeSlider.onValueChanged.AddListener(v =>
        {
            AudioListener.volume = v;
            SaveSystem.Volume = v;
        });
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
        bool inMenu = state == GameManager.State.Menu;
        if (!inMenu) inLevelSelect = false;

        hudGroup.SetActive(!inMenu);
        mainMenuPanel.SetActive(inMenu && !inLevelSelect);
        levelSelectPanel.SetActive(inMenu && inLevelSelect);
        pausePanel.SetActive(state == GameManager.State.Playing && game.IsPaused);
        winPanel.SetActive(state == GameManager.State.Won);
        losePanel.SetActive(state == GameManager.State.Lost);

        if (inMenu)
        {
            if (inLevelSelect) UpdateLevelButtons();
            return;
        }

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

    void UpdateLevelButtons()
    {
        for (int i = 0; i < levelButtons.Length; i++)
        {
            bool available = i < game.LevelTotal;               // level ini punya LevelConfig
            bool unlocked = i <= SaveSystem.UnlockedLevel;      // sudah dibuka lewat progres
            levelButtons[i].interactable = available && unlocked;
        }
    }

    void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
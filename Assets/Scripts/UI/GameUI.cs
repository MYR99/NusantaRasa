using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameUI : MonoBehaviour
{
    public GameManager game;

    [Header("HUD")]
    public TMP_Text levelText;
    public RectTransform progressFill;
    public TMP_Text progressText;
    public Button refreshButton;
    public TMP_Text refreshCountText;
    public Button pauseButton;

    [Header("Panel Pause")]
    public GameObject pausePanel;
    public TMP_Text pauseLevelText;
    public Button resumeButton;
    public Button pauseRestartButton;
    public Button pauseMainMenuButton;

    [Header("Panel Menang")]
    public GameObject winPanel;
    public TMP_Text sotoNameText;
    public TMP_Text compositionText;
    public SotoViewer sotoViewer;
    public Button nextButton;
    public TMP_Text nextButtonLabel;

    [Header("Panel Kalah")]
    public GameObject losePanel;
    public Button retryButton;

    LevelConfig shownSoto;

    void Awake()
    {
        if (game == null) game = FindFirstObjectByType<GameManager>();
        if (FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
            Debug.LogError("Tidak ada EventSystem di scene ini: UI tidak akan bisa diklik.");
        if (GetComponent<GraphicRaycaster>() == null)
            Debug.LogError("Canvas tidak punya Graphic Raycaster: UI tidak akan bisa diklik.");

        game.OnChanged += Redraw;

        refreshButton.onClick.AddListener(game.Refresh);
        pauseButton.onClick.AddListener(game.TogglePause);
        resumeButton.onClick.AddListener(() => game.SetPaused(false));
        pauseRestartButton.onClick.AddListener(game.StartLevel);
        pauseMainMenuButton.onClick.AddListener(game.ReturnToMenu);
        nextButton.onClick.AddListener(game.NextLevel);
        retryButton.onClick.AddListener(game.StartLevel);

        pausePanel.SetActive(false);
        winPanel.SetActive(false);
        losePanel.SetActive(false);
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
            nextButtonLabel.text = game.HasNextLevel ? "Level berikutnya" : "Pilih level";

            if (shownSoto != cfg)               // Redraw bisa terpanggil berulang: tampilkan sekali saja
            {
                sotoViewer.Show(cfg.sotoPrefab);
                shownSoto = cfg;
            }
        }
        else if (state != GameManager.State.Won)
        {
            shownSoto = null;
        }
    }
}
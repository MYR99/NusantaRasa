using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public enum State { Menu, Playing, Won, Lost }

    public GameObject cellPrefab;
    public Material material;
    public List<LevelConfig> levels = new List<LevelConfig>();
    public float trayZ = -3f;

    public State state { get; private set; } = State.Menu;
    public bool IsPaused { get; private set; }
    public bool Playing => state == State.Playing && !IsPaused;

    // untuk UI
    public event System.Action OnChanged;
    public LevelConfig CurrentConfig => cfg;
    public int LevelNumber => levelIndex + 1;
    public int LevelTotal => Mathf.Max(1, levels.Count);
    public bool IsLastLevel => levelIndex >= LevelTotal - 1;
    public int RefreshesLeft => refreshesLeft;
    public bool CanRefresh => Playing && refreshesLeft > 0 && unseen.Count > 0;
    public float Progress => board != null && board.TotalCells > 0
        ? (float)board.FilledCount / board.TotalCells : 0f;

    GridBoard board;
    LevelConfig fallback, cfg;
    int levelIndex;
    Piece[] tray = new Piece[0];
    List<ShapeType> unseen = new List<ShapeType>();
    int refreshesLeft;

    void Awake()
    {
        Instance = this;
        board = GetComponent<GridBoard>();
        fallback = LevelConfig.CreateDefault();
        cfg = levels.Count > 0 ? levels[0] : fallback;
    }

    void Start()
    {
        board.OnPlaced += HandlePlaced;
        OnChanged?.Invoke();   // tampilkan menu utama
    }

    void Update()
    {
        if (PointerInput.PausePressed) TogglePause();
    }

    // ---------- alur game ----------

    public void StartLevelAt(int index)
    {
        levelIndex = Mathf.Clamp(index, 0, LevelTotal - 1);
        StartLevel();
    }

    // mulai (atau ulang) level yang sedang dipilih
    public void StartLevel()
    {
        IsPaused = false;
        cfg = levels.Count > 0 ? levels[Mathf.Clamp(levelIndex, 0, levels.Count - 1)] : fallback;

        foreach (var p in FindObjectsByType<Piece>(FindObjectsSortMode.None))
            Destroy(p.gameObject);

        board.Configure(cfg.width, cfg.height, cfg.cellX, cfg.cellZ);
        refreshesLeft = cfg.refreshes;
        tray = new Piece[cfg.trayCount];

        var list = LevelGenerator.Generate(board.width, board.height, cfg.shapes, cfg.maxSingles);
        if (list == null)
        {
            Debug.LogError($"Generator gagal untuk {cfg.name}. Cek daftar bentuk dan Max Singles.");
            return;
        }
        Debug.Log($"[{cfg.name}] {list.Count} piece -> {string.Join(", ", list)}");

        LevelGenerator.Shuffle(list);
        unseen = list;

        for (int i = 0; i < tray.Length; i++) DrawIntoSlot(i);

        state = State.Playing;
        CheckState();
        OnChanged?.Invoke();
    }

    public void NextLevel()
    {
        levelIndex = IsLastLevel ? 0 : levelIndex + 1;
        StartLevel();
    }

    public void ReturnToMenu()
    {
        foreach (var p in FindObjectsByType<Piece>(FindObjectsSortMode.None))
            Destroy(p.gameObject);

        board.Clear();
        tray = new Piece[0];
        unseen.Clear();
        IsPaused = false;
        state = State.Menu;
        OnChanged?.Invoke();
    }

    // ---------- pause ----------

    public void SetPaused(bool paused)
    {
        if (state != State.Playing || IsPaused == paused) return;
        IsPaused = paused;
        OnChanged?.Invoke();
    }

    public void TogglePause() => SetPaused(!IsPaused);

    // ---------- piece ----------

    void DrawIntoSlot(int i)
    {
        if (unseen.Count == 0) { tray[i] = null; return; }

        ShapeType type = unseen[0];
        unseen.RemoveAt(0);

        var go = new GameObject("Piece_" + type);
        float cx = board.width * 0.5f + (i - (tray.Length - 1) * 0.5f) * 3.5f;
        go.transform.position = board.transform.TransformPoint(
            new Vector3(cx * board.cellX, 0, trayZ * board.cellZ));

        var p = go.AddComponent<Piece>();
        p.shape = type;
        p.Build(Shapes.Get(type), cellPrefab, material, board.cellX, board.cellZ);
        tray[i] = p;
    }

    void HandlePlaced(Piece p)
    {
        int i = System.Array.IndexOf(tray, p);
        if (i >= 0)
        {
            tray[i] = null;
            DrawIntoSlot(i);
        }
        CheckState();
        OnChanged?.Invoke();
    }

    public void Refresh()
    {
        if (!CanRefresh) return;
        refreshesLeft--;

        for (int i = 0; i < tray.Length; i++)
        {
            if (tray[i] == null) continue;
            unseen.Add(tray[i].shape);
            Destroy(tray[i].gameObject);
            tray[i] = null;
        }
        LevelGenerator.Shuffle(unseen);
        for (int i = 0; i < tray.Length; i++) DrawIntoSlot(i);

        CheckState();
        OnChanged?.Invoke();
    }

    // ---------- menang / kalah ----------

    void CheckState()
    {
        if (board.IsFull)
        {
            state = State.Won;
            SaveSystem.Unlock(levelIndex + 1);   // buka level berikutnya
            return;
        }

        bool visibleFits = false;
        foreach (var p in tray)
            if (p != null && FitsAnywhere(p.shape)) { visibleFits = true; break; }

        bool poolFits = visibleFits;
        if (!poolFits)
            foreach (var t in unseen)
                if (FitsAnywhere(t)) { poolFits = true; break; }

        bool refreshUseful = refreshesLeft > 0 && unseen.Count > 0;

        if (!poolFits || (!visibleFits && !refreshUseful))
            state = State.Lost;
    }

    bool FitsAnywhere(ShapeType type)
    {
        foreach (var o in LevelGenerator.Orientations(type))
            if (board.CanFit(o)) return true;
        return false;
    }
}
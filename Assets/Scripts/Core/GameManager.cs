using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public enum State { Menu, Playing, Won, Lost }

    public GameObject cellPrefab;
    public Material material;
    public List<LevelConfig> levels = new List<LevelConfig>();   // level di scene ini
    public float trayZ = -3f;

    [Header("Scene ini")]
    public int globalOffset = 0;           // nomor global level pertama di scene ini (Stage 1-3 = 0, Stage 4-5 = 3, Stage 6-7 = 5)
    public bool startImmediately = true;
    public int debugStartLevel = -1;       // -1 = pakai level dari pemilih level; isi angka untuk tes langsung

    public State state { get; private set; } = State.Menu;
    public bool IsPaused { get; private set; }
    public bool HelpOpen { get; private set; }
    public bool Playing => state == State.Playing && !IsPaused && !HelpOpen;

    // untuk UI
    public event System.Action OnChanged;
    public LevelConfig CurrentConfig => cfg;
    public int LevelNumber => globalOffset + levelIndex + 1;
    public int LevelTotal => StageCatalog.Count;
    public bool HasNextLevel =>
        levelIndex + 1 < levels.Count || StageCatalog.IsAvailable(globalOffset + levelIndex + 1);
    public int RefreshesLeft => refreshesLeft;
    public int UnseenCount => unseen.Count;
    public bool RefreshAvailable => refreshesLeft > 0 && HasRefreshChoice();
    public bool CanRefresh => Playing && RefreshAvailable;
    public float Progress => board != null && board.TotalCells > 0
        ? (float)board.FilledCount / board.TotalCells : 0f;

    GridBoard board;
    int refreshesLeft;
    float traySpacing = 3.5f;
    float trayDepth = 3f;
    LevelConfig fallback, cfg;
    int levelIndex;                         // indeks lokal di scene ini
    Piece[] tray = new Piece[0];
    List<ShapeType> unseen = new List<ShapeType>();

    void Awake()
    {
        Instance = this;
        board = GetComponent<GridBoard>();
        fallback = LevelConfig.CreateDefault();
        cfg = levels.Count > 0 ? levels[0] : fallback;
        board.Configure(cfg.width, cfg.height, cfg.cellX, cfg.cellZ);
    }

    void Start()
    {
        board.OnPlaced += HandlePlaced;

        if (startImmediately)
        {
            int global = debugStartLevel >= 0 ? debugStartLevel : GameSession.SelectedLevel;
            StartLevelAt(global - globalOffset);
        }
        else OnChanged?.Invoke();
    }

    void Update()
    {
        if (PointerInput.PausePressed)
        {
            if (HelpOpen) SetHelp(false);
            else TogglePause();
        }
    }

    public void SetHelp(bool open)
    {
        if (HelpOpen == open) return;
        if (open && (state != State.Playing || IsPaused)) return;
        HelpOpen = open;
        OnChanged?.Invoke();
    }

    // ---------- alur game ----------

    public void StartLevelAt(int localIndex)
    {
        levelIndex = Mathf.Clamp(localIndex, 0, Mathf.Max(1, levels.Count) - 1);
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
        board.Configure(cfg.width, cfg.height, cfg.cellX, cfg.cellZ);
        ComputeTray();
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
        int nextGlobal = globalOffset + levelIndex + 1;

        if (levelIndex + 1 < levels.Count)               // masih ada level di scene ini
        {
            levelIndex++;
            GameSession.SelectedLevel = nextGlobal;
            StartLevel();
        }
        else if (StageCatalog.IsAvailable(nextGlobal))   // level berikutnya ada di scene lain
        {
            GameSession.SelectedLevel = nextGlobal;
            SceneLoader.Load(StageCatalog.SceneFor(nextGlobal));
        }
        else                                             // tidak ada lagi: kembali ke pemilih level
        {
            SceneLoader.Load(SceneNames.LevelSelect);
        }
    }

    public void ReturnToMenu() => SceneLoader.Load(SceneNames.MainMenu);

    // ---------- pause ----------

    public void SetPaused(bool paused)
    {
        if (state != State.Playing || IsPaused == paused) return;
        if (paused && HelpOpen) return;
        IsPaused = paused;
        OnChanged?.Invoke();
    }

    public void TogglePause() => SetPaused(!IsPaused);

    // ---------- piece ----------

    void ComputeTray()
    {
        int m = 1;
        foreach (var sw in cfg.shapes)
            foreach (var o in Shapes.Get(sw.type))
                m = Mathf.Max(m, o.x + 1, o.y + 1);
        traySpacing = Mathf.Max(3.5f, m + 0.5f);
        trayDepth   = Mathf.Max(-trayZ, m * 0.5f + 1.5f);
    }

    void DrawIntoSlot(int i)
    {
        if (unseen.Count == 0) { tray[i] = null; return; }

        ShapeType type = unseen[0];
        unseen.RemoveAt(0);

        var go = new GameObject("Piece_" + type);
        float cx = board.width * 0.5f + (i - (tray.Length - 1) * 0.5f) * traySpacing;
        go.transform.position = board.transform.TransformPoint(
            new Vector3(cx * board.cellX, 0, -trayDepth * board.cellZ));

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

    bool HasRefreshChoice()
    {
        if (unseen.Count == 0) return false;
        var types = new HashSet<ShapeType>(unseen);
        foreach (var t in tray) if (t != null) types.Add(t.shape);
        return types.Count > 1;   // pool satu jenis = tidak mungkin berubah
    }

    public void Refresh()
    {
        if (!CanRefresh) return;
        refreshesLeft--;

        var before = new List<ShapeType>();
        foreach (var t in tray) if (t != null) before.Add(t.shape);
        before.Sort();

        for (int i = 0; i < tray.Length; i++)
        {
            if (tray[i] == null) continue;
            unseen.Add(tray[i].shape);
            Destroy(tray[i].gameObject);
            tray[i] = null;
        }

        // acak ulang sampai tray baru berbeda dari yang lama
        for (int k = 0; k < 20; k++)
        {
            LevelGenerator.Shuffle(unseen);
            var next = unseen.GetRange(0, Mathf.Min(tray.Length, unseen.Count));
            next.Sort();
            if (!next.SequenceEqual(before)) break;
        }

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
            SaveSystem.Unlock(globalOffset + levelIndex + 1);   // buka level global berikutnya
            AudioManager.Instance?.PlaySfx(AudioManager.Instance.win);
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
        {
            state = State.Lost;
            AudioManager.Instance?.PlaySfx(AudioManager.Instance.lose);
        }
    }

    bool FitsAnywhere(ShapeType type)
    {
        foreach (var o in LevelGenerator.Orientations(type))
            if (board.CanFit(o)) return true;
        return false;
    }
}
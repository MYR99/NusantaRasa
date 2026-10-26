using UnityEngine;
using UnityEngine.SceneManagement;

public static class SceneNames
{
    public const string Loading = "Loading";
    public const string MainMenu = "MainMenu";
    public const string LevelSelect = "LevelSelect";
}

// Scene untuk tiap level global (urut level 1 sampai 7).
// Nama scene harus sama persis dengan nama file scene.
public static class StageCatalog
{
    static readonly string[] scenes =
    {
        "Stage 1-3", "Stage 1-3", "Stage 1-3",
        "Stage 4-5", "Stage 4-5",
        "Stage 6-7", "Stage 6-7",
    };

    public static int Count => scenes.Length;

    public static string SceneFor(int level) =>
        scenes[Mathf.Clamp(level, 0, scenes.Length - 1)];

    // true kalau scene level ini sudah ada di Build Profiles
    public static bool IsAvailable(int level) =>
        level >= 0 && level < scenes.Length && Application.CanStreamedLevelBeLoaded(scenes[level]);
}

// Level global (0 = Level 1) yang dipilih di LevelSelect, dibaca oleh scene game.
public static class GameSession
{
    public static int SelectedLevel;
}

public static class SceneLoader
{
    public static void Load(string sceneName) => SceneManager.LoadScene(sceneName);
}
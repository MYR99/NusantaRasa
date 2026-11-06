using UnityEditor;
using UnityEngine;

public static class GeneratorTest
{
    [MenuItem("Tools/Test Generator (Semua Level)")]
    static void Run()
    {
        foreach (var guid in AssetDatabase.FindAssets("t:LevelConfig"))
        {
            var cfg = AssetDatabase.LoadAssetAtPath<LevelConfig>(
                AssetDatabase.GUIDToAssetPath(guid));
            Test(cfg, 100);
        }
    }

    static void Test(LevelConfig c, int runs)
    {
        int ok = 0, minP = int.MaxValue, maxP = 0;
        long sumP = 0, sumMs = 0, worst = 0;

        for (int i = 0; i < runs; i++)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            var r = LevelGenerator.Generate(c.width, c.height, c.shapes, c.maxSingles);
            sw.Stop();

            sumMs += sw.ElapsedMilliseconds;
            if (sw.ElapsedMilliseconds > worst) worst = sw.ElapsedMilliseconds;
            if (r == null) continue;

            ok++;
            sumP += r.Count;
            minP = Mathf.Min(minP, r.Count);
            maxP = Mathf.Max(maxP, r.Count);
        }

        string pieces = ok > 0 ? $"piece {sumP / ok} (min {minP}, maks {maxP})" : "-";
        Debug.Log($"[GenTest] {c.name} {c.width}x{c.height}: berhasil {ok}/{runs} | " +
                  $"rata {sumMs / runs} ms, terburuk {worst} ms | {pieces}");
    }
}
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class BotTest
{
    static int budget;

    [MenuItem("Tools/Bot Playtest")]
    static void Run()
    {
        foreach (var guid in AssetDatabase.FindAssets("t:LevelConfig"))
        {
            var cfg = AssetDatabase.LoadAssetAtPath<LevelConfig>(AssetDatabase.GUIDToAssetPath(guid));
            int win = 0, lose = 0, unknown = 0, n = 50;
            for (int i = 0; i < n; i++)
            {
                var r = Solve(cfg);
                if (r == 1) win++; else if (r == 0) lose++; else unknown++;
            }
            Debug.Log($"[Solver] {cfg.name}: bisa menang {win} | pasti kalah {lose} | tak tentu {unknown} (dari {n})");
        }
    }

    // 1 = ada jalan menang, 0 = tidak ada, -1 = budget habis
    static int Solve(LevelConfig c)
    {
        var pool = LevelGenerator.Generate(c.width, c.height, c.shapes, c.maxSingles);
        if (pool == null) return 0;
        LevelGenerator.Shuffle(pool);

        var tray = new List<ShapeType>();
        int next = 0;
        while (tray.Count < c.trayCount && next < pool.Count) tray.Add(pool[next++]);

        budget = 300000;
        var g = new bool[c.width, c.height];
        bool ok = Dfs(g, c.width, c.height, tray, next, pool);
        if (ok) return 1;
        return budget <= 0 ? -1 : 0;
    }

    static bool Dfs(bool[,] g, int w, int h, List<ShapeType> tray, int next, List<ShapeType> pool)
    {
        if (budget-- <= 0) return false;

        int fx = -1, fy = -1;
        for (int y = 0; y < h && fx < 0; y++)
            for (int x = 0; x < w; x++)
                if (!g[x, y]) { fx = x; fy = y; break; }
        if (fx < 0) return true;

        var tried = new HashSet<ShapeType>();
        for (int s = 0; s < tray.Count; s++)
        {
            if (!tried.Add(tray[s])) continue;   // tipe yang sama cukup dicoba sekali

            foreach (var o in LevelGenerator.Orientations(tray[s]))
            {
                var a = new Vector2Int(fx - o[0].x, fy - o[0].y);
                if (!Fits(g, w, h, o, a)) continue;

                Set(g, o, a, true);
                var nt = new List<ShapeType>(tray);
                int nn = next;
                if (nn < pool.Count) nt[s] = pool[nn++]; else nt.RemoveAt(s);

                if (Dfs(g, w, h, nt, nn, pool)) return true;
                Set(g, o, a, false);
                if (budget <= 0) return false;
            }
        }
        return false;
    }

    static bool Fits(bool[,] g, int w, int h, Vector2Int[] o, Vector2Int a)
    {
        foreach (var v in o)
        {
            int x = a.x + v.x, y = a.y + v.y;
            if (x < 0 || y < 0 || x >= w || y >= h || g[x, y]) return false;
        }
        return true;
    }

    static void Set(bool[,] g, Vector2Int[] o, Vector2Int a, bool val)
    {
        foreach (var v in o) g[a.x + v.x, a.y + v.y] = val;
    }
}
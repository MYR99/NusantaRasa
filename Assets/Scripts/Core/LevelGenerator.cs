using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ShapeWeight
{
    public ShapeType type;
    public float weight = 1f;
}

public static class LevelGenerator
{
    static readonly Dictionary<ShapeType, List<Vector2Int[]>> cache =
        new Dictionary<ShapeType, List<Vector2Int[]>>();

    // Semua orientasi unik (rotasi saja, tanpa cermin) dari sebuah bentuk.
    // Setiap orientasi sudah dinormalisasi dan diurut: elemen [0] = sel pertama (baris terbawah, paling kiri).
    public static List<Vector2Int[]> Orientations(ShapeType t)
    {
        if (cache.TryGetValue(t, out var cached)) return cached;

        var result = new List<Vector2Int[]>();
        var seen = new HashSet<string>();
        Vector2Int[] cur = Shapes.Get(t);

        for (int r = 0; r < 4; r++)
        {
            cur = NormalizeSorted(cur);
            string key = string.Join(";", System.Array.ConvertAll(cur, v => v.x + "," + v.y));
            if (seen.Add(key)) result.Add(cur);
            cur = RotateCW(cur);
        }

        cache[t] = result;
        return result;
    }

    // Hasil: daftar bentuk yang dijamin menutup seluruh papan. null kalau gagal.
    public static List<ShapeType> Generate(int width, int height, List<ShapeWeight> shapes,
                                           int maxSingles, int maxAttempts = 100)
    {
        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            var grid = new bool[width, height];
            var result = new List<ShapeType>();
            int budget = 20000;
            if (Fill(grid, width, height, shapes, maxSingles, 0, result, ref budget))
                return result;
        }
        return null;
    }

    static bool Fill(bool[,] g, int w, int h, List<ShapeWeight> shapes, int maxSingles,
                     int singles, List<ShapeType> result, ref int budget)
    {
        if (budget-- <= 0) return false;

        // sel kosong pertama: baris demi baris dari bawah, kiri ke kanan
        int fx = -1, fy = -1;
        for (int y = 0; y < h && fx < 0; y++)
            for (int x = 0; x < w; x++)
                if (!g[x, y]) { fx = x; fy = y; break; }
        if (fx < 0) return true;   // papan penuh

        foreach (var type in WeightedOrder(shapes))
        {
            if (type == ShapeType.Single && singles >= maxSingles) continue;

            var orients = new List<Vector2Int[]>(Orientations(type));
            Shuffle(orients);

            foreach (var o in orients)
            {
                // sel pertama bentuk dipasang di sel kosong pertama
                var anchor = new Vector2Int(fx - o[0].x, fy - o[0].y);
                if (!Fits(g, w, h, o, anchor)) continue;

                Set(g, o, anchor, true);
                result.Add(type);

                int s = singles + (type == ShapeType.Single ? 1 : 0);
                if (Fill(g, w, h, shapes, maxSingles, s, result, ref budget)) return true;

                result.RemoveAt(result.Count - 1);
                Set(g, o, anchor, false);
            }
        }
        return false;
    }

    static bool Fits(bool[,] g, int w, int h, Vector2Int[] o, Vector2Int anchor)
    {
        foreach (var v in o)
        {
            int x = anchor.x + v.x, y = anchor.y + v.y;
            if (x < 0 || y < 0 || x >= w || y >= h || g[x, y]) return false;
        }
        return true;
    }

    static void Set(bool[,] g, Vector2Int[] o, Vector2Int anchor, bool value)
    {
        foreach (var v in o) g[anchor.x + v.x, anchor.y + v.y] = value;
    }

    // urutan acak berbobot: bobot lebih besar = lebih sering di depan
    static List<ShapeType> WeightedOrder(List<ShapeWeight> shapes)
    {
        var keyed = new List<KeyValuePair<float, ShapeType>>();
        foreach (var s in shapes)
            if (s.weight > 0f)
                keyed.Add(new KeyValuePair<float, ShapeType>(
                    Mathf.Pow(Random.value, 1f / s.weight), s.type));
        keyed.Sort((a, b) => b.Key.CompareTo(a.Key));

        var order = new List<ShapeType>();
        foreach (var k in keyed) order.Add(k.Value);
        return order;
    }

    static Vector2Int[] RotateCW(Vector2Int[] a)
    {
        var r = new Vector2Int[a.Length];
        for (int i = 0; i < a.Length; i++) r[i] = new Vector2Int(a[i].y, -a[i].x);
        return r;
    }

    static Vector2Int[] NormalizeSorted(Vector2Int[] a)
    {
        int mx = int.MaxValue, my = int.MaxValue;
        foreach (var v in a) { mx = Mathf.Min(mx, v.x); my = Mathf.Min(my, v.y); }
        var r = new Vector2Int[a.Length];
        for (int i = 0; i < a.Length; i++) r[i] = new Vector2Int(a[i].x - mx, a[i].y - my);
        System.Array.Sort(r, (p, q) => p.y != q.y ? p.y.CompareTo(q.y) : p.x.CompareTo(q.x));
        return r;
    }

    public static void Shuffle<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}
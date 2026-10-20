using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "NusantaRasa/Level Config", fileName = "Level_01")]
public class LevelConfig : ScriptableObject
{
    [Header("Papan")]
    public int width = 8;
    public int height = 8;
    public float cellX = 0.148f;
    public float cellZ = 0.148f;

    [Header("Piece")]
    public List<ShapeWeight> shapes = new List<ShapeWeight>();
    public int maxSingles = 2;   // batas piece 1x1 per level
    public int trayCount = 3;    // jumlah piece yang tampil sekaligus
    public int refreshes = 3;

    [Header("Soto (ditampilkan saat menang)")]
    public string sotoName;
    [TextArea(2, 6)] public string composition;
    [TextArea(3, 10)] public string recipe;

    void Reset() => ApplyDefaults();

    public void ApplyDefaults()
    {
        shapes = new List<ShapeWeight>
        {
            new ShapeWeight { type = ShapeType.Block,  weight = 1f },
            new ShapeWeight { type = ShapeType.L,      weight = 2f },
            new ShapeWeight { type = ShapeType.Domino, weight = 2f },
            new ShapeWeight { type = ShapeType.Single, weight = 1f },
        };
    }

    // dipakai kalau belum ada aset level yang dipasang di GameManager
    public static LevelConfig CreateDefault()
    {
        var c = CreateInstance<LevelConfig>();
        c.ApplyDefaults();
        c.sotoName = "(isi nama Soto)";
        return c;
    }
}
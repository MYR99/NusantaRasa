using UnityEngine;

public class GridBoard : MonoBehaviour
{
    public int width = 8, height = 8;
    public float cellX = 0.148f, cellZ = 0.148f;

    public event System.Action<Piece> OnPlaced;
    public bool IsFull => filled >= width * height;
    public int FilledCount => filled;
    public int TotalCells => width * height;

    Piece[,] cells;
    int filled;

    void Awake() => Clear();

    public void Clear()
    {
        cells = new Piece[width, height];
        filled = 0;
    }

    public void Configure(int w, int h, float cx, float cz)
    {
        width = w;
        height = h;
        cellX = cx;
        cellZ = cz;
        Clear();
    }

    public bool CanPlace(Piece p, Vector2Int anchor) => CanPlaceOffsets(p.offsets, anchor, p);

    public bool CanPlaceOffsets(Vector2Int[] offs, Vector2Int anchor, Piece self = null)
    {
        foreach (var o in offs)
        {
            var c = anchor + o;
            if (c.x < 0 || c.y < 0 || c.x >= width || c.y >= height) return false;
            if (cells[c.x, c.y] != null && cells[c.x, c.y] != self) return false;
        }
        return true;
    }

    // apakah bentuk ini muat di salah satu posisi di papan sekarang?
    public bool CanFit(Vector2Int[] offs)
    {
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                if (CanPlaceOffsets(offs, new Vector2Int(x, y))) return true;
        return false;
    }

    public Vector3 AnchorToWorld(Piece p, Vector2Int anchor)
    {
        Vector2Int min = p.Min(), size = p.Size();
        float cx = anchor.x + min.x + size.x * 0.5f;
        float cy = anchor.y + min.y + size.y * 0.5f;
        return transform.TransformPoint(new Vector3(cx * cellX, 0, cy * cellZ));
    }

    // titik pusat piece di dunia -> sel anchor terdekat
    public Vector2Int WorldToAnchor(Piece p, Vector3 worldCenter)
    {
        Vector3 l = transform.InverseTransformPoint(worldCenter);
        Vector2Int min = p.Min(), size = p.Size();
        int ax = Mathf.RoundToInt(l.x / cellX - size.x * 0.5f) - min.x;
        int ay = Mathf.RoundToInt(l.z / cellZ - size.y * 0.5f) - min.y;
        return new Vector2Int(ax, ay);
    }

    public bool Place(Piece p, Vector2Int anchor)
    {
        if (!CanPlace(p, anchor)) return false;

        foreach (var o in p.offsets) cells[anchor.x + o.x, anchor.y + o.y] = p;
        filled += p.offsets.Length;

        p.transform.position = AnchorToWorld(p, anchor);
        p.placed = true;
        OnPlaced?.Invoke(p);
        return true;
    }

    // garis kuning = alat bantu. Matikan lewat tombol Gizmos di Game view.
    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        for (int x = 0; x <= width; x++)
            Gizmos.DrawLine(
                transform.TransformPoint(new Vector3(x * cellX, 0, 0)),
                transform.TransformPoint(new Vector3(x * cellX, 0, height * cellZ)));
        for (int y = 0; y <= height; y++)
            Gizmos.DrawLine(
                transform.TransformPoint(new Vector3(0, 0, y * cellZ)),
                transform.TransformPoint(new Vector3(width * cellX, 0, y * cellZ)));
    }
}
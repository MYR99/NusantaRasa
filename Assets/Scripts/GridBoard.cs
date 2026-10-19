using UnityEngine;

public class GridBoard : MonoBehaviour
{
    public int width = 8, height = 8;
    public float cellX = 0.1f, cellZ = 0.1f;   //sesuaikan dengan garis Gizmo
    Piece[,] cells;

    void Awake() => cells = new Piece[width, height];

    public bool CanPlace(Piece p, Vector2Int anchor)
    {
        foreach (var o in p.offsets)
        {
            var c = anchor + o;
            if (c.x < 0 || c.y < 0 || c.x >= width || c.y >= height) return false;
            if (cells[c.x, c.y] != null && cells[c.x, c.y] != p) return false;
        }
        return true;
    }

    public bool Place(Piece p, Vector2Int anchor)
    {
        if (!CanPlace(p, anchor)) return false;

        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                if (cells[x, y] == p) cells[x, y] = null;

        foreach (var o in p.offsets) cells[anchor.x + o.x, anchor.y + o.y] = p;

        // pusat bounding box piece, dalam satuan sel
        Vector2Int min = p.Min(), size = p.Size();
        float cx = anchor.x + min.x + size.x * 0.5f;
        float cy = anchor.y + min.y + size.y * 0.5f;
        p.transform.position = transform.TransformPoint(
            new Vector3(cx * cellX, 0, cy * cellZ));
        return true;
    }

    // garis kuning di Scene view = alat ukur. Atur cellX/cellZ sampai garis pas dengan kotak papan.
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
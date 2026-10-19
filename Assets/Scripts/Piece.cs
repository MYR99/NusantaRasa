using UnityEngine;

public class Piece : MonoBehaviour
{
    public Vector2Int[] offsets;

    GameObject cellPrefab;
    Material material;
    float cellX, cellZ;

    public void Build(Vector2Int[] shape, GameObject cell, Material mat, float cx, float cz)
    {
        cellPrefab = cell;
        material = mat;
        cellX = cx;
        cellZ = cz;
        offsets = (Vector2Int[])shape.Clone();
        Normalize();
        Rebuild();
    }

    public void RotateCW()
    {
        for (int i = 0; i < offsets.Length; i++)
            offsets[i] = new Vector2Int(offsets[i].y, -offsets[i].x);
        Normalize();
        Rebuild();
    }

    // geser supaya offset minimum selalu (0,0)
    void Normalize()
    {
        Vector2Int m = Min();
        for (int i = 0; i < offsets.Length; i++) offsets[i] -= m;
    }

    static bool TryGetBounds(GameObject go, out Bounds b)
    {
        var rs = go.GetComponentsInChildren<Renderer>();
        b = new Bounds();
        if (rs.Length == 0) return false;
        b = rs[0].bounds;
        for (int i = 1; i < rs.Length; i++) b.Encapsulate(rs[i].bounds);
        return true;
    }

    void Rebuild()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
            Destroy(transform.GetChild(i).gameObject);

        Vector2Int size = Size();
        foreach (var o in offsets)
        {
            var c = Instantiate(cellPrefab, transform);

            // 1) ukur ukuran asli prefab (skala parent dikembalikan ke 1)
            c.transform.localScale = Vector3.one;
            c.transform.localPosition = Vector3.zero;
            if (!TryGetBounds(c, out Bounds b)) { Debug.LogError("Cell prefab tidak punya Renderer"); return; }

            // 2) skala seragam supaya lebar = 95% satu sel
            float s = (cellX * 0.95f) / b.size.x;
            c.transform.localScale = Vector3.one * s;
            TryGetBounds(c, out b);

            // 3) taruh di tengah sel, alas kubus di permukaan (y piece)
            Vector3 target = transform.TransformPoint(new Vector3(
                (o.x + 0.5f - size.x * 0.5f) * cellX, 0,
                (o.y + 0.5f - size.y * 0.5f) * cellZ));
            c.transform.position += new Vector3(
                target.x - b.center.x,
                transform.position.y - b.min.y,
                target.z - b.center.z);

            // 4) warna: isi semua slot material di semua renderer
            if (material != null)
            {
                foreach (var r in c.GetComponentsInChildren<MeshRenderer>())
                {
                    var mats = new Material[r.sharedMaterials.Length];
                    for (int k = 0; k < mats.Length; k++) mats[k] = material;
                    r.sharedMaterials = mats;
                }
            }
        }
    }

    public Vector2Int Min()
    {
        int mx = int.MaxValue, my = int.MaxValue;
        foreach (var o in offsets) { mx = Mathf.Min(mx, o.x); my = Mathf.Min(my, o.y); }
        return new Vector2Int(mx, my);
    }

    public Vector2Int Size()
    {
        int mx = int.MinValue, my = int.MinValue;
        foreach (var o in offsets) { mx = Mathf.Max(mx, o.x); my = Mathf.Max(my, o.y); }
        var min = Min();
        return new Vector2Int(mx - min.x + 1, my - min.y + 1);
    }
}
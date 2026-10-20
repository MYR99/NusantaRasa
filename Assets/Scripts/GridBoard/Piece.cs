using System.Collections.Generic;
using UnityEngine;

public class Piece : MonoBehaviour
{
    public Vector2Int[] offsets;
    public bool placed;
    public ShapeType shape;

    GameObject cellPrefab;
    Material material;
    Material outlineMaterial;
    GameObject outline;
    bool highlighted;
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

    public void RotateCCW()
    {
        for (int i = 0; i < offsets.Length; i++)
            offsets[i] = new Vector2Int(-offsets[i].y, offsets[i].x);
        Normalize();
        Rebuild();
    }

    // Highlight = garis tepi di sekeliling bentuk piece (warna cell tidak diubah)
    public void SetHighlight(bool on, Material hl)
    {
        highlighted = on;
        outlineMaterial = hl;
        BuildOutline();
    }

    // geser supaya offset minimum selalu (0,0)
    void Normalize()
    {
        Vector2Int m = Min();
        for (int i = 0; i < offsets.Length; i++) offsets[i] -= m;
    }

    void Paint(MeshRenderer r)
    {
        if (material == null) return;
        var mats = new Material[r.sharedMaterials.Length];
        for (int k = 0; k < mats.Length; k++) mats[k] = material;
        r.sharedMaterials = mats;
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
             c.SetActive(true);    

            // 1) ukur ukuran asli prefab
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

            // 4) warna
            foreach (var r in c.GetComponentsInChildren<MeshRenderer>()) Paint(r);
        }

        BuildOutline();   // outline ikut dibangun ulang setiap bentuk berubah
    }

    void BuildOutline()
    {
        if (outline != null) Destroy(outline);
        outline = null;
        if (!highlighted || outlineMaterial == null) return;

        outline = new GameObject("Outline");
        outline.transform.SetParent(transform, false);

        var set = new HashSet<Vector2Int>(offsets);
        Vector2Int size = Size();
        float t = cellX * 0.07f;   // tebal garis
        float h = cellX * 0.07f;   // tinggi garis
        Vector2Int[] dirs = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        foreach (var o in offsets)
        {
            Vector3 center = new Vector3(
                (o.x + 0.5f - size.x * 0.5f) * cellX, 0,
                (o.y + 0.5f - size.y * 0.5f) * cellZ);

            foreach (var d in dirs)
            {
                if (set.Contains(o + d)) continue;   // sisi ini menempel ke sel lain: tidak perlu garis

                Vector3 pos, scale;
                if (d.x != 0)   // sisi kiri/kanan: batang memanjang di sumbu Z
                {
                    pos = center + new Vector3(d.x * (cellX * 0.475f + t * 0.5f), h * 0.5f + 0.001f, 0);
                    scale = new Vector3(t, h, cellZ + 2f * t);
                }
                else            // sisi atas/bawah: batang memanjang di sumbu X
                {
                    pos = center + new Vector3(0, h * 0.5f + 0.001f, d.y * (cellZ * 0.475f + t * 0.5f));
                    scale = new Vector3(cellX + 2f * t, h, t);
                }

                var bar = GameObject.CreatePrimitive(PrimitiveType.Cube);
                Destroy(bar.GetComponent<Collider>());
                bar.transform.SetParent(outline.transform, false);
                bar.transform.localPosition = pos;
                bar.transform.localScale = scale;

                var r = bar.GetComponent<MeshRenderer>();
                r.sharedMaterial = outlineMaterial;
                r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                r.receiveShadows = false;
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
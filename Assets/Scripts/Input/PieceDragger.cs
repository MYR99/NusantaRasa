using UnityEngine;
using UnityEngine.EventSystems;

public class PieceDragger : MonoBehaviour
{
    public GridBoard board;
    public Camera cam;
    public Material highlightMaterial;   // material untuk garis highlight piece terpilih
    public float liftHeight = 0.05f;

    Piece selected;
    bool dragging;
    Vector3 homePos;
    bool validNow;
    Vector2Int anchorNow;

    void Awake()
    {
        if (board == null) board = GetComponent<GridBoard>();
        if (cam == null) cam = Camera.main;
    }

    void Update()
    {
        var gm = GameManager.Instance;
        if (gm != null && !gm.Playing)
        {
            // pause / menu / menang / kalah: batalkan drag yang sedang berjalan
            if (dragging)
            {
                if (selected != null) selected.transform.position = homePos;
                dragging = false;
            }
            return;
        }

        // piece bisa saja sudah dihapus (restart): bersihkan status yang basi
        if (dragging && selected == null) dragging = false;

        // klik di atas elemen UI tidak boleh dianggap klik di game
        bool overUI = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();

        if (!dragging && !overUI)
        {
            // klik kiri: pilih piece di bawah kursor (kosong = batal pilih)
            if (PointerInput.SelectPressed)
                Select(PickAt(PointerInput.Position));

            // klik kanan: pilih dan mulai drag piece di bawah kursor
            if (PointerInput.DragPressed)
            {
                Piece p = PickAt(PointerInput.Position);
                if (p != null)
                {
                    Select(p);
                    dragging = true;
                    homePos = p.transform.position;
                }
            }
        }

        // rotasi: berlaku untuk piece terpilih, sedang di-drag atau tidak
        if (selected != null)
        {
            int step = PointerInput.RotateStep;
            if (step > 0) selected.RotateCW();
            else if (step < 0) selected.RotateCCW();
        }

        if (dragging) Drag();
    }

    void Select(Piece p)
    {
        if (selected == p) return;
        if (selected != null) selected.SetHighlight(false, highlightMaterial);
        selected = p;
        if (selected != null) selected.SetHighlight(true, highlightMaterial);
    }

    Piece PickAt(Vector2 screenPos)
    {
        Ray ray = cam.ScreenPointToRay(screenPos);
        Piece best = null;
        float bestDist = float.MaxValue;

        foreach (var p in FindObjectsByType<Piece>(FindObjectsSortMode.None))
        {
            if (p.placed) continue;   // yang sudah ditaruh tidak bisa dipilih lagi
            foreach (var r in p.GetComponentsInChildren<Renderer>())
            {
                if (r.bounds.IntersectRay(ray, out float d) && d < bestDist)
                {
                    bestDist = d;
                    best = p;
                }
            }
        }
        return best;
    }

    void Drag()
    {
        validNow = false;
        var plane = new Plane(Vector3.up, board.transform.position);
        Ray ray = cam.ScreenPointToRay(PointerInput.Position);

        if (plane.Raycast(ray, out float t))
        {
            Vector3 pt = ray.GetPoint(t);
            anchorNow = board.WorldToAnchor(selected, pt);
            validNow = board.CanPlace(selected, anchorNow);

            selected.transform.position = validNow
                ? board.AnchorToWorld(selected, anchorNow)   // snap ke sel
                : pt + Vector3.up * liftHeight;              // ikut kursor
        }

        if (PointerInput.DragReleased) Drop();
    }

    void Drop()
    {
        dragging = false;
        if (validNow && board.Place(selected, anchorNow))
            Select(null);                                    // sudah ditaruh: batal pilih
        else
            selected.transform.position = homePos;           // tidak valid: balik, tetap terpilih
    }
}
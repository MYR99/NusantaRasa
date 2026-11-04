using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(RawImage))]
public class SotoViewer : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public Camera viewCam;
    public Transform pivot;
    public float targetSize = 1f;     // sisi terpanjang model dinormalkan ke ukuran ini
    public float dragSpeed = 0.4f;
    public float autoSpin = 15f;      // derajat/detik saat tidak disentuh

    RawImage output;
    RenderTexture rt;
    GameObject current;
    bool dragging;
    float yaw, pitch;

    void Awake()
    {
        output = GetComponent<RawImage>();
        rt = new RenderTexture(1024, 1024, 24, RenderTextureFormat.ARGB32);
        rt.antiAliasing = 4;
        viewCam.targetTexture = rt;
        output.texture = rt;
    }

    // kamera hanya menyala saat pop-up terlihat
    void OnEnable()  { if (viewCam != null) viewCam.enabled = true; }
    void OnDisable() { if (viewCam != null) viewCam.enabled = false; }

    void OnDestroy()
    {
        if (rt != null) { rt.Release(); Destroy(rt); }
    }

    public void Show(GameObject prefab)
    {
        if (current != null) Destroy(current);
        current = null;

        if (prefab == null)
        {
            Debug.LogWarning("sotoPrefab kosong di LevelConfig.");
            return;
        }

        yaw = 25f; pitch = 15f;
        pivot.rotation = Quaternion.identity;        // ukur bounds saat belum berputar

        current = Instantiate(prefab, pivot);
        current.SetActive(true);
        current.transform.localPosition = Vector3.zero;   // buang offset bawaan FBX (x=15, 30, dst.)
        Fit(current);
    }

    // samakan ukuran semua soto (skala prefab asli beda-beda: 109 sampai 256) dan tengahkan
    void Fit(GameObject go)
    {
        if (!TryGetBounds(go, out Bounds b)) return;

        float max = Mathf.Max(b.size.x, b.size.y, b.size.z);
        if (max <= 0.0001f) return;
        go.transform.localScale *= targetSize / max;

        TryGetBounds(go, out b);
        go.transform.position += pivot.position - b.center;
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

    void Update()
    {
        if (!dragging) yaw += autoSpin * Time.unscaledDeltaTime;
        // putar di sumbu tegak model dulu, baru miringkan ke arah kamera
        pivot.rotation = Quaternion.Euler(pitch, 0f, 0f) * Quaternion.Euler(0f, yaw, 0f);
    }

    public void OnBeginDrag(PointerEventData e) => dragging = true;
    public void OnEndDrag(PointerEventData e) => dragging = false;

    public void OnDrag(PointerEventData e)
    {
        yaw -= e.delta.x * dragSpeed;
        pitch = Mathf.Clamp(pitch + e.delta.y * dragSpeed, -10f, 60f);
    }
}
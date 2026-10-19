using UnityEngine;

public class GridTest : MonoBehaviour
{
    public GameObject cellPrefab;
    public Material material;

    GridBoard board;
    Piece lPiece;
    Vector2Int lAnchor = new Vector2Int(5, 3);

    void Start()
    {
        board = GetComponent<GridBoard>();

        Spawn(ShapeType.Block,  new Vector2Int(2, 2));   // harus berhasil✅
        Spawn(ShapeType.Domino, new Vector2Int(0, 0));   // harus berhasil✅
        lPiece = Spawn(ShapeType.L, lAnchor);            // harus berhasil✅
        Spawn(ShapeType.Single, new Vector2Int(7, 7));   // harus berhasil✅

        Spawn(ShapeType.Block,  new Vector2Int(7, 7));   // harus GAGAL: keluar papan✅
        Spawn(ShapeType.Single, new Vector2Int(3, 3));   // harus GAGAL: menimpa block✅
    }

    Piece Spawn(ShapeType type, Vector2Int anchor)
    {
        var go = new GameObject("Piece_" + type);
        var p = go.AddComponent<Piece>();
        p.Build(Shapes.Get(type), cellPrefab, material, board.cellX, board.cellZ);

        if (!board.Place(p, anchor))
        {
            Debug.LogWarning($"{type} di {anchor} tidak muat");
            Destroy(go);
            return null;
        }
        return p;
    }

    // Saat Play: klik kanan header komponen GridTest > Rotate L✅
    [ContextMenu("Rotate L")]
    void RotateL()
    {
        if (lPiece == null) return;
        lPiece.RotateCW();
        board.Place(lPiece, lAnchor);
    }
}

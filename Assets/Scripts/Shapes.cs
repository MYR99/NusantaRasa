using UnityEngine;

public enum ShapeType { Single, Domino, Block, L }

public static class Shapes
{
    public static Vector2Int[] Get(ShapeType t)
    {
        switch (t)
        {
            case ShapeType.Single:
                return new[] { new Vector2Int(0, 0) };
            case ShapeType.Domino:   // 2x1
                return new[] { new Vector2Int(0, 0), new Vector2Int(1, 0) };
            case ShapeType.Block:    // 2x2
                return new[] { new Vector2Int(0, 0), new Vector2Int(1, 0),
                               new Vector2Int(0, 1), new Vector2Int(1, 1) };
            case ShapeType.L:
                return new[] { new Vector2Int(0, 0), new Vector2Int(0, 1),
                               new Vector2Int(0, 2), new Vector2Int(1, 0) };
        }
        return new Vector2Int[0];
    }
}

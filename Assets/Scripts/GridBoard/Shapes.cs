using UnityEngine;

public enum ShapeType { Single, Domino, Block, L, Line3, Line4, Block3, Block4 }

public static class Shapes
{
    public static Vector2Int[] Get(ShapeType t)
    {
        switch (t)
        {
            case ShapeType.Single: return Rect(1, 1);
            case ShapeType.Domino: return Rect(2, 1);   // 2x1
            case ShapeType.Block:  return Rect(2, 2);
            case ShapeType.Line3:  return Rect(3, 1);   // 3x1
            case ShapeType.Line4:  return Rect(4, 1);   // 4x1
            case ShapeType.Block3: return Rect(3, 3);
            case ShapeType.Block4: return Rect(4, 4);
            case ShapeType.L:
                return new[] { new Vector2Int(0, 0), new Vector2Int(0, 1),
                               new Vector2Int(0, 2), new Vector2Int(1, 0) };
        }
        return new Vector2Int[0];
    }

    static Vector2Int[] Rect(int w, int h)
    {
        var r = new Vector2Int[w * h];
        int i = 0;
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
                r[i++] = new Vector2Int(x, y);
        return r;
    }
}
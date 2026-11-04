using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class GeneratorTest
{
    [MenuItem("Tools/Test Generator")]
    static void Run()
    {
        // set bentuk mengikuti Level_03 (8x8, maxSingles 0)
        var setA = new List<ShapeWeight>
        {
            new ShapeWeight { type = ShapeType.Block,  weight = 1 },
            new ShapeWeight { type = ShapeType.L,      weight = 2 },
            new ShapeWeight { type = ShapeType.Domino, weight = 2 },
            new ShapeWeight { type = ShapeType.Single, weight = 1 },
            new ShapeWeight { type = ShapeType.Line3,  weight = 1 },
            new ShapeWeight { type = ShapeType.Block3, weight = 1 },
        };

        // set lebih berat: bentuk besar untuk level akhir
        var setB = new List<ShapeWeight>
        {
            new ShapeWeight { type = ShapeType.Block,  weight = 1 },
            new ShapeWeight { type = ShapeType.L,      weight = 2 },
            new ShapeWeight { type = ShapeType.Domino, weight = 2 },
            new ShapeWeight { type = ShapeType.Line3,  weight = 1 },
            new ShapeWeight { type = ShapeType.Line4,  weight = 1 },
            new ShapeWeight { type = ShapeType.Block3, weight = 1 },
            new ShapeWeight { type = ShapeType.Block4, weight = 1 },
        };

        Test("8x8   set A (pembanding)", 8, 8, setA, 0, 20);
        Test("12x12 set A", 12, 12, setA, 0, 20);
        Test("12x12 set B", 12, 12, setB, 0, 20);
        Test("17x17 set A", 17, 17, setA, 0, 10);
        Test("17x17 set B", 17, 17, setB, 0, 10);
    }

    static void Test(string name, int w, int h, List<ShapeWeight> shapes, int maxSingles, int runs)
    {
        int ok = 0;
        long total = 0, worst = 0;

        for (int i = 0; i < runs; i++)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            var result = LevelGenerator.Generate(w, h, shapes, maxSingles);
            sw.Stop();

            if (result != null) ok++;
            total += sw.ElapsedMilliseconds;
            if (sw.ElapsedMilliseconds > worst) worst = sw.ElapsedMilliseconds;
        }

        Debug.Log($"[GenTest] {name}: berhasil {ok}/{runs} | rata-rata {total / runs} ms | terburuk {worst} ms");
    }
}
using UnityEngine;

// Satu-satunya tempat yang membaca Input.
public static class PointerInput
{
    public static Vector2 Position => Input.mousePosition;

    public static bool SelectPressed => Input.GetMouseButtonDown(0);   // klik kiri: pilih
    public static bool DragPressed   => Input.GetMouseButtonDown(1);   // klik kanan: mulai drag
    public static bool DragReleased  => Input.GetMouseButtonUp(1);     // lepas klik kanan: jatuhkan
    public static bool PausePressed  => Input.GetKeyDown(KeyCode.Escape);

    // +1 = searah jarum jam, -1 = berlawanan, 0 = tidak ada
    public static int RotateStep
    {
        get
        {
            float s = Input.mouseScrollDelta.y;
            if (s > 0f) return 1;
            if (s < 0f) return -1;
            return 0;
        }
    }
}
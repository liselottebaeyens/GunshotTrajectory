using UnityEngine;

public static class BulletData
{
    // Data van 3D Slicer
    public static Vector3 entryPoint;
    public static Vector3 direction;

    // Handig om te checken of er al data binnen is
    public static bool hasData = false;

    // Optioneel: timestamp/debug
    public static float lastUpdateTime;

    public static void SetData(Vector3 entry, Vector3 dir)
    {
        entryPoint = entry;
        direction = dir.normalized;

        hasData = true;
    }

}
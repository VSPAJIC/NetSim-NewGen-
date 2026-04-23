using UnityEngine;

public static class PlayerReturnManager
{
    public static Vector3 position;
    public static Quaternion rotation;
    public static bool hasData = false;

    public static void Save(Transform player, Transform camera)
    {
        position = player.position;
        rotation = camera.rotation;
        hasData = true;
    }

    public static void Load(Transform player, Transform camera)
    {
        if (!hasData) return;

        player.position = position;
        camera.rotation = rotation;

        hasData = false;
    }
}
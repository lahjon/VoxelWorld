using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;

public static class Helpers
{
    public static IEnumerable<T> FindInterfacesOfType<T>(bool includeInactive = true)
    {
        var objs = SceneManager.GetActiveScene().GetRootGameObjects().SelectMany(go => go.GetComponentsInChildren<T>(includeInactive));

        return SceneManager.GetActiveScene().GetRootGameObjects().SelectMany(go => go.GetComponentsInChildren<T>(includeInactive));
    }
}

public static class ExtensionMethods 
{
    public static float Remap (this float value, float from1, float to1, float from2, float to2) 
    {
        // remap range1 to range2, 1, 0, 2, 0, 10 = 5
        return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
    }

    public static int Remap (this int value, int from1, int to1, int from2, int to2) 
    {
        // remap range1 to range2, 1, 0, 2, 0, 10 = 5
        return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
    }
}
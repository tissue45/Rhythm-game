using UnityEngine;
using UnityEditor;

public class SceneDumper : MonoBehaviour
{
    [MenuItem("Debug/Dump Hierarchy")]
    public static void Dump()
    {
        string output = "=== Scene Hierarchy ===\n";
        foreach (GameObject obj in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
        {
            output += DumpGameObject(obj, "");
        }
        Debug.Log(output);
    }

    private static string DumpGameObject(GameObject obj, string indent)
    {
        string info = $"{indent}- {obj.name} (Active: {obj.activeSelf})";
        
        // Component check
        var components = obj.GetComponents<Component>();
        info += " [";
        foreach(var c in components) {
            if (c == null) info += "MISSING_SCRIPT, ";
            else info += c.GetType().Name + ", ";
        }
        info += "]\n";

        foreach (Transform child in obj.transform)
        {
            info += DumpGameObject(child.gameObject, indent + "  ");
        }
        return info;
    }
}

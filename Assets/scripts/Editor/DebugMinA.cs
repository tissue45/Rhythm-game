using UnityEngine;
using UnityEditor;

public class DebugMinA : MonoBehaviour
{
    [MenuItem("Debug/Dump MinA Hierarchy")]
    public static void Dump()
    {
        GameObject obj = GameObject.Find("minA_1");
        if (obj == null) 
        {
            Debug.LogError("Could not find 'minA_1'");
            return;
        }

        Debug.Log($"=== Hierarchy of {obj.name} ===");
        DumpRecursive(obj.transform, "");
    }

    private static void DumpRecursive(Transform t, string indent)
    {
        string info = $"{indent}- {t.name}";
        var anim = t.GetComponent<Animator>();
        if (anim != null) info += " [HAS ANIMATOR]";
        
        Debug.Log(info);

        foreach(Transform child in t)
        {
            DumpRecursive(child, indent + "  ");
        }
    }
}

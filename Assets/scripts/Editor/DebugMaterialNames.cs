using UnityEngine;
using UnityEditor;

public class DebugMaterialNames : EditorWindow
{
    [MenuItem("Tools/Debug Material Names")]
    public static void DebugNames()
    {
        GameObject selectedObj = Selection.activeGameObject;
        if (selectedObj == null) return;

        Renderer[] renderers = selectedObj.GetComponentsInChildren<Renderer>(true);
        string log = "=== Material List ===\n";
        foreach (Renderer r in renderers)
        {
            foreach (Material m in r.sharedMaterials)
            {
                if (m != null)
                {
                    log += $"Material Name: '{m.name}'\n";
                }
                else
                {
                     log += "Material: NULL\n";
                }
            }
        }
        Debug.Log(log);
        EditorUtility.DisplayDialog("Material Names", log, "Copy to Clipboard?", "Close"); // Unity dialog doesn't support copy natively easily in one button but showing it helps
        EditorGUIUtility.systemCopyBuffer = log; // Copy to clipboard automatically
        EditorUtility.DisplayDialog("Copied!", "Material names have been copied to your clipboard.\nPlease paste them to the chat!", "OK");
    }
}

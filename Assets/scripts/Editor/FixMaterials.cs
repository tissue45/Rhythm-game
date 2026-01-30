using UnityEngine;
using UnityEditor;
using System.IO;

public class FixMaterials
{
    [MenuItem("Tools/Fix MMD Materials")]
    public static void Fix()
    {
        string folderPath = "Assets/Materials";
        if (!Directory.Exists(folderPath))
        {
            Debug.LogError("Assets/Materials folder not found!");
            return;
        }

        string[] guids = AssetDatabase.FindAssets("t:Material", new[] { "Assets/Materials" });
        int count = 0;

        Shader toonShader = Shader.Find("Custom/SimpleToon");
        if (toonShader == null)
        {
            Debug.LogError("Custom/SimpleToon shader not found! Did you compile the shader?");
            return;
        }

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            
            if (mat == null) continue;

            // Replace with our custom Toon shader if not already using it
            if (mat.shader.name != "Custom/SimpleToon")
            {
                Debug.Log($"Fixing material: {mat.name} (was {mat.shader.name})");
                mat.shader = toonShader;
                
                // Reset common properties to defaults
                mat.SetColor("_Color", Color.white);
                if (mat.HasProperty("_Cutoff")) mat.SetFloat("_Cutoff", 0.1f);
                // if (mat.HasProperty("_Outline")) mat.SetFloat("_Outline", 0.002f); // Outline removed
                if (mat.HasProperty("_RampThreshold")) mat.SetFloat("_RampThreshold", 0.5f);

                count++;
                EditorUtility.SetDirty(mat);
            }
        }
        
        if (count > 0)
        {
            AssetDatabase.SaveAssets();
            Debug.Log($"Fixed {count} materials to use Custom Toon shader.");
            EditorUtility.DisplayDialog("Material Fixer", $"Fixed {count} materials to use 'Custom Toon' shader.\nPlease rebuild your project now.", "OK");
        }
        else
        {
            Debug.Log("No MMD/Toon materials found to fix.");
            EditorUtility.DisplayDialog("Material Fixer", "No MMD materials found to fix. They might already be correct.", "OK");
        }
    }
}

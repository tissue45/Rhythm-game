using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

public class FixMMDMaterials : EditorWindow
{
    [MenuItem("Tools/Fix MMD Materials (Project Assets)")]
    static void FixProjectMaterials()
    {
        // [TARGET FOLDERS] Scan these specific folders for materials
        string[] targetFolders = { 
            "Assets/SodaPop", 
            "Assets/Model",
            "Assets/minA",
            "Assets/Minwoo",
            "Assets/Jiwoon", // Added explicit folder if exists
            "Assets/Yena"    // Added explicit folder if exists
        };

        int fixedCount = 0;
        int outlineHiddenCount = 0;
        
        Debug.Log("[FixMMDMaterials] Starting Project Scan...");

        // Find all Material assets in the project
        string[] guids = AssetDatabase.FindAssets("t:Material", targetFolders);
        
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);

            if (mat == null) continue;

            string matNameLower = mat.name.ToLower();

            // [CHECK 1] Is this an Outline material?
            // MMD4Mecanim usually names them "..._edge"
            bool isOutline = matNameLower.Contains("edge") || 
                             matNameLower.Contains("outline") ||
                             matNameLower.Contains("line");

            if (isOutline)
            {
                // [AGGRESSIVE FIX] Make completely invisible
                // Use a transparent shader and set everything to zero/clear
                mat.shader = Shader.Find("Legacy Shaders/Transparent/Diffuse"); 
                if (mat.shader == null) mat.shader = Shader.Find("Transparent/Diffuse");
                
                mat.color = new Color(0, 0, 0, 0); // 0 Alpha (Invisible)
                
                // Remove texture reference to be sure
                mat.mainTexture = null; 
                
                Debug.Log($"[Outline-Remover] Hidden (Transparent): {mat.name}");
                EditorUtility.SetDirty(mat); // Mark as changed
                outlineHiddenCount++;
                fixedCount++;
                continue;
            }

            // [CHECK 2] Fix Character Materials (Body/Face/Clothes)
            // [CHECK 2] Fix Character Materials (Body/Face/Clothes)
            // [CHECK 2] Fix Character Materials (Body/Face/Clothes)
            // [CHECK 2] Fix Character Materials (Body/Face/Clothes)
            // [CHECK 2] Fix Character Materials
            // Strategy: "WebGL-Compatible MMD Clone"
            // - Uses 'Custom/MMDWebGL' shader (Custom created)
            // - Restores Outlines & Toon Shading (Anime Look)
            // - Fixes Transparency/Sorting bugs (Cutout+ZWrite)
            
            Shader targetShader = Shader.Find("Custom/MMDWebGL");
            if (targetShader == null) targetShader = Shader.Find("Standard"); // Fallback

            if (mat.shader.name != targetShader.name || mat.shader.name.Contains("Unlit") || mat.shader.name.Contains("Standard"))
            {
                // Preserve Texture
                Texture mainTex = mat.mainTexture;
                if (mainTex == null && mat.HasProperty("_MainTex")) mainTex = mat.GetTexture("_MainTex");

                // Change Shader
                mat.shader = targetShader;
                
                // --- Setup Properties ---
                if (mainTex != null) mat.SetTexture("_MainTex", mainTex);
                mat.color = Color.white;
                if (mat.HasProperty("_Color")) mat.SetColor("_Color", Color.white);
                
                // --- MMD Look Settings ---
                // 1. Cutoff (Save Lashes)
                if (mat.HasProperty("_Cutoff")) mat.SetFloat("_Cutoff", 0.05f); 
                
                // 2. Toon Lighting (Ramp)
                // x=Threshold(0.5), y=Smooth(0.05), z=MinBrightness(0.5, Bright Anime)
                if (mat.HasProperty("_ShadowProps")) mat.SetVector("_ShadowProps", new Vector4(0.5f, 0.05f, 0.5f, 0));

                // 3. Outline Settings
                string name = mat.name.ToLower();
                float outlineWidth = 0.002f; // Default thin
                
                // Make Face outline thinner to avoid "Mustache" effect
                if (name.Contains("face") || name.Contains("skin") || name.Contains("eye")) 
                {
                    outlineWidth = 0.001f; 
                }
                else 
                {
                    outlineWidth = 0.0025f; // Body/Clothes slightly thicker
                }

                if (mat.HasProperty("_OutlineWidth")) mat.SetFloat("_OutlineWidth", outlineWidth);
                if (mat.HasProperty("_OutlineColor")) mat.SetColor("_OutlineColor", new Color(0.1f, 0.1f, 0.1f, 1f)); // Dark Grey (Softer)

                Debug.Log($"[MMD-Upgrade] {mat.name} -> MMDWebGL Shader (Outline: {outlineWidth})");
                EditorUtility.SetDirty(mat); 
                fixedCount++;
            }
        }
        
        // Save
        AssetDatabase.SaveAssets();
        
        EditorUtility.DisplayDialog("Result", 
            $"Applied 'MMD WebGL Upgrade' to {fixedCount} materials!\n\n- Shader: Custom/MMDWebGL\n- Outlines: Restored (Auto-Size)\n- Lashes: Visible", 
            "OK");
    }
}

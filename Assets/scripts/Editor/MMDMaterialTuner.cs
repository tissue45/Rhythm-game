using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class MMDMaterialTuner : EditorWindow
{
    // [UPDATED] Default Values from User's Golden Ratio
    float outlineWidth = 0.0012f;
    Color outlineColor = Color.black; 
    
    // Lashes
    float cutoff = 0.01f;
    Color eyelashColor = Color.black; // Fix for white lashes
    bool forceLashColor = false;      // NEW: Power to overwrite texture color
    
    // Toon Lighting
    float shadowThreshold = 1.0f;   
    float shadowSmooth = 0.01f;     
    float shadowBrightness = 0.29f;  
    float emissionIntensity = 0.3f; 

    bool autoThinFaceOutline = true; 

    // [NEW] Ignore List for Effects/Orbs
    string[] ignoreKeywords = new string[] { 
        "sphere", "ball", "ring", "effect", "particle", "beam", "glow", "ui", "skybox", "glass", "bubble" 
    };

    [MenuItem("Tools/MMD Material Tuner")]
    public static void ShowWindow()
    {
        GetWindow<MMDMaterialTuner>("MMD Tuner");
    }

    void OnGUI()
    {
        GUILayout.Label("MMD Character Fine-Tuning", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        // 1. Outline
        GUILayout.Label("1. Outline (Line) Settings", EditorStyles.boldLabel);
        outlineWidth = EditorGUILayout.Slider("Line Width", outlineWidth, 0.000f, 0.02f);
        outlineColor = EditorGUILayout.ColorField("Line Color", outlineColor);
        autoThinFaceOutline = EditorGUILayout.Toggle("Thinner Face Lines", autoThinFaceOutline);
        GUILayout.Label("   (Checks 'Face'/'Eye' materials and halves width)", EditorStyles.miniLabel);

        EditorGUILayout.Space();

        // 2. Face / Lashes
        GUILayout.Label("2. Lashes & Details", EditorStyles.boldLabel);
        cutoff = EditorGUILayout.Slider("Alpha Cutoff", cutoff, 0.001f, 0.9f);
        eyelashColor = EditorGUILayout.ColorField("Lash Color", eyelashColor);
        forceLashColor = EditorGUILayout.Toggle("Force Lash Color", forceLashColor);
        GUILayout.Label("   * Check 'Force' to override black textures!", EditorStyles.miniLabel);

        EditorGUILayout.Space();

        // 3. Lighting
        GUILayout.Label("3. Toon Lighting & Brightness", EditorStyles.boldLabel);
        shadowThreshold = EditorGUILayout.Slider("Shadow Area", shadowThreshold, 0.0f, 1.0f);
        shadowSmooth = EditorGUILayout.Slider("Shadow Softness", shadowSmooth, 0.01f, 0.5f);
        shadowBrightness = EditorGUILayout.Slider("Shadow Brightness", shadowBrightness, 0.0f, 1.0f);
        emissionIntensity = EditorGUILayout.Slider("Self-Glow (Emission)", emissionIntensity, 0.0f, 1.0f);
        GUILayout.Label("   * Reduce Glow if face is too white!", EditorStyles.miniLabel);

        EditorGUILayout.Space();
        EditorGUILayout.Space();

        EditorGUILayout.HelpBox("Clicking Apply will Fix Characters AND Restore Effects (Orbs)!", MessageType.Info);

        if (GUILayout.Button("APPLY SETTINGS (Update All)", GUILayout.Height(30)))
        {
            ApplySettings();
        }

        EditorGUILayout.Space();
        
        GUI.backgroundColor = new Color(1f, 0.5f, 0.5f); // Reddish tint
        if (GUILayout.Button("FIX WHITE ORBS ONLY (Leave Characters Alone)", GUILayout.Height(40)))
        {
            RestoreEffectsOnly();
        }
        GUI.backgroundColor = Color.white;
    }

    void RestoreEffectsOnly()
    {
        string[] targetFolders = new string[] { 
            "Assets/SodaPop", "Assets/Model", "Assets/minA", 
            "Assets/Minwoo", "Assets/Jiwoon", "Assets/Yena" 
        };

        var existingFolders = new List<string>();
        foreach (var f in targetFolders) if (AssetDatabase.IsValidFolder(f)) existingFolders.Add(f);

        string[] guids = AssetDatabase.FindAssets("t:Material", existingFolders.ToArray());
        int restoredCount = 0;
        Shader standardRating = Shader.Find("Standard");

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null) continue;

            string name = mat.name.ToLower();
            bool isEffect = false;
            foreach (var kw in ignoreKeywords) {
                if (name.Contains(kw)) { isEffect = true; break; }
            }

            // 1. Force Bubbles/Glass to Transparent Standard (regardless of current shader)
            if (name.Contains("glass") || name.Contains("bubble"))
            {
                mat.shader = standardRating;
                mat.SetFloat("_Mode", 3); // 3 = Transparent
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                mat.SetInt("_ZWrite", 0);
                mat.DisableKeyword("_ALPHATEST_ON");
                mat.DisableKeyword("_ALPHABLEND_ON");
                mat.EnableKeyword("_ALPHAPREMULTIPLY_ON");
                mat.renderQueue = 3000;
                
                // CRITICAL FIX FOR BLACK BUBBLES:
                // High Metallic + No Reflection Probe = Pure Black.
                // Force Metallic to 0 to ensure it looks like white semi-transparent glass.
                mat.SetFloat("_Metallic", 0.0f); 
                mat.SetFloat("_Glossiness", 0.8f); // Shiny but not mirror
                
                // Ensure visible alpha
                Color col = Color.white;
                col.a = 0.3f; // Nice transparency
                mat.color = col;
                
                mat.DisableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", Color.black);
                
                EditorUtility.SetDirty(mat);
                restoredCount++;
                Debug.Log($"[-] Forced Clean Bubble (Metallic 0): {mat.name}");
                continue; // Done with this material
            }

            // 2. Revert other erroneous MMDWebGL effects to Opaque Standard
            if (isEffect && mat.shader.name.Contains("MMDWebGL"))
            {
                mat.shader = standardRating;
                mat.SetFloat("_Mode", 0); // 0 = Opaque
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                mat.SetInt("_ZWrite", 1);
                mat.DisableKeyword("_ALPHATEST_ON");
                mat.DisableKeyword("_ALPHABLEND_ON");
                mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                mat.renderQueue = -1;
                
                mat.DisableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", Color.black);
                mat.color = Color.white;
                
                EditorUtility.SetDirty(mat);
                restoredCount++;
                Debug.Log($"[-] Reverted Opaque Effect: {mat.name}");
            }
        }
        AssetDatabase.SaveAssets();
        EditorUtility.DisplayDialog("Done", $"Fixed {restoredCount} items.\n\nBubbles -> Transparent (Metallic 0)\nOther Effects -> Opaque\nCharacters -> IGNORED", "OK");
    }

    void ApplySettings()
    {
        string[] targetFolders = new string[] { 
            "Assets/SodaPop", 
            "Assets/Model", 
            "Assets/minA", 
            "Assets/Minwoo", 
            "Assets/Jiwoon", 
            "Assets/Yena" 
        };

        // Validate folders
        var existingFolders = new List<string>();
        foreach (var f in targetFolders)
        {
            if (AssetDatabase.IsValidFolder(f)) existingFolders.Add(f);
        }

        if (existingFolders.Count == 0)
        {
            EditorUtility.DisplayDialog("Error", "No MMD folders found!", "OK");
            return;
        }

        string[] guids = AssetDatabase.FindAssets("t:Material", existingFolders.ToArray());
        int count = 0;
        int restoredCount = 0;

        Shader mmdShader = Shader.Find("Custom/MMDWebGL");
        if (mmdShader == null) mmdShader = Shader.Find("Standard");

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);

            if (mat == null) continue;

            string name = mat.name.ToLower();

            // --- [FIX] Effect/Orb Restoration Logic ---
            bool isEffect = false;
            foreach (var kw in ignoreKeywords)
            {
                if (name.Contains(kw)) 
                {
                    isEffect = true;
                    break;
                }
            }

            if (isEffect)
            {
                // If it was mistakenly converted to MMDWebGL, revert it!
                if (mat.shader.name == mmdShader.name)
                {
                    mat.shader = Shader.Find("Standard"); // Revert to Standard
                    mat.SetColor("_EmissionColor", Color.black); // Turn off the white glow
                    mat.DisableKeyword("_EMISSION");
                    mat.color = Color.white; // Reset base color
                    
                    EditorUtility.SetDirty(mat);
                    restoredCount++;
                    Debug.Log($"[MMD-Restore] Reverted Effect/Orb: {mat.name}");
                }
                continue; // Skip character logic for this material
            }

            // --- Character Logic Below ---

            // Assign Shader
            if (mat.shader.name != mmdShader.name) mat.shader = mmdShader;

            // 1. Cutoff
            if (mat.HasProperty("_Cutoff")) mat.SetFloat("_Cutoff", cutoff);

            // 2. Shadow/Toon Props
            if (mat.HasProperty("_ShadowProps")) 
                mat.SetVector("_ShadowProps", new Vector4(shadowThreshold, shadowSmooth, shadowBrightness, 0));

            // 3. Emission (Global)
            if (mat.HasProperty("_EmissionColor")) 
                mat.SetColor("_EmissionColor", new Color(emissionIntensity, emissionIntensity, emissionIntensity));
            mat.EnableKeyword("_EMISSION");

            // 4. Outline & RESCUE LOGIC (Lashes)
            float finalWidth = outlineWidth;
            
            // [CRITICAL FIX] Rescue "Eyeline" (Lashes)
            if (name.Contains("eyeline") || name.Contains("eye_line") || name.Contains("lash"))
            {
                // Use USER Color
                mat.SetColor("_Color", eyelashColor);
                
                // IF FORCED: Apply to Emission too (Overwrites black texture)
                if (forceLashColor)
                {
                    mat.SetColor("_EmissionColor", eyelashColor);
                }

                mat.renderQueue = 2450; 
                finalWidth = 0.0005f; 
                
                EditorUtility.SetDirty(mat);
            }
            else
            {
                // Normal Body Parts -> White Base
                mat.SetColor("_Color", Color.white);
            }

            // Auto-Thin Face Logic
            if (autoThinFaceOutline && (name.Contains("face") || name.Contains("skin") || name.Contains("eye")))
            {
                finalWidth = outlineWidth * 0.5f;
                if (finalWidth < 0.0005f) finalWidth = 0.0005f; // Min limit
            }

            if (mat.HasProperty("_OutlineWidth")) mat.SetFloat("_OutlineWidth", finalWidth);
            if (mat.HasProperty("_OutlineColor")) mat.SetColor("_OutlineColor", outlineColor);

            EditorUtility.SetDirty(mat);
            count++;
        }

        AssetDatabase.SaveAssets();
        EditorUtility.DisplayDialog("Success", 
            $"Applied to {count} character materials.\n\n[RESTORED] {restoredCount} Effect/Orb materials reverted to Standard!\n\nIf orbs are still white, check if they have 'sphere' or 'ball' in their name.", 
            "OK");
    }
}

using UnityEngine;
using UnityEditor;
using TMPro;

/// <summary>
/// Automatically detects and fixes TMP_FontAsset missing material issues.
/// Runs on load and provides a menu item.
/// </summary>
[InitializeOnLoad]
public class FixTMPIssue
{
    static FixTMPIssue()
    {
        // Run once after compilation/load
        EditorApplication.delayCall += CheckAndFixFonts;
    }

    [MenuItem("Rhythm Game/Fix TMP Fonts")]
    public static void CheckAndFixFonts()
    {
        // Find all font assets including those in packages
        var fonts = Resources.FindObjectsOfTypeAll<TMP_FontAsset>();
        bool fixedAny = false;

        foreach (var font in fonts)
        {
            if (font == null) continue;

            // Only care about persistent assets (files), not instances in memory
            if (!EditorUtility.IsPersistent(font)) continue;

            if (font.material == null)
            {
                Debug.LogWarning($"[FixTMPIssue] Font '{font.name}' has unassigned material. Attempting to fix...");

                // Strategy 1: Find valid material within the same asset (Sub-asset)
                var assets = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(font));
                foreach (var obj in assets)
                {
                    if (obj is Material mat)
                    {
                        if (mat.name == font.name || mat.name.Contains("Atlas"))
                        {
                            font.material = mat;
                            Debug.Log($"[FixTMPIssue] Found sub-asset material '{mat.name}' for font '{font.name}'. Assigned.");
                            EditorUtility.SetDirty(font);
                            fixedAny = true;
                            break;
                        }
                    }
                }

                // Strategy 2: Find external material with same name
                if (font.material == null)
                {
                    string[] guids = AssetDatabase.FindAssets(font.name + " t:Material");
                    foreach (var guid in guids)
                    {
                        string path = AssetDatabase.GUIDToAssetPath(guid);
                        Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
                        // Avoid Outline/Shadow variants for the main material
                        if (mat != null && !mat.name.Contains("Outline") && !mat.name.Contains("Shadow") && !mat.name.Contains("Fallback"))
                        {
                            font.material = mat;
                            Debug.Log($"[FixTMPIssue] Found external material '{mat.name}' at {path}. Assigned.");
                            EditorUtility.SetDirty(font);
                            fixedAny = true;
                            break;
                        }
                    }
                }
                
                // Strategy 3: Setup Default Fallback if it's LiberationSans
                if (font.material == null && font.name.Contains("Liberation"))
                {
                    Shader s = Shader.Find("TextMeshPro/Distance Field");
                    if (s != null)
                    {
                         // Create a temporary material if needed, but best not to modify asset structure too aggressively without user input.
                         // Instead, try to locate the standard TMP package material
                         string[] guids = AssetDatabase.FindAssets("LiberationSans SDF t:Material");
                         if (guids.Length > 0)
                         {
                             // Pick first reasonable one
                             var m = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(guids[0]));
                             font.material = m;
                             EditorUtility.SetDirty(font);
                             fixedAny = true;
                             Debug.Log($"[FixTMPIssue] Forced assignment of found material '{m.name}' to '{font.name}'");
                         }
                    }
                }
            }
        }

        if (fixedAny)
        {
            AssetDatabase.SaveAssets();
            Debug.Log("<color=green>[FixTMPIssue] Fixed broken TMP_FontAssets! Please restart the scene or enter Play Mode.</color>");
        }
    }
}

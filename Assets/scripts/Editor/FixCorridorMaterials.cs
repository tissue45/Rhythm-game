using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

public class FixCorridorMaterials : EditorWindow
{
    [MenuItem("Tools/Fix White Corridor Materials")]
    public static void FixMaterials()
    {
        GameObject selectedObj = Selection.activeGameObject;
        if (selectedObj == null)
        {
            EditorUtility.DisplayDialog("Error", "Please select the 'Japanese school corridor' object in the Scene or Hierarchy first!", "OK");
            return;
        }

        Undo.RegisterCompleteObjectUndo(selectedObj, "Fix Materials");

        // 1. 모델 내의 모든 Renderer 찾기
        Renderer[] renderers = selectedObj.GetComponentsInChildren<Renderer>(true);
        
        // 2. 텍스처 찾기 (프로젝트 전체 검색)
        string[] textureGUIDs = AssetDatabase.FindAssets("t:Texture2D");
        Dictionary<string, string> texturePathMap = new Dictionary<string, string>();

        foreach (var guid in textureGUIDs)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            string fileName = Path.GetFileNameWithoutExtension(path).ToLower();
            if (!texturePathMap.ContainsKey(fileName))
            {
                texturePathMap[fileName] = path;
            }
        }

        int fixedCount = 0;

        foreach (Renderer r in renderers)
        {
            Material[] sharedMaterials = r.sharedMaterials;
            Material[] newMaterials = new Material[sharedMaterials.Length];

            for (int i = 0; i < sharedMaterials.Length; i++)
            {
                Material originalMat = sharedMaterials[i];
                string originalName = originalMat != null ? originalMat.name : "Material";
                
                // 이름 정리
                originalName = originalName.Replace("(Instance)", "").Trim();
                string searchName = originalName.ToLower();

                Debug.Log($"[FixCorridor] Processing Material: '{originalName}' (Search: '{searchName}')");

                // 여러가지 이름 패턴 시도
                string[] trySuffixes = new string[] { "_base_color", "_albedo", "_diffuse", "", "_basecolor" };
                string foundKey = null;

                foreach (var suffix in trySuffixes)
                {
                    string key = searchName + suffix;
                    if (texturePathMap.ContainsKey(key))
                    {
                        foundKey = key;
                        break;
                    }
                }

                if (foundKey == null)
                {
                    // [Fallback] 머테리얼 이름이 이상하면 '게임오브젝트 이름'으로 시도!
                    string objName = r.gameObject.name.ToLower();
                    Debug.Log($"[FixCorridor] Fallback Check: Object Name '{objName}'");
                    
                    foreach (var suffix in trySuffixes)
                    {
                        string key = objName + suffix;
                         // 부분 일치(Contains)도 시도
                        foreach(var kvp in texturePathMap)
                        {
                            if (kvp.Key.Contains(objName) && kvp.Key.Contains("base_color"))
                            {
                                foundKey = kvp.Key;
                                break;
                            }
                        }
                        if (foundKey != null) break;
                    }
                }

                if (foundKey != null)
                {
                    // 텍스처 찾음!
                    string texPath = texturePathMap[foundKey];
                    Debug.Log($"[FixCorridor] Found texture for '{originalName}': {texPath}");
                    
                    Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(texPath);
                    string matPath = Path.GetDirectoryName(texPath) + "/" + originalName + "_Fixed_" + System.DateTime.Now.Ticks + ".mat"; // 중복 방지
                    
                    Material newMat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
                    if (newMat == null)
                    {
                        newMat = new Material(Shader.Find("Universal Render Pipeline/Lit")); 
                        if (newMat.shader.name == "Hidden/InternalErrorShader") 
                            newMat = new Material(Shader.Find("Standard"));
                        
                        newMat.mainTexture = tex; 
                        if (newMat.HasProperty("_BaseMap")) newMat.SetTexture("_BaseMap", tex);
                        
                        AssetDatabase.CreateAsset(newMat, matPath);
                    }
                    newMaterials[i] = newMat;
                    fixedCount++;
                }
                else
                {
                    Debug.LogWarning($"[FixCorridor] Could not find texture for Material '{originalName}' OR Object '{r.gameObject.name}'");
                    newMaterials[i] = originalMat;
                }
            }
            r.sharedMaterials = newMaterials;
        }

        if (fixedCount > 0)
        {
            EditorUtility.DisplayDialog("Success", $"Fixed {fixedCount} material slots! \nCheck the model now.", "Cool");
        }
        else
        {
            EditorUtility.DisplayDialog("Failed", "Could not find matching textures. \nCheck 'Console' tab for details on what materials were searched.", "OK");
        }
    }
}

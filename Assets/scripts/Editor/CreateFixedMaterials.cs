using UnityEngine;
using UnityEditor;
using System.IO;

public class CreateFixedMaterials : EditorWindow
{
    [MenuItem("Tools/Create Fixed Materials")]
    public static void CreateMaterials()
    {
        // 1. 텍스처 찾기 (주요 텍스처만)
        string[] targetNames = new string[] { "Floor", "Wall", "Roof", "Windows", "Ladder" };
        int createdCount = 0;
        string firstMatPath = "";

        foreach (string name in targetNames)
        {
            // Floor_Base_Color.png 같은거 찾기
            string[] guids = AssetDatabase.FindAssets(name + "_Base_Color t:Texture2D");
            if (guids.Length == 0) guids = AssetDatabase.FindAssets(name + " t:Texture2D"); // 이름이 그냥 Floor일수도

            if (guids.Length > 0)
            {
                string texPath = AssetDatabase.GUIDToAssetPath(guids[0]);
                Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(texPath);
                
                // 머테리얼 생성 경로 (Assets/Materials/Fixed_...)
                string folder = "Assets/Materials";
                if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
                
                string matPath = $"{folder}/{name}_Fixed.mat";
                
                Material newMat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
                if (newMat == null)
                {
                    newMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                    if (newMat.shader.name == "Hidden/InternalErrorShader") newMat = new Material(Shader.Find("Standard"));
                    
                    AssetDatabase.CreateAsset(newMat, matPath);
                }

                // 텍스처 연결
                newMat.mainTexture = tex;
                if (newMat.HasProperty("_BaseMap")) newMat.SetTexture("_BaseMap", tex);
                
                createdCount++;
                if (string.IsNullOrEmpty(firstMatPath)) firstMatPath = matPath;
                
                Debug.Log($"Created Material: {matPath}");
            }
        }

        if (createdCount > 0)
        {
            // 해당 폴더 열기 (Project 창에서)
            Object obj = AssetDatabase.LoadAssetAtPath<Object>(firstMatPath);
            EditorGUIUtility.PingObject(obj);
            EditorUtility.DisplayDialog("Success", $"Created {createdCount} materials in 'Assets/Materials' folder!\n\nNow simple DRAG & DROP them onto the school objects.", "OK");
        }
        else
        {
            EditorUtility.DisplayDialog("Failed", "Could not find standard textures (Floor/Wall/Roof).", "OK");
        }
    }
}

using UnityEngine;
using UnityEditor;

public class InspectMaterial
{
    [MenuItem("Tools/Inspect First Material")]
    public static void Inspect()
    {
        string[] guids = AssetDatabase.FindAssets("t:Material", new[] { "Assets/Materials" });
        if (guids.Length == 0)
        {
            Debug.Log("No materials found.");
            return;
        }

        string path = AssetDatabase.GUIDToAssetPath(guids[0]);
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        
        Debug.Log($"Inspecting Material: {mat.name}, Shader: {mat.shader.name}");
        
        Shader shader = mat.shader;
        int count = ShaderUtil.GetPropertyCount(shader);
        
        for (int i = 0; i < count; i++)
        {
            string propName = ShaderUtil.GetPropertyName(shader, i);
            ShaderUtil.ShaderPropertyType type = ShaderUtil.GetPropertyType(shader, i);
            Debug.Log($"Property: {propName} ({type})");
            
            if (type == ShaderUtil.ShaderPropertyType.TexEnv)
            {
                Texture tex = mat.GetTexture(propName);
                if (tex != null) Debug.Log($"   -> Assigned Texture: {tex.name}");
            }
        }
    }
}

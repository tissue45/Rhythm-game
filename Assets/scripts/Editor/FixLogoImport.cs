using UnityEngine;
using UnityEditor;

public class FixLogoImport : EditorWindow
{
    [MenuItem("Tools/Fix Logo Import")]
    public static void FixImport()
    {
        string path = "Assets/Resources/TitleLogo.png";
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        
        if (importer != null)
        {
            if (importer.textureType != TextureImporterType.Sprite)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.SaveAndReimport();
                Debug.Log("Fixed TitleLogo import settings to Sprite!");
                EditorUtility.DisplayDialog("Success", "Logo fixed! Now try 'Build Lobby UI' again.", "OK");
            }
            else
            {
                EditorUtility.DisplayDialog("Already Fixed", "Logo is already a Sprite. Try 'Build Lobby UI' again.", "OK");
            }
        }
        else
        {
            EditorUtility.DisplayDialog("Error", "Could not find 'Assets/Resources/TitleLogo.png'", "OK");
        }
    }
}

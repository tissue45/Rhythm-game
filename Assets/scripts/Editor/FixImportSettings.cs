using UnityEngine;
using UnityEditor;

public class FixImportSettings : EditorWindow
{
    [MenuItem("Tools/Fix Model Import Settings")]
    public static void FixSettings()
    {
        if (Selection.activeObject == null) return;
        
        string path = AssetDatabase.GetAssetPath(Selection.activeObject);
        ModelImporter importer = AssetImporter.GetAtPath(path) as ModelImporter;
        
        if (importer != null)
        {
            // 1. 머테리얼 이름 규칙을 'From Model's Material'로 변경
            importer.materialName = ModelImporterMaterialName.BasedOnMaterialName;
            
            // 2. 머테리얼 검색 위치를 'Local Materials Folder'로 변경
            importer.materialLocation = ModelImporterMaterialLocation.External;
            
            importer.SaveAndReimport();
            
            Debug.Log($"Fixed import settings for: {path}");
            EditorUtility.DisplayDialog("Done", "Import settings updated! \nNow try 'Debug Material Names' again to see if names changed.", "OK");
        }
        else
        {
            EditorUtility.DisplayDialog("Error", "Please select the FBX file in the Project window (not Scene)!", "OK");
        }
    }
}

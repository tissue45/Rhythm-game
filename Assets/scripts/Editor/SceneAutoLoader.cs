using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

[InitializeOnLoad]
public class SceneAutoLoader
{
    static SceneAutoLoader()
    {
        EnsureScenesInBuild();
    }

    [MenuItem("Tools/Fix Build Scenes")]
    public static void EnsureScenesInBuild()
    {
        // 필수 씬 경로 목록 
        // (필요에 따라 soda pop.unity 등 다른 씬도 추가 가능)
        string[] requiredScenes = new string[]
        {
            "Assets/Scenes/Main.unity",
            "Assets/Scenes/Lobby.unity",
            "Assets/Scenes/Game_first.unity"
        };

        List<EditorBuildSettingsScene> currentScenes = EditorBuildSettings.scenes.ToList();
        bool changed = false;

        foreach (string scenePath in requiredScenes)
        {
            // 현재 빌드 세팅에 해당 씬이 있는지 확인 (경로 기준)
            if (!currentScenes.Any(s => s.path == scenePath))
            {
                // 없으면 추가 (enabled = true)
                currentScenes.Add(new EditorBuildSettingsScene(scenePath, true));
                Debug.Log($"[SceneAutoLoader] 빌드 세팅에 씬 추가됨: {scenePath}");
                changed = true;
            }
        }

        if (changed)
        {
            EditorBuildSettings.scenes = currentScenes.ToArray();
            Debug.Log("[SceneAutoLoader] Build Settings가 성공적으로 업데이트되었습니다.");
        }
    }
}

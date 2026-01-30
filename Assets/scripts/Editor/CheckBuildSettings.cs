using UnityEngine;
using UnityEditor;
using System.Linq;

public class CheckBuildSettings
{
    [MenuItem("Tools/Fix Build Settings")]
    public static void FixScenes()
    {
        var scenes = EditorBuildSettings.scenes.ToList();
        Debug.Log("Current Scenes in Build:");
        foreach(var s in scenes) Debug.Log($"{s.path} (Enabled: {s.enabled})");

        string lobbyPath = "Assets/Scenes/Lobby.unity";

        // Check if Lobby is first
        if (scenes.Count > 0 && scenes[0].path != lobbyPath)
        {
            Debug.Log("Lobby is not the first scene! Fixing...");
            
            var lobbyScene = scenes.FirstOrDefault(s => s.path.Contains("Lobby.unity"));
            var gameScene = scenes.FirstOrDefault(s => s.path.Contains("Game.unity"));

            if (lobbyScene != null && gameScene != null)
            {
                var newScenes = new EditorBuildSettingsScene[] 
                {
                    lobbyScene,
                    gameScene
                };
                EditorBuildSettings.scenes = newScenes;
                Debug.Log("Build Settings Fixed: Lobby is now index 0.");
            }
            else
            {
                Debug.LogError("Could not find Lobby or Game scene to fix order.");
            }
        }
        else
        {
            Debug.Log("Scene order seems correct (Lobby is first).");
        }
    }
}

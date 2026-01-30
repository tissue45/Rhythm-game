using UnityEngine;
using UnityEditor;

public class ForceCleanupButtons : EditorWindow
{
    [MenuItem("Tools/Force Cleanup Ghost Buttons")]
    public static void Cleanup()
    {
        int count = 0;
        
        // Cleanup QuitButton
        var quitBtns = GameObject.FindObjectsOfType<GameObject>(); // Get everything to be sure
        foreach(var obj in quitBtns) 
        {
            if (obj.name == "QuitButton" && obj.scene.isLoaded)
            {
                Undo.DestroyObjectImmediate(obj);
                count++;
            }
        }

        // Cleanup StartButton (be careful not to delete the new one if named same, though new one is likely Btn_Start)
        // Check if it has RhythmMenuButton. If not, delete it.
        var startBtns = GameObject.FindObjectsOfType<GameObject>();
        foreach(var obj in startBtns)
        {
            if (obj.name == "StartButton" && obj.scene.isLoaded)
            {
                if (obj.GetComponent<RhythmMenuButton>() == null)
                {
                    Undo.DestroyObjectImmediate(obj);
                    count++;
                }
            }
        }

        Debug.Log($"[Cleanup] Removed {count} ghost buttons.");
    }
}

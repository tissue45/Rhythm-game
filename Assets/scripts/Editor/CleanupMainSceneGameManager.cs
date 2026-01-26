using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;

public class CleanupMainSceneGameManager : MonoBehaviour
{
    [MenuItem("Debug/Remove GameManager from Main")]
    public static void Cleanup()
    {
        if (SceneManager.GetActiveScene().name != "Main")
        {
            Debug.LogWarning("Please run this in the 'Main' scene.");
            return;
        }

        GameManager gm = Object.FindFirstObjectByType<GameManager>();
        if (gm != null)
        {
            Undo.DestroyObjectImmediate(gm.gameObject);
            Debug.Log("[Cleanup] Removed GameManager from Main scene. It belongs in 'Game' scene only.");
        }
        else
        {
            Debug.Log("[Cleanup] No GameManager found in Main scene. Good!");
        }
    }
}

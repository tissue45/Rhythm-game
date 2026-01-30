using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

public class RescueScene : MonoBehaviour
{
    [MenuItem("Tools/Rescue UI")]
    public static void Rescue()
    {
        Debug.Log("Trying to rescue UI...");

        // 1. Ensure Canvas exists and is active
        Canvas[] canvases = Resources.FindObjectsOfTypeAll<Canvas>();
        if (canvases.Length == 0)
        {
            Debug.LogError("No Canvas found in the scene/resources!");
        }
        else
        {
            foreach (var c in canvases)
            {
                if (c.gameObject.scene.name == null) continue; // Skip prefabs
                Debug.Log($"Found Canvas: {c.name} (Active: {c.gameObject.activeSelf})");
                c.gameObject.SetActive(true);
            }
        }

        // 2. Find Panels
        string[] panelNames = { "SongSelectPanel", "SongSelectPanel2", "StepUpShopPanel", "PremiumShop", "RankingPanel", "Canvas" };
        foreach (string name in panelNames)
        {
            GameObject obj = GameObject.Find(name);
            if (obj == null)
            {
                 // Try finding inactive
                 foreach (var c in canvases)
                 {
                     if (c.gameObject.scene.name == null) continue;
                     Transform t = c.transform.Find(name);
                     if (t != null) 
                     {
                         obj = t.gameObject;
                         break;
                     }
                 }
            }

            if (obj != null)
            {
                Debug.Log($"Found {name}, activating...");
                obj.SetActive(true);
            }
            else
            {
                Debug.LogWarning($"Could not find {name}");
            }
        }

        // 3. Fix MinA (Optional - just logging)
        GameObject mina = GameObject.Find("minA_1");
        if (mina != null)
        {
            Animator anim = mina.GetComponent<Animator>();
            if (anim != null && anim.runtimeAnimatorController == null)
            {
                Debug.LogError("MinA has no Animator Controller!");
            }
        }
    }
}

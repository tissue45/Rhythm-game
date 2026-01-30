using UnityEngine;
using UnityEditor;

public class ResetLobbyUI : MonoBehaviour
{
    [MenuItem("Tools/Reset Lobby UI")]
    public static void ResetUI()
    {
        Debug.Log("Resetting Lobby UI to clean state...");

        // 1. Hide Overlay Panels
        string[] panelsToHide = { 
            "SongSelectPanel", 
            "SongSelectPanel2", 
            "RankingPanel", 
            "StepUpShopPanel", 
            "PremiumShop", 
            "OptionsPanel", 
            "ProfilePanel",
            "CustomizationPanel",
            "QRConnectionPanel"
        };

        foreach (string name in panelsToHide)
        {
            GameObject obj = GameObject.Find(name);
            if (obj != null)
            {
                Undo.RecordObject(obj, "Hide Panel"); // Support Undo
                obj.SetActive(false);
                Debug.Log($"Hidden: {name}");
            }
        }

        // 2. Ensure Main Canvas and Background are visible
        GameObject canvas = GameObject.Find("Canvas");
        if (canvas != null) canvas.SetActive(true);

        Debug.Log("Lobby UI Reset Complete! Only main buttons should be visible now.");
    }
}

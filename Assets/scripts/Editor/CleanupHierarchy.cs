using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class CleanupHierarchy : MonoBehaviour
{
    [MenuItem("Debug/Cleanup Duplicates")]
    public static void Cleanup()
    {
        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.LogAssertion("No Canvas found!");
            return;
        }

        var children = new List<Transform>();
        foreach (Transform child in canvas.transform) children.Add(child);

        var seenNames = new HashSet<string>();
        int deletedCount = 0;

        // Keep the FIRST active one, or just the first one if all inactive?
        // Let's keep the first one we encounter.
        // Actually, SchoolLobbyManager might reference one specific instance.
        // But usually Find("Name") finds the top one.
        
        foreach (Transform child in children)
        {
            // Objects to explicitly remove
            if (child.name == "ConnectionPanel" || child.name == "QuitButton" || child.name == "StartButton")
            {
                Undo.DestroyObjectImmediate(child.gameObject);
                deletedCount++;
                continue;
            }

            // Remove duplicates for known panel names
            if (IsPanelName(child.name))
            {
                if (seenNames.Contains(child.name))
                {
                    Undo.DestroyObjectImmediate(child.gameObject);
                    deletedCount++;
                }
                else
                {
                    seenNames.Add(child.name);
                }
            }
        }

        Debug.Log($"Cleaned up {deletedCount} duplicate/unwanted objects.");
    }

    private static bool IsPanelName(string name)
    {
        return name.Contains("Panel") || name == "BackgroundPanel";
    }
}

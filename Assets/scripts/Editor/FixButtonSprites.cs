using UnityEngine;
using UnityEditor;

// Infinite loop fix: Removed AssetPostprocessor
public class FixButtonSprites : MonoBehaviour
{
    [MenuItem("Tools/Fix Button Sprites")]
    public static void FixSprites()
    {
        Debug.Log("Button Sprite Fixer is disabled to prevent loops.");
    }
}

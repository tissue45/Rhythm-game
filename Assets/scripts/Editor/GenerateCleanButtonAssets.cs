using UnityEngine;
using UnityEditor;
using System.IO;

public class GenerateCleanButtonAssets
{
    [MenuItem("Tools/Generate Clean Button Assets")]
    [InitializeOnLoadMethod] // Auto-run on compile
    public static void Generate()
    {
        GenerateParallelogramBG();
        GenerateParallelogramStrip();
        
        AssetDatabase.Refresh();
        
        // Trigger the fix import script
        if (EditorApplication.ExecuteMenuItem("Tools/Fix Button Sprites"))
        {
            Debug.Log("Triggered Fix Button Sprites");
        }
    }

    static void GenerateParallelogramBG()
    {
        int width = 512;
        int height = 128;
        Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
        
        // Fill with clear
        Color[] clear = new Color[width * height];
        for (int i = 0; i < clear.Length; i++) clear[i] = Color.clear;
        tex.SetPixels(clear);

        // Draw Parallelogram
        // Skew factor: let's say slope is ~0.3
        float slope = 0.3f; 
        
        for (int y = 0; y < height; y++)
        {
            float shift = (height - y) * slope; 
            int startX = (int)shift + 20; // 20px padding
            int endX = width - (int)((y) * slope) - 20;

            // Make sure we have 20px padding on all sides roughly
            // Actually, simpler math:
            // Center is width/2
            // Width of shape = 400?
            
            // Let's iterate x
            for (int x = 0; x < width; x++)
            {
                // Parallelogram logic
                // x + y*slope check
                // Let's simply fill a skewed box
                
                // x offset based on y
                float skewOffset = (height - 1 - y) * 0.4f; // Manual skew 
                
                if (x >= 40 + skewOffset && x <= width - 40 + skewOffset - 60) // -60 to account for total skew width help
                {
                    // tex.SetPixel(x, y, new Color(1, 1, 1, 0.8f)); // White semi-transparent
                }
            }
        }
        
        // Better approach: Draw Polygon properly
        Color fillColor = new Color(1, 1, 1, 1f); // White solid (alpha handled by UI color)
        
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                // Defined by two lines
                // x = y * 0.3 + offset (Left edge)
                // x = y * 0.3 + offset + width (Right edge)
                
                float skew = 0.35f; // Skew amount
                float leftEdge = (height - y) * skew + 20;
                float rightEdge = leftEdge + 380;
                
                if (x >= leftEdge && x <= rightEdge)
                {
                    // Add simple Anti-aliasing border
                    float alpha = 1f;
                    if (x < leftEdge + 1) alpha = x - leftEdge;
                    else if (x > rightEdge - 1) alpha = rightEdge - x;
                    
                    if (y < 2) alpha *= (y / 2f);
                    else if (y > height - 3) alpha *= ((height - y) / 3f);

                    tex.SetPixel(x, y, new Color(1, 1, 1, alpha));
                }
            }
        }

        tex.Apply();
        byte[] bytes = tex.EncodeToPNG();
        File.WriteAllBytes("Assets/Resources/ui_parallelogram_bg.png", bytes);
        Debug.Log("Generated ui_parallelogram_bg.png");
    }

    static void GenerateParallelogramStrip()
    {
        int width = 64;
        int height = 128;
        Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
        
        Color[] clear = new Color[width * height];
        for (int i = 0; i < clear.Length; i++) clear[i] = Color.clear;
        tex.SetPixels(clear);

        Color fillColor = Color.white; // We tint it later

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float skew = 0.35f;
                float leftEdge = (height - y) * skew + 10;
                float rightEdge = leftEdge + 20; // 20px wide strip

                if (x >= leftEdge && x <= rightEdge)
                {
                    tex.SetPixel(x, y, fillColor);
                }
            }
        }

        tex.Apply();
        byte[] bytes = tex.EncodeToPNG();
        File.WriteAllBytes("Assets/Resources/ui_parallelogram_strip.png", bytes);
        Debug.Log("Generated ui_parallelogram_strip.png");
    }
}

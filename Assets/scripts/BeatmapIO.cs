using UnityEngine;
using System.Collections.Generic;

public class BeatmapIO
{
    public static BeatmapData ReadBeatmap(string json)
    {
        return JsonUtility.FromJson<BeatmapData>(json);
    }
    
    public static string WriteBeatmap(BeatmapData data)
    {
        return JsonUtility.ToJson(data, true);
    }
}

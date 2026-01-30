using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 노트 스포너 (Game_first, Game_second 씬 모두 지원)
/// </summary>
public class NoteSpawner : MonoBehaviour
{
    [Header("Note Settings")]
    public GameObject notePrefab;
    public Transform[] spawnPoints;
    public float noteSpeed = 5f;

    [Header("Beatmap")]
    public BeatmapData beatmap;
    private int currentNoteIndex = 0;

    [Header("Timing")]
    public float songBPM = 120f; // 기본 BPM (씬에 따라 조정)
    private bool isSpawning = false;

    // [FIX] Public Reference for Manual Assignment (Backup)
    public BubbleSpawner bubbleSpawnerReference;

    private void Awake()
    {
        // [안전장치] SpawnPoints가 연결 안 되어 있으면 자동으로 찾기
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            List<Transform> points = new List<Transform>();
            // 4개 레인 가정
            for (int i = 0; i < 4; i++)
            {
                GameObject obj = GameObject.Find($"SpawnPoint{i}");
                if (obj == null)
                {
                    // 없으면 임시로 생성
                    obj = new GameObject($"SpawnPoint{i}");
                    obj.transform.position = new Vector3(-1.5f + i, 6f, 0f); // 대략적인 위치
                }
                points.Add(obj.transform);
            }
            spawnPoints = points.ToArray();
        }
    }

    private void Start()
    {
        // 씬에 따라 BPM 조정
        if (GameManager.Instance != null && GameManager.Instance.isSecondScene)
        {
            songBPM = 128f; // Sodapop BPM
        }
        
        // 테스트용 비트맵 생성 (게임 시작 시 한 번만)
        GenerateTestBeatmap();
    }

    private void Update()
    {
        if (GameManager.Instance == null || !GameManager.Instance.isPlaying || GameManager.Instance.isPaused) return;

        if (isSpawning)
        {
            CheckAndSpawnNotes();
        }
    }

    public void StartSpawning()
    {
        isSpawning = true;
        currentNoteIndex = 0;
    }

    private void CheckAndSpawnNotes()
    {
        if (beatmap == null || beatmap.notes == null) return;

        while (currentNoteIndex < beatmap.notes.Count)
        {
            NoteData noteData = beatmap.notes[currentNoteIndex];
            float spawnTime = noteData.time - 1.5f; // [조정] 더 빠른 노트 도착 (2초 → 1.5초)

            if (GameManager.Instance.songPosition >= spawnTime)
            {
                SpawnNote(noteData);
                currentNoteIndex++;
            }
            else break;
        }
    }

    [Header("Note Visuals")]
    public Material[] noteMaterials; // User can drag Glass 0-7 here

    private void SpawnNote(NoteData noteData)
    {
        if (noteData.lane < 0 || noteData.lane >= spawnPoints.Length) return;

        // [중요] 구멍이 비어있으면 에러 없이 넘어가는 안전장치
        if (spawnPoints[noteData.lane] == null) return;

        if (notePrefab == null)
        {
             Debug.LogError("Note Prefab이 없습니다!");
             return;
        }

        GameObject noteObj = Instantiate(notePrefab, spawnPoints[noteData.lane].position, Quaternion.identity);
        
        // 1. Game Logic Init
        Note note = noteObj.GetComponent<Note>();
        if (note != null)
        {
            note.Initialize(noteData.lane, noteData.time, noteSpeed);
        }

        // 2. Visuals Init
        BubbleNote bubble = noteObj.GetComponent<BubbleNote>();
        if (bubble != null)
        {
            bubble.Initialize(1.5f); // [조정] 더 빠른 수축 (2.0 → 1.5)
            // Note: New BubbleNote manages its own colors internally
        }
    }

    [ContextMenu("Generate Test Beatmap")]
    public void GenerateTestBeatmap()
    {
        if (beatmap == null) beatmap = new BeatmapData();
        beatmap.notes.Clear();

        // [FIX] Get accurate song length
        float songLength = 180f; // Default
        if (GameManager.Instance != null && GameManager.Instance.gameMusic != null && GameManager.Instance.gameMusic.length > 0)
        {
            songLength = GameManager.Instance.gameMusic.length;
            Debug.Log($"[NoteSpawner] Detected Song Length: {songLength}");
        }

        float currentTime = 2.0f; // Start delay
        bool isSecondScene = GameManager.Instance != null && GameManager.Instance.isSecondScene;

        if (isSecondScene) // Sodapop (Harder)
        {
            float beatInterval = 60f / 128f; // 128 BPM
            while (currentTime < songLength - 3.0f)
            {
                // Consistent beat spawning
                if (Random.value > 0.3f) // 70% chance per beat
                {
                    int lane = Random.Range(0, 4);
                    beatmap.notes.Add(new NoteData { lane = lane, time = currentTime });
                }
                currentTime += beatInterval;
            }
        }
        else // Galaxias (Easy/Normal)
        {
            float beatInterval = 60f / 120f; // 120 BPM base
            // [FIX] Ensure notes spawn UNTIL THE END
            while (currentTime < songLength - 3.0f) 
            {
                // Spawn randomly every beat or two
                if (Random.value > 0.4f)
                {
                    int lane = Random.Range(0, 4);
                    beatmap.notes.Add(new NoteData { lane = lane, time = currentTime });
                }
                currentTime += beatInterval;
            }
        }

        // Sort by time
        beatmap.notes.Sort((a, b) => a.time.CompareTo(b.time));
        
        Debug.Log($"[NoteSpawner] Generated {beatmap.notes.Count} notes covering {songLength} seconds.");
    }
}

[System.Serializable]
public class BeatmapData
{
    public string songName;
    public float bpm;
    public List<NoteData> notes = new List<NoteData>();
}

[System.Serializable]
public class NoteData
{
    public int lane;
    public float time;
}
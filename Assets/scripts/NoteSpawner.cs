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
            float spawnTime = noteData.time - 2f; // 노트가 판정선에 도달하기 2초 전에 생성

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
            bubble.Initialize(2.0f); // Just init visuals

            // Assign Random Material from Manager (NoteSpawner)
            if (noteMaterials != null && noteMaterials.Length > 0)
            {
                Material randomMat = noteMaterials[Random.Range(0, noteMaterials.Length)];
                bubble.SetMaterial(randomMat);
            }
        }
    }

    [ContextMenu("Generate Test Beatmap")]
    public void GenerateTestBeatmap()
    {
        if (beatmap == null) beatmap = new BeatmapData();
        beatmap.notes.Clear();

        float beatInterval = 60f / songBPM; // 1박자당 시간
        float currentTime = 2.0f; // 게임 시작 후 2초부터

        // 씬에 따라 다른 패턴 생성
        bool isSecondScene = GameManager.Instance != null && GameManager.Instance.isSecondScene;
        
        // [FIX] Generate notes for the full song duration
        float songLength = 180f; // Default 3 mins if clip is null
        if (GameManager.Instance != null && GameManager.Instance.gameMusic != null)
        {
            songLength = GameManager.Instance.gameMusic.length;
        }

        if (isSecondScene)
        {
            // Game_second: Sodapop (Complex Pattern)
            // Generate until song ends
            int measures = Mathf.CeilToInt((songLength - 2.0f) / (beatInterval * 4f));
            
            for (int measure = 0; measure < measures; measure++)
            {
                for (int beat = 0; beat < 4; beat++)
                {
                    if (currentTime > songLength - 2.0f) break; // Stop before end

                    if (Random.value > 0.5f)
                    {
                        int randomLane = Random.Range(0, 4);
                        beatmap.notes.Add(new NoteData { 
                            lane = randomLane, 
                            time = currentTime + (beat * beatInterval) 
                        });
                    }
                    
                    if (Random.value > 0.75f)
                    {
                        int randomLane = Random.Range(0, 4);
                        beatmap.notes.Add(new NoteData { 
                            lane = randomLane, 
                            time = currentTime + (beat * beatInterval) + (beatInterval * 0.5f) 
                        });
                    }
                }
                currentTime += beatInterval * 4f;
            }
        }
        else
        {
            // Game_first: Regular Pattern (Galaxias) -> [FIX] EXTREME EASY MODE
            // Reduced density: Spawn every 2.0s
            
            float safeZone = 5.0f; // Seconds before song starts
            currentTime = safeZone;

            while (currentTime < songLength - 5.0f) // Buffer at end
            {
                // Sparse spawning: 10% chance every 2.0 seconds
                // Valid for absolute beginners
                if (Random.value > 0.9f) 
                {
                    int randomLane = Random.Range(0, 4);
                    beatmap.notes.Add(new NoteData { lane = randomLane, time = currentTime });
                }
                
                currentTime += 2.0f; // Extremely Slow pace
            }
        }

        // 시간순으로 정렬
        beatmap.notes.Sort((a, b) => a.time.CompareTo(b.time));
        
        string sceneName = isSecondScene ? "Sodapop" : "Galaxias";
        Debug.Log($"[NoteSpawner] {sceneName} 테스트 비트맵 생성 완료: 노트 {beatmap.notes.Count}개");
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
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 버블 스포너 (Game_first, Game_second 씬 모두 지원)
/// </summary>
public class BubbleSpawner : MonoBehaviour
{
    [Header("Scene Settings")]
    public bool isSecondScene = false;   // Game_second 씬인지 여부

    [Header("Settings")]
    public GameObject bubblePrefab;
    public Material[] bubbleMaterials;

    [Header("Spawn Area (User Defined)")]
    public Vector2 xRange = new Vector2(29f, 49f);
    public Vector2 yRange = new Vector2(10f, 20f);
    public float fixedZ = 10f;

    [Header("Timing")]
    public float lifeTime = 2.0f; 
    [Range(0f, 1f)]
    public float spawnDensity = 0.8f; 

    [Header("Beatmap (Game_first)")]
    public BeatmapData beatmap;
    private int currentNoteIndex = 0;

    [Header("Beat Settings (Game_second)")]
    public float bpm = 128f;                 // Sodapop의 BPM
    public float beatOffset = 0f;            // 박자 오프셋 조정
    public float[] beatPattern = { 0f, 0.5f, 1f, 1.5f, 2f, 2.5f, 3f, 3.5f }; // 8분음표 패턴
    
    private float beatInterval;              // 1박자당 시간
    private float nextBeatTime = 0f;         // 다음 박자 시간
    private int currentBeatIndex = 0;        // 현재 박자 패턴 인덱스
    private bool spawningStarted = false;    // 스포닝 시작 여부

    private List<Transform> activeBubbles = new List<Transform>();

    private void Start()
    {
        if (isSecondScene)
        {
            // Game_second: 박자 기반 스포닝
            beatInterval = 60f / bpm; // 1박자당 시간 계산
            Debug.Log($"[BubbleSpawner] Sodapop 버블 스포너 초기화! BPM: {bpm}");
        }
        else
        {
            // Game_first: 비트맵 기반 스포닝
            LoadBeatmap("Beatmaps/Galaxias");
        }
    }

    private void LoadBeatmap(string path)
    {
        TextAsset jsonFile = Resources.Load<TextAsset>(path);
        if (jsonFile == null)
        {
            Debug.LogError($"비트맵 파일을 찾을 수 없습니다: {path}");
            return;
        }

        beatmap = BeatmapIO.ReadBeatmap(jsonFile.text);
    }

    private void Update()
    {
        if (GameManager.Instance == null) return;
        
        if (!GameManager.Instance.isPlaying || GameManager.Instance.isPaused) return;

        if (isSecondScene)
        {
            // Game_second: 박자 기반 스포닝
            UpdateBeatSpawning();
        }
        else
        {
            // Game_first: 비트맵 기반 스포닝
            UpdateBeatmapSpawning();
        }
    }

    private void UpdateBeatmapSpawning()
    {
        GameManager.Instance.songPosition += Time.deltaTime;

        if (beatmap != null)
        {
            CheckAndSpawnNotes();
        }
    }

    private void UpdateBeatSpawning()
    {
        // 게임이 시작되면 스포닝 활성화
        if (!spawningStarted && GameManager.Instance.songPosition > 0f)
        {
            StartBeatSpawning();
        }

        if (spawningStarted)
        {
            CheckBeatSpawning();
        }
    }

    private void StartBeatSpawning()
    {
        spawningStarted = true;
        // 첫 박자 시간 설정 (현재 노래 시간 + 오프셋 + 버블 생존시간)
        nextBeatTime = GameManager.Instance.songPosition + beatOffset + lifeTime;
        currentBeatIndex = 0;
        
        Debug.Log($"[BubbleSpawner] 박자 스포닝 시작! 첫 박자 시간: {nextBeatTime}");
    }

    private void CheckBeatSpawning()
    {
        if (GameManager.Instance.songPosition >= nextBeatTime)
        {
            if (Random.value <= spawnDensity)
            {
                SpawnBubble();
            }
            
            // 다음 박자 시간 계산
            CalculateNextBeatTime();
        }
    }

    private void CalculateNextBeatTime()
    {
        currentBeatIndex = (currentBeatIndex + 1) % beatPattern.Length;
        
        // 만약 패턴이 처음으로 돌아갔다면 다음 마디로
        if (currentBeatIndex == 0)
        {
            nextBeatTime += beatInterval * 4f; // 4박자 후 (한 마디)
        }
        else
        {
            nextBeatTime += beatInterval * 0.5f; // 8분음표 간격
        }
    }

    private void CheckAndSpawnNotes()
    {
        while (currentNoteIndex < beatmap.notes.Count)
        {
            NoteData noteData = beatmap.notes[currentNoteIndex];
            float spawnTime = noteData.time - lifeTime;

            if (GameManager.Instance.songPosition >= spawnTime)
            {
                if (Random.value <= spawnDensity)
                {
                    SpawnBubble();
                }
                currentNoteIndex++;
            }
            else
            {
                break;
            }
        }
    }

    private void SpawnBubble()
    {
        if (bubblePrefab == null) return;

        activeBubbles.RemoveAll(b => b == null);

        Vector3 spawnPos = Vector3.zero;
        bool validPositionFound = false;
        
        // [UPDATE] 버블 크기를 적절하게 조정 (2.5~3.5)
        float randomScale = Random.Range(2.0f, 3.0f);

        for (int i = 0; i < 10; i++)
        {
            Vector3 candidate = new Vector3(
                Random.Range(xRange.x, xRange.y),
                Random.Range(yRange.x, yRange.y),
                fixedZ
            );

            if (!IsOverlapping(candidate, randomScale))
            {
                spawnPos = candidate;
                validPositionFound = true;
                break;
            }
        }

        if (!validPositionFound) return; 

        GameObject bubble = Instantiate(bubblePrefab, spawnPos, Quaternion.identity);
        bubble.transform.localScale = Vector3.one * randomScale;
        activeBubbles.Add(bubble.transform);

        if (bubbleMaterials != null && bubbleMaterials.Length > 0)
        {
            Material randomMat = bubbleMaterials[Random.Range(0, bubbleMaterials.Length)];
            Renderer rd = bubble.GetComponent<Renderer>();
            if (rd != null)
            {
                rd.material = randomMat;
            }
        }

        if (bubble.GetComponent<Collider>() == null)
            bubble.AddComponent<SphereCollider>();

        BubbleNote noteLogic = bubble.AddComponent<BubbleNote>();
        noteLogic.Initialize(lifeTime); 
    }

    private bool IsOverlapping(Vector3 center, float scale)
    {
        // 반지름 계수 조정
        float myRadius = 0.7f * scale; 
        foreach (Transform t in activeBubbles)
        {
            if (t == null) continue;
            float otherRadius = 0.7f * t.localScale.x; 
            float dist = Vector3.Distance(center, t.position);
            if (dist < (myRadius + otherRadius)) return true; 
        }
        return false; 
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Vector3 center = new Vector3((xRange.x + xRange.y) / 2, (yRange.x + yRange.y) / 2, fixedZ);
        Vector3 size = new Vector3(xRange.y - xRange.x, yRange.y - yRange.x, 1f);
        Gizmos.DrawWireCube(center, size);
    }

    // 박자 설정 조정 메서드들 (Game_second용)
    public void SetBPM(float newBPM)
    {
        bpm = newBPM;
        beatInterval = 60f / bpm;
        Debug.Log($"[BubbleSpawner] BPM 변경: {bpm}");
    }

    public void SetBeatOffset(float offset)
    {
        beatOffset = offset;
        Debug.Log($"[BubbleSpawner] Beat Offset 변경: {beatOffset}");
    }

    public void SetBeatPattern(float[] newPattern)
    {
        beatPattern = newPattern;
        currentBeatIndex = 0; // 패턴 변경시 인덱스 리셋
        Debug.Log($"[BubbleSpawner] Beat Pattern 변경: {beatPattern.Length}개 패턴");
    }

    // 스포닝 강제 중지/재시작 (Game_second용)
    public void StopSpawning()
    {
        spawningStarted = false;
        Debug.Log("[BubbleSpawner] 스포닝 중지");
    }

    public void ResumeSpawning()
    {
        if (GameManager.Instance != null && GameManager.Instance.isPlaying)
        {
            if (isSecondScene)
            {
                StartBeatSpawning();
            }
        }
    }
}

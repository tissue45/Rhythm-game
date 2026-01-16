using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 노트를 생성하고 관리하는 스포너
/// </summary>
public class NoteSpawner : MonoBehaviour
{
    [Header("Note Settings")]
    public GameObject notePrefab;
    public Transform[] spawnPoints; // 4개의 레인
    public float noteSpeed = 5f;

    [Header("Beatmap")]
    public BeatmapData beatmap;
    private int currentNoteIndex = 0;

    [Header("Timing")]
    public float songPosition = 0f;
    public float songBPM = 120f;
    private float secPerBeat;

    private bool isSpawning = false;

    private void Start()
    {
        secPerBeat = 60f / songBPM;
    }

    private void Update()
    {
        if (!GameManager.Instance.isPlaying || GameManager.Instance.isPaused)
            return;

        // 음악 시간 업데이트
        songPosition += Time.deltaTime;

        // 노트 생성 체크
        if (isSpawning)
        {
            CheckAndSpawnNotes();
        }
    }

    /// <summary>
    /// 노트 생성 시작
    /// </summary>
    public void StartSpawning()
    {
        isSpawning = true;
        currentNoteIndex = 0;
    }

    /// <summary>
    /// 노트 생성 체크 및 실행
    /// </summary>
    private void CheckAndSpawnNotes()
    {
        if (beatmap == null || beatmap.notes == null)
            return;

        while (currentNoteIndex < beatmap.notes.Count)
        {
            NoteData noteData = beatmap.notes[currentNoteIndex];

            // 노트 생성 타이밍 체크 (미리 생성)
            float spawnTime = noteData.time - 2f; // 2초 전에 생성

            if (songPosition >= spawnTime)
            {
                SpawnNote(noteData);
                currentNoteIndex++;
            }
            else
            {
                break;
            }
        }
    }

    /// <summary>
    /// 노트 생성
    /// </summary>
    private void SpawnNote(NoteData noteData)
    {
        if (noteData.lane < 0 || noteData.lane >= spawnPoints.Length)
            return;

        if (spawnPoints[noteData.lane] == null)
        {
            Debug.LogError($"Spawn Point for Lane {noteData.lane} is missing!");
            return;
        }

        GameObject noteObj = Instantiate(notePrefab, spawnPoints[noteData.lane].position, Quaternion.identity);
        Note note = noteObj.GetComponent<Note>();

        if (note != null)
        {
            note.Initialize(noteData.lane, noteData.time, noteSpeed);
        }
    }

    /// <summary>
    /// 테스트용 비트맵 자동 생성 (Inspector에서 우클릭으로 실행)
    /// </summary>
    [ContextMenu("Generate Test Beatmap")]
    public void GenerateTestBeatmap()
    {
        if (beatmap == null) beatmap = new BeatmapData();
        beatmap.notes.Clear();

        float currentTime = 2.0f; // 2초부터 시작
        for (int i = 0; i < 50; i++) // 노트 50개 생성
        {
            int randomLane = Random.Range(0, 4); // 0~3 레인 랜덤
            beatmap.notes.Add(new NoteData { lane = randomLane, time = currentTime });
            currentTime += 0.5f; // 0.5초 간격 (BPM 120 기준 4분음표)
        }
        
        Debug.Log("테스트 비트맵 생성 완료: 노트 50개");
    }
}

/// <summary>
/// 비트맵 데이터 (ScriptableObject로 만들 수도 있음)
/// </summary>
[System.Serializable]
public class BeatmapData
{
    public string songName;
    public float bpm;
    public List<NoteData> notes = new List<NoteData>();
}

/// <summary>
/// 개별 노트 데이터
/// </summary>
[System.Serializable]
public class NoteData
{
    public int lane; // 0-3 (4개 레인)
    public float time; // 노트가 판정 라인에 도달하는 시간
}

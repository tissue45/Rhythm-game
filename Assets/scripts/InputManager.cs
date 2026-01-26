using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 입력 매니저 (Game_first, Game_second 씬 모두 지원)
/// </summary>
public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    // [Header("Key Settings")]
    // public KeyCode[] laneKeys = { KeyCode.D, KeyCode.F, KeyCode.J, KeyCode.K }; // 키보드 미지원

    [Header("Hit Zones")]
    public Transform[] hitZones; // 판정선 위치 (4개)
    public float hitRadius = 0.5f; // 판정 범위

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (hitZones == null || hitZones.Length == 0)
        {
            List<Transform> zones = new List<Transform>();
            for (int i = 0; i < 4; i++)
            {
                // HitZone0, HitZone1... 찾기
                GameObject obj = GameObject.Find($"HitZone{i}");
                if (obj == null)
                {
                    // 없으면 임시 생성 (판정선 위치)
                    obj = new GameObject($"HitZone{i}");
                    obj.transform.position = new Vector3(-1.5f + i, -4f, 0f); 
                }
                zones.Add(obj.transform);
            }
            hitZones = zones.ToArray();
        }
    }

    private void Update()
    {
        // 키보드 입력 제거됨 (마우스/터치 및 모바일 컨트롤러 전용)
    }

    private void CheckHit(int laneIndex)
    {
        // 해당 레인의 모든 노트를 찾아서 거리 체크
        // FindObjectsOfType -> FindObjectsByType (경고 수정)
        Note[] notes = Object.FindObjectsByType<Note>(FindObjectsSortMode.None);
        
        Note closestNote = null;
        float minDistance = float.MaxValue;
        foreach (Note note in notes)
        {
            if (note.lane == laneIndex)
            {
                float distance = Mathf.Abs(note.transform.position.y - hitZones[laneIndex].position.y);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestNote = note;
                }
            }
        }
        // 판정 범위 안에 들어왔으면 Hit!
        if (closestNote != null && minDistance < hitRadius)
        {
            closestNote.OnHit();
        }
    }

    private void ShowHitEffect(int laneIndex)
    {
        // TODO: 버튼 눌림 효과 (색상 변경 등)
        Debug.Log($"Lane {laneIndex} Key Pressed");
    }

    // 레인 트리거 (외부 호출용) -> 99번은 Auto-Aim으로 처리
    public void TriggerLane(int laneIndex)
    {
        if (GameManager.Instance == null || GameManager.Instance.isPaused) return;

        if (laneIndex == 99)
        {
            CheckAutoHit();
        }
        else
        {
            CheckHit(laneIndex);
            ShowHitEffect(laneIndex);
        }
    }

    private void CheckAutoHit()
    {
        // 모든 레인의 노트를 검사하여 가장 가까운 것 찾기
        Note[] notes = Object.FindObjectsByType<Note>(FindObjectsSortMode.None);
        
        Note closestNote = null;
        float minDistance = float.MaxValue;
        int targetLane = -1;

        foreach (Note note in notes)
        {
            // 각 노트의 해당 레인 판정선과의 거리
            float distance = Mathf.Abs(note.transform.position.y - hitZones[note.lane].position.y);
            
            // 더 가까운 노트 발견
            if (distance < minDistance)
            {
                minDistance = distance;
                closestNote = note;
                targetLane = note.lane;
            }
        }

        // 판정 범위 내에 들어왔으면 Hit!
        if (closestNote != null && minDistance < hitRadius)
        {
            closestNote.OnHit();
            ShowHitEffect(targetLane);
            Debug.Log($"[Auto-Aim] Hit Lane {targetLane} (Dist: {minDistance:F2})");
        }
        else
        {
            Debug.Log("[Auto-Aim] No hittable note found.");
        }
    }
}
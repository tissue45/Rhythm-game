using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 입력 매니저 (Game_first, Game_second 씬 모두 지원)
/// </summary>
public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    [Header("Key Settings")]
    public KeyCode[] laneKeys = { KeyCode.D, KeyCode.F, KeyCode.J, KeyCode.K };

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
        if (GameManager.Instance == null || GameManager.Instance.isPaused) return;
        
        for (int i = 0; i < laneKeys.Length; i++)
        {
            if (Input.GetKeyDown(laneKeys[i]))
            {
                CheckHit(i);
                ShowHitEffect(i);
            }
        }
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

    // 외부(네트워크 등)에서 입력을 발생시키는 함수
    public void TriggerLane(int laneIndex)
    {
        if (GameManager.Instance == null || GameManager.Instance.isPaused) return;
        CheckHit(laneIndex);
        ShowHitEffect(laneIndex);
    }
}
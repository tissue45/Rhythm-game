using UnityEngine;

/// <summary>
/// 플레이어 입력을 감지하고 노트를 처리
/// </summary>
public class InputManager : MonoBehaviour
{
    [Header("Lane Settings")]
    public KeyCode[] laneKeys = new KeyCode[4] 
    { 
        KeyCode.D,  // Lane 0
        KeyCode.F,  // Lane 1
        KeyCode.J,  // Lane 2
        KeyCode.K   // Lane 3
    };

    [Header("Hit Detection")]
    public Transform[] hitZones; // 각 레인의 판정 영역
    public float hitRange = 0.5f;

    private void Update()
    {
        if (!GameManager.Instance.isPlaying || GameManager.Instance.isPaused)
            return;

        // 각 레인의 입력 체크
        for (int i = 0; i < laneKeys.Length; i++)
        {
            if (Input.GetKeyDown(laneKeys[i]))
            {
                CheckNoteHit(i);
            }
        }
    }

    /// <summary>
    /// 해당 레인의 노트 히트 체크
    /// </summary>
    private void CheckNoteHit(int lane)
    {
        if (lane < 0 || lane >= hitZones.Length)
            return;

        // 판정 영역 내의 노트 찾기
        Collider[] hitColliders = Physics.OverlapSphere(
            hitZones[lane].position, 
            hitRange
        );

        Note closestNote = null;
        float closestDistance = float.MaxValue;

        // 가장 가까운 노트 찾기
        foreach (Collider col in hitColliders)
        {
            Note note = col.GetComponent<Note>();
            if (note != null && note.lane == lane)
            {
                float distance = Vector3.Distance(
                    note.transform.position, 
                    hitZones[lane].position
                );

                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestNote = note;
                }
            }
        }

        // 노트 히트 처리
        if (closestNote != null)
        {
            closestNote.OnHit();
        }
    }

    // Gizmo로 판정 영역 표시 (에디터에서만)
    private void OnDrawGizmos()
    {
        if (hitZones == null)
            return;

        Gizmos.color = Color.yellow;
        foreach (Transform hitZone in hitZones)
        {
            if (hitZone != null)
            {
                Gizmos.DrawWireSphere(hitZone.position, hitRange);
            }
        }
    }
}

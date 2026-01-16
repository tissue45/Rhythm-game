using UnityEngine;

/// <summary>
/// 개별 노트의 동작을 관리
/// </summary>
public class Note : MonoBehaviour
{
    [Header("Note Info")]
    public int lane;
    public float targetTime;
    public float speed;

    [Header("Judgment Line")]
    public float judgmentLineY = -4f;

    [Header("Visual")]
    public Renderer noteRenderer;
    public Color[] laneColors = new Color[4]; // 레인별 색상

    private bool hasBeenHit = false;
    private float spawnTime;

    /// <summary>
    /// 노트 초기화
    /// </summary>
    public void Initialize(int lane, float targetTime, float speed)
    {
        this.lane = lane;
        this.targetTime = targetTime;
        this.speed = speed;
        this.spawnTime = Time.time;

        // 레인별 색상 설정
        if (noteRenderer != null && lane < laneColors.Length)
        {
            noteRenderer.material.color = laneColors[lane];
        }
    }

    private void Update()
    {
        if (GameManager.Instance.isPaused)
            return;

        // 아래로 이동
        transform.position += Vector3.down * speed * Time.deltaTime;

        // 판정 라인을 지나쳤는지 체크
        if (transform.position.y < judgmentLineY - 1f && !hasBeenHit)
        {
            // Miss 처리
            OnMiss();
        }
    }

    [Header("Effects")]
    public GameObject hitEffectPrefab; // 타격 이펙트 프리팹

    /// <summary>
    /// 노트 입력 처리
    /// </summary>
    public void OnHit()
    {
        if (hasBeenHit)
            return;

        hasBeenHit = true;

        // 판정 라인과의 거리로 정확도 계산
        float distance = Mathf.Abs(transform.position.y - judgmentLineY);
        JudgmentType judgment = CalculateJudgment(distance);

        // 점수 추가
        int points = GetPointsForJudgment(judgment);
        GameManager.Instance.AddScore(points);

        // 판정 이펙트 표시
        ShowJudgmentEffect(judgment);

        // 타격 이펙트 생성 (파티클)
        if (hitEffectPrefab != null)
        {
            Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
        }

        // 노트 제거
        Destroy(gameObject);
    }

    /// <summary>
    /// Miss 처리
    /// </summary>
    private void OnMiss()
    {
        hasBeenHit = true;
        GameManager.Instance.ResetCombo();
        ShowJudgmentEffect(JudgmentType.Miss);
        Destroy(gameObject);
    }

    /// <summary>
    /// 정확도 판정
    /// </summary>
    private JudgmentType CalculateJudgment(float distance)
    {
        if (distance < 0.1f)
            return JudgmentType.Perfect;
        else if (distance < 0.3f)
            return JudgmentType.Great;
        else if (distance < 0.5f)
            return JudgmentType.Good;
        else
            return JudgmentType.Miss;
    }

    /// <summary>
    /// 판정별 점수 반환
    /// </summary>
    private int GetPointsForJudgment(JudgmentType judgment)
    {
        switch (judgment)
        {
            case JudgmentType.Perfect:
                return 100;
            case JudgmentType.Great:
                return 75;
            case JudgmentType.Good:
                return 50;
            default:
                return 0;
        }
    }

    /// <summary>
    /// 판정 이펙트 표시
    /// </summary>
    private void ShowJudgmentEffect(JudgmentType judgment)
    {
        // TODO: 파티클 이펙트나 텍스트 표시
        Debug.Log($"Judgment: {judgment}");
    }
}

/// <summary>
/// 판정 타입
/// </summary>
public enum JudgmentType
{
    Perfect,
    Great,
    Good,
    Miss
}

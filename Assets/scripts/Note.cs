using UnityEngine;

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
    public Color[] laneColors = new Color[4];

    private bool hasBeenHit = false;

    [Header("Effects")]
    public GameObject hitEffectPrefab; // 타격 이펙트

    public void Initialize(int lane, float targetTime, float speed)
    {
        this.lane = lane;
        this.targetTime = targetTime;
        this.speed = speed;

        if (noteRenderer != null && lane < laneColors.Length)
        {
            noteRenderer.material.color = laneColors[lane];
        }
    }

    private void Update()
    {
        // GameManager 참조 (통합됨)
        if (GameManager.Instance == null || GameManager.Instance.isPaused) return;

        transform.position += Vector3.down * speed * Time.deltaTime;

        if (transform.position.y < judgmentLineY - 1f && !hasBeenHit)
        {
            OnMiss();
        }
    }

    public void OnHit()
    {
        if (hasBeenHit) return;
        hasBeenHit = true;

        float distance = Mathf.Abs(transform.position.y - judgmentLineY);
        JudgmentType judgment = CalculateJudgment(distance);

        // 점수 올리기 - GameManager 참조 (통합됨)
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddScore(GetPointsForJudgment(judgment));
        }

        // 이펙트 터트리기
        if (hitEffectPrefab != null)
        {
            Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
        }

        Destroy(gameObject);
    }

    private void OnMiss()
    {
        hasBeenHit = true;
        
        // 콤보 리셋 - GameManager 참조 (통합됨)
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ResetCombo();
        }
        
        Destroy(gameObject);
    }

    private JudgmentType CalculateJudgment(float distance)
    {
        if (distance < 0.1f) return JudgmentType.Perfect;
        else if (distance < 0.3f) return JudgmentType.Great;
        else if (distance < 0.5f) return JudgmentType.Good;
        else return JudgmentType.Miss;
    }

    private int GetPointsForJudgment(JudgmentType judgment)
    {
        switch (judgment)
        {
            case JudgmentType.Perfect: return 100;
            case JudgmentType.Great: return 75;
            case JudgmentType.Good: return 50;
            default: return 0;
        }
    }
}

public enum JudgmentType { Perfect, Great, Good, Miss }
using UnityEngine;
using VRM;

/// <summary>
/// VRM 아바타 애니메이션 및 제어
/// </summary>
public class AvatarController : MonoBehaviour
{
    [Header("VRM Components")]
    public Animator animator;
    public VRMBlendShapeProxy blendShapeProxy;

    [Header("Animation Settings")]
    public string idleAnimationName = "Idle";
    public string danceAnimationName = "Dance";
    public string hitAnimationName = "Hit";

    [Header("Lane Positions")]
    public Transform[] lanePositions; // 4개 레인에 대응하는 위치
    public float moveSpeed = 5f;

    private int currentLane = 1; // 시작 레인 (중앙)
    private Vector3 targetPosition;

    private void Start()
    {
        if (lanePositions.Length > 0)
        {
            targetPosition = lanePositions[currentLane].position;
            transform.position = targetPosition;
        }

        // 기본 애니메이션 재생
        PlayAnimation(idleAnimationName);
    }

    private void Update()
    {
        // 부드러운 이동
        transform.position = Vector3.Lerp(
            transform.position, 
            targetPosition, 
            moveSpeed * Time.deltaTime
        );
    }

    /// <summary>
    /// 레인 이동
    /// </summary>
    public void MoveToLane(int lane)
    {
        if (lane < 0 || lane >= lanePositions.Length)
            return;

        currentLane = lane;
        targetPosition = lanePositions[lane].position;
    }

    /// <summary>
    /// 히트 액션 (노트를 칠 때)
    /// </summary>
    public void OnNoteHit(JudgmentType judgment)
    {
        // 판정에 따른 표정 변경
        if (blendShapeProxy != null)
        {
            switch (judgment)
            {
                case JudgmentType.Perfect:
                    SetExpression("Joy", 1f);
                    break;
                case JudgmentType.Great:
                    SetExpression("Fun", 0.7f);
                    break;
                case JudgmentType.Good:
                    SetExpression("Neutral", 0.5f);
                    break;
                case JudgmentType.Miss:
                    SetExpression("Sorrow", 1f);
                    break;
            }
        }

        // 히트 애니메이션 재생
        PlayAnimation(hitAnimationName);
    }

    /// <summary>
    /// 애니메이션 재생
    /// </summary>
    private void PlayAnimation(string animationName)
    {
        if (animator != null && !string.IsNullOrEmpty(animationName))
        {
            animator.Play(animationName);
        }
    }

    /// <summary>
    /// VRM 표정 설정
    /// </summary>
    private void SetExpression(string expressionName, float weight)
    {
        if (blendShapeProxy == null)
            return;

        // 모든 표정 초기화
        blendShapeProxy.ImmediatelySetValue(BlendShapePreset.Joy, 0f);
        blendShapeProxy.ImmediatelySetValue(BlendShapePreset.Fun, 0f);
        blendShapeProxy.ImmediatelySetValue(BlendShapePreset.Sorrow, 0f);
        blendShapeProxy.ImmediatelySetValue(BlendShapePreset.Angry, 0f);

        // 선택한 표정 설정
        switch (expressionName)
        {
            case "Joy":
                blendShapeProxy.ImmediatelySetValue(BlendShapePreset.Joy, weight);
                break;
            case "Fun":
                blendShapeProxy.ImmediatelySetValue(BlendShapePreset.Fun, weight);
                break;
            case "Sorrow":
                blendShapeProxy.ImmediatelySetValue(BlendShapePreset.Sorrow, weight);
                break;
            case "Neutral":
                blendShapeProxy.ImmediatelySetValue(BlendShapePreset.Neutral, weight);
                break;
        }
    }

    /// <summary>
    /// 춤 시작
    /// </summary>
    public void StartDancing()
    {
        PlayAnimation(danceAnimationName);
    }

    /// <summary>
    /// 춤 멈춤
    /// </summary>
    public void StopDancing()
    {
        PlayAnimation(idleAnimationName);
    }
}

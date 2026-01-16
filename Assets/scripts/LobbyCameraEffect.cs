using UnityEngine;

public class LobbyCameraEffect : MonoBehaviour
{
    [Header("Parallax Settings")]
    public float moveAmount = 0.1f;   // 움직임 강도 (0.5 -> 0.1로 대폭 감소)
    public float smoothTime = 0.2f;   // 부드러운 정도 (0.1 -> 0.2로 증가)

    private Vector3 startPosition;
    private Vector3 targetPosition;
    private Vector3 currentVelocity;

    void Start()
    {
        startPosition = transform.position;
    }

    void Update()
    {
        // 마우스 위치를 -1 ~ 1 사이의 값으로 변환 (화면 중앙 기준)
        float x = (Input.mousePosition.x / Screen.width) * 2 - 1;
        float y = (Input.mousePosition.y / Screen.height) * 2 - 1;

        // 목표 위치 계산 (기본 위치 + 마우스 오프셋)
        // x축 마우스 이동 -> 카메라 x축 이동 (반대 방향으로 움직이면 더 입체감 있음, 여기선 정방향)
        // y축 마우스 이동 -> 카메라 y축 이동
        Vector3 offset = new Vector3(x * moveAmount, y * moveAmount, 0);
        targetPosition = startPosition + offset;

        // 부드럽게 이동
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref currentVelocity, smoothTime);
    }
}

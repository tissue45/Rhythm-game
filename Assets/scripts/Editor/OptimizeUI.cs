using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

/// <summary>
/// UI 최적화 도구: 불필요한 Raycast Target 비활성화
/// 모바일 성능 향상에 필수적입니다.
/// </summary>
public class OptimizeUI : EditorWindow
{
    [MenuItem("Rhythm Game/Optimize UI Raycasts")]
    public static void OptimizeRaycasts()
    {
        int count = 0;
        // 씬의 모든 그래픽 컴포넌트 찾기 (Image, Text, TMP 등)
        Graphic[] graphics = FindObjectsOfType<Graphic>(true);

        foreach (Graphic g in graphics)
        {
            // 이미 꺼져있으면 패스
            if (!g.raycastTarget) continue;

            GameObject go = g.gameObject;

            // 1. 버튼, 토글, 슬라이더 등 인터랙터블이 있으면 유지
            if (go.GetComponent<Selectable>() != null) continue;

            // 2. 이벤트 트리거가 있으면 유지 (클릭 이벤트 등)
            if (go.GetComponent<EventTrigger>() != null) continue;

            // 3. IPointer 인터페이스를 구현한 커스텀 스크립트가 있으면 유지 (예: RhythmButtonStyle)
            // 주의: 모든 컴포넌트를 뒤져야 함
            bool hasPointerHandler = false;
            MonoBehaviour[] scripts = go.GetComponents<MonoBehaviour>();
            foreach (var script in scripts)
            {
                if (script is IPointerEnterHandler || script is IPointerClickHandler || script is IPointerDownHandler || script is IPointerUpHandler || script is IDragHandler)
                {
                    hasPointerHandler = true;
                    break;
                }
            }
            if (hasPointerHandler) continue;

            // 4. InputField의 텍스트/플레이스홀더인 경우 유지 (필수 아닐 수 있으나 안전을 위해)
            if (go.GetComponentInParent<TMP_InputField>() != null || go.GetComponentInParent<InputField>() != null)
            {
                // InputField의 자식이면 일단 유지 (복잡성 회피)
                continue;
            }

            // 위 조건에 해당하지 않으면 RaycastTarget 끄기 (최적화)
            // 보통 텍스트, 장식용 아이콘, 배경 패널 등이 해당됨
            Undo.RecordObject(g, "Optimize RaycastTarget");
            g.raycastTarget = false;
            count++;
        }

        Debug.Log($"<b>[UI Optimization]</b> Optimized {count} UI elements by disabling RaycastTarget.");
    }
}

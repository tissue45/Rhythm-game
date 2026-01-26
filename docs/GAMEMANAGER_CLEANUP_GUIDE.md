# GameManager 정리 가이드

## 현재 상태
- **총 라인 수**: 1,701줄
- **주요 문제**: 과도한 주석, 중복 코드, 디버그 로그

## 정리 필요 항목

### 1. 제거 가능한 주석
```csharp
// [FIX] - 이미 수정 완료된 항목
// [NEW] - 더 이상 새로운 기능이 아님
// [REMOVED] - 제거된 코드 설명
// [DEBUG] - 디버그용 주석
```

### 2. 중복 기능
- `CleanupLegacyUI()` - 여러 곳에서 호출
- `gameOverPanel` null 체크 중복
- AudioSource 정리 로직 중복

### 3. 불필요한 디버그 로그
```csharp
Debug.Log("[GameManager] ...") // 너무 많음
```

## 권장 정리 방법

### 자동 정리 (추천)
```csharp
// 1. 주석 정리
// [FIX], [NEW], [REMOVED] 등 제거

// 2. 디버그 로그 조건부 처리
#if UNITY_EDITOR
    Debug.Log(...);
#endif

// 3. 중복 코드 함수화
private void CleanupAudio() { ... }
private void CleanupUI() { ... }
```

### 수동 정리
1. 주석 검색 (`Ctrl+F`): `// [FIX]`, `// [NEW]`
2. 불필요한 주석 삭제
3. 중복 코드 통합

## 핵심 기능 (유지)
- ✅ `StartGame()` - 게임 시작
- ✅ `EndGame()` - 게임 종료
- ✅ `SubmitScoreToRanking()` - 랭킹 제출
- ✅ `UpdateUI()` - UI 업데이트
- ✅ `OnBubbleMiss()` - 미스 처리
- ✅ `PerfectHitBoost()` - 퍼펙트 효과

## 제거 가능 (선택)
- ⚠️ `CleanupLegacyUI()` - 간소화 가능
- ⚠️ 과도한 null 체크
- ⚠️ 중복된 AudioSource 정리 로직

---

**참고**: GameManager는 핵심 파일이므로 대규모 리팩토링은 신중하게 진행하세요.

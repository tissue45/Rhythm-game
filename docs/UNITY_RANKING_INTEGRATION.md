# 🎮 Unity 게임 개선 완료

## ✅ 완료된 작업

### 1. **SHOP 뒤 빨간 버튼 제거** 🔴 → ⚪
**문제**: SongSelectPanel의 CLOSE 버튼이 빨간색으로 표시되어 SHOP 뒤에 보임

**해결**:
```csharp
// SchoolLobbyManager.cs 809줄
btnImg.color = new Color(0, 0, 0, 0); // 완전 투명
```

**결과**: ✅ 빨간 버튼이 완전히 투명해져 보이지 않음

---

### 2. **Unity 게임 랭킹 시스템 연동** 🏆

#### A. **NetworkManager.cs 수정**
```csharp
// 랭킹 제출 기능 추가
public void SubmitScore(
    string songName,
    int score,
    int maxCombo,
    int perfect,
    int great,
    int bad,
    int miss,
    Action<bool> callback
)
```

**기능**:
- 게임 종료 시 자동으로 Supabase `ranking` 테이블에 점수 제출
- 사용자 정보 (ID, 이름) 자동 포함
- 곡 이름, 점수, 콤보, 판정 통계 저장

#### B. **GameManager.cs 수정**
```csharp
// EndGame() 함수에서 자동 호출
private void SubmitScoreToRanking()
{
    // 로그인 확인
    if (!NetworkManager.Instance.IsLoggedIn)
    {
        Debug.Log("User not logged in. Skipping score submission.");
        return;
    }

    // 곡 이름 자동 감지
    string songName = sceneName == "Game_second" ? "Sodapop" : "Galaxias";

    // 점수 제출
    NetworkManager.Instance.SubmitScore(
        songName, score, maxCombo,
        perfectCount, greatCount, badCount, missCount,
        (success) => { /* 콜백 */ }
    );
}
```

**작동 방식**:
1. 게임 플레이 완료
2. `EndGame()` 호출
3. **자동으로 랭킹 제출** (로그인된 경우)
4. Supabase `ranking` 테이블에 저장
5. 웹 랭킹 페이지에서 실시간 확인 가능

---

## 🎯 랭킹 데이터 구조

```json
{
  "user_id": "uuid",
  "user_name": "닉네임",
  "song_name": "Galaxias" | "Sodapop",
  "score": 12345,
  "max_combo": 150,
  "perfect": 100,
  "great": 40,
  "bad": 5,
  "miss": 5,
  "created_at": "2026-01-23T10:00:00.000Z"
}
```

---

## 🔍 확인 방법

### Unity Console 로그:
```
[GameManager] Submitting score to ranking: Galaxias, Score: 12345, Combo: 150
[Network] Score submitted successfully!
[GameManager] ✅ Score submitted successfully!
```

### 웹 랭킹 페이지:
- http://localhost:5173/ranking
- 실시간으로 Unity 게임 점수 확인 가능

---

## 📝 수정된 파일

1. ✅ `Assets/scripts/SchoolLobbyManager.cs`
   - CLOSE 버튼 투명하게 변경

2. ✅ `Assets/scripts/NetworkManager.cs`
   - `SubmitScore()` 함수 추가
   - `PostRequest()` 함수 추가

3. ✅ `Assets/scripts/GameManager.cs`
   - `SubmitScoreToRanking()` 함수 추가
   - `EndGame()`에서 자동 호출

---

## 🎮 사용 방법

### 자동 랭킹 제출:
1. Unity 게임 실행
2. 로그인 (LOGIN 버튼 클릭)
3. 게임 플레이
4. 게임 종료 시 **자동으로 랭킹 제출** ✅

### 랭킹 확인:
1. 웹 브라우저에서 http://localhost:5173/ranking 접속
2. Unity 게임 점수가 실시간으로 표시됨

---

## ⚠️ 주의사항

**로그인 필수**:
- 랭킹 제출은 **로그인된 사용자만** 가능
- 비로그인 시 Console에 "User not logged in" 메시지 표시

**NetworkManager 필수**:
- NetworkManager가 씬에 없으면 자동 생성
- Supabase 연결 필요

---

**작성일**: 2026-01-23  
**기능**: SHOP 뒤 빨간 버튼 제거 + Unity 랭킹 시스템 연동

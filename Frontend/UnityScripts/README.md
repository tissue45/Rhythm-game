# Unity 리듬 게임 C# 스크립트

Unity 프로젝트에서 사용할 C# 스크립트들입니다.

## 📁 스크립트 파일 목록

### 1. GameManager.cs
**역할**: 게임 전체 흐름 관리
- 게임 상태 (시작/일시정지/종료)
- 점수 및 콤보 관리
- UI 업데이트
- 음악 재생 제어

**사용법**:
1. 빈 GameObject 생성 → 이름: `GameManager`
2. `GameManager.cs` 스크립트 추가
3. Inspector에서 UI 요소 연결

---

### 2. NoteSpawner.cs
**역할**: 노트 생성 및 타이밍 관리
- 비트맵 데이터 읽기
- 음악과 동기화하여 노트 생성
- 4개 레인 관리

**사용법**:
1. 빈 GameObject 생성 → 이름: `NoteSpawner`
2. `NoteSpawner.cs` 스크립트 추가
3. Note Prefab 및 Spawn Points 설정

---

### 3. Note.cs
**역할**: 개별 노트 동작
- 아래로 이동
- 판정 라인 체크
- 히트 판정 (Perfect/Great/Good/Miss)

**사용법**:
1. Cube 또는 Sphere로 노트 Prefab 생성
2. `Note.cs` 스크립트 추가
3. Collider 컴포넌트 추가 (Trigger 체크)

---

### 4. InputManager.cs
**역할**: 플레이어 입력 처리
- 키보드 입력 감지 (D, F, J, K)
- 노트 히트 체크
- 판정 영역 관리

**사용법**:
1. 빈 GameObject 생성 → 이름: `InputManager`
2. `InputManager.cs` 스크립트 추가
3. 각 레인의 Hit Zone 위치 설정

---

### 5. AvatarController.cs
**역할**: VRM 아바타 제어
- 레인 간 이동
3. Unity로 돌아가면 자동으로 컴파일됨

### 2단계: 씬 구성
다음 GameObject들을 Hierarchy에 생성:

```
Scene
├── GameManager (GameManager.cs)
├── NoteSpawner (NoteSpawner.cs)
├── InputManager (InputManager.cs)
├── VRM_Avatar (AvatarController.cs)
├── Main Camera
├── Directional Light
├── SpawnPoints (4개 Transform)
│   ├── Lane0_Spawn
│   ├── Lane1_Spawn
│   ├── Lane2_Spawn
│   └── Lane3_Spawn
├── HitZones (4개 Transform)
│   ├── Lane0_Hit
│   ├── Lane1_Hit
│   ├── Lane2_Hit
│   └── Lane3_Hit
├── LanePositions (4개 Transform)
│   ├── Lane0_Pos
│   ├── Lane1_Pos
│   ├── Lane2_Pos
│   └── Lane3_Pos
└── Canvas (UI)
    ├── ScoreText (TextMeshPro)
    ├── ComboText (TextMeshPro)
    └── GameOverPanel
```

### 3단계: Prefab 생성
1. **Note Prefab**:
   - Cube 생성 → Scale: (0.5, 0.5, 0.5)
   - `Note.cs` 스크립트 추가
   - Box Collider 추가 (Is Trigger 체크)
   - Prefab으로 저장

### 4단계: Inspector 설정
각 스크립트의 public 변수들을 Inspector에서 연결:

**GameManager**:
- Score Text → Canvas/ScoreText
- Combo Text → Canvas/ComboText
- Game Over Panel → Canvas/GameOverPanel
- Music Source → AudioSource 컴포넌트

**NoteSpawner**:
- Note Prefab → 생성한 Note Prefab
- Spawn Points → 4개의 SpawnPoint Transform
- Song BPM → 120 (음악에 맞게 조정)

**InputManager**:
- Hit Zones → 4개의 HitZone Transform

**AvatarController**:
- Animator → VRM 아바타의 Animator
- Blend Shape Proxy → VRM의 BlendShapeProxy
- Lane Positions → 4개의 LanePosition Transform

**LobbyManager**:
- Settings Panel → 설정 UI 패널 (선택)
- Game Scene Name → 게임 플레이 씬 이름 (기본값: "Game")
- Background Music → 배경 음악 AudioSource


---

## 🎵 다음 단계

1. **음악 파일 추가**: `Assets/Audio/` 폴더에 배경 음악 추가
2. **비트맵 생성**: 음악에 맞는 노트 타이밍 데이터 작성
3. **UI 디자인**: Canvas에 점수, 콤보 UI 꾸미기
4. **테스트**: Play 버튼으로 게임 테스트

Unity 설치가 완료되면 이 스크립트들을 사용해서 게임을 만들 수 있습니다!

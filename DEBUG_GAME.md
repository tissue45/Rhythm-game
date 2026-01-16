# Unity 게임 디버깅 가이드

## 확인 사항

### 1. 브라우저 개발자 도구에서 확인

1. `https://blueberrrrrry.github.io/rhythmgame/game` 접속
2. F12로 개발자 도구 열기
3. Network 탭에서 확인:
   - `game.loader.js` - 로드 성공?
   - `game.framework.js` - 로드 성공?
   - `game.data` - 로드 성공? (크기 확인)
   - `game.wasm` - 로드 성공?

### 2. 콘솔 오류 확인

- `Failed to initialize player` - 게임 초기화 실패
- `No GlobalGameManagers file was found` - Unity 빌드 파일 누락

### 3. 파일 경로 확인

현재 설정:
- iframe src: `/rhythmgame/game/index.html`
- game/index.html 내부: `buildUrl = "Build"` (상대 경로)
- 실제 경로: `/rhythmgame/game/Build/`

## 해결 방법

### 방법 1: GitHub Pages에서 직접 확인
게임 페이지에 직접 접속해서 파일들이 로드되는지 확인:
- `https://blueberrrrrry.github.io/rhythmgame/game/index.html`

### 방법 2: Render 배포 (동료 방식)
Render에 Unity 게임 파일 배포 후 URL 사용

### 방법 3: Unity 빌드 재확인
Unity에서 WebGL 빌드를 다시 수행하여 모든 파일이 포함되었는지 확인

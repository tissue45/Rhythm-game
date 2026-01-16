# Render 배포 가이드 - Unity 게임 파일

## 배포 구조

- **GitHub Pages**: 메인 웹사이트 (React 앱)
- **Render**: Unity 게임 파일 서빙

## Render 배포 방법

### 1. Render 계정 생성 및 서비스 생성

1. https://render.com 접속
2. "New +" → "Static Site" 선택
3. 설정:
   - **Name**: `rhythmgame-unity` (원하는 이름)
   - **Build Command**: (비워두기 - 정적 파일만)
   - **Publish Directory**: `Frontend/public/game`
   - **Environment**: Static

### 2. GitHub 저장소 연결

- GitHub 저장소 연결
- Branch: `main`
- Root Directory: `Frontend/public/game` (또는 전체 저장소 연결 후 Publish Directory 설정)

### 3. 배포 후 URL 확인

배포 완료 후 Render URL을 받습니다:
- 예: `https://rhythmgame-unity.onrender.com`

### 4. 환경 변수 설정

`Frontend/.env` 파일에 추가:
```
VITE_GAME_URL=https://your-render-url.onrender.com/game/index.html
```

또는 GitHub Actions에서 환경 변수로 설정

## 현재 설정

`GamePage.tsx`가 환경 변수 `VITE_GAME_URL`을 우선 사용하도록 설정되어 있습니다.
없으면 로컬 경로를 사용합니다.

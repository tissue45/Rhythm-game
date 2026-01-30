# 🎮 Unity WebGL 업데이트 가이드

## 📍 현재 배포 구조

- **Unity WebGL 빌드 위치**: `Frontend/public/game/Build/`
- **배포 플랫폼**: Render.com (GitHub 자동 연동)
- **배포 방식**: GitHub 푸시 시 자동 재배포

---

## 🔄 Unity WebGL 수정 후 업데이트 절차

### 1️⃣ Unity에서 WebGL 빌드 생성

1. Unity 에디터에서 프로젝트 열기
2. **File** → **Build Settings** (Ctrl+Shift+B)
3. **Platform**에서 **WebGL** 선택
4. **Player Settings** 클릭하여 설정 확인:
   - **Compression Format**: Brotli (권장) 또는 Gzip
   - **Data caching**: 활성화 (선택사항)
5. **Build** 버튼 클릭
6. 빌드 폴더 선택 (예: `C:\Users\user\Downloads\Rhythm_game (1)\Rhythm_game (2)\Build\WebGL`)

### 2️⃣ 빌드된 파일을 프로젝트에 복사

Unity가 빌드를 완료하면 다음 파일들이 생성됩니다:

```
Build/WebGL/
├── index.html
├── Build/
│   ├── game.loader.js
│   ├── game.framework.js
│   ├── game.wasm
│   ├── game.data (또는 game.data.partaa, game.data.partab 등)
│   └── ...
└── TemplateData/
    ├── style.css
    ├── favicon.ico
    └── ...
```

**복사해야 할 위치**:
```
Frontend/public/game/
├── index.html          ← Unity 빌드의 index.html 복사
├── Build/              ← Unity 빌드의 Build 폴더 전체 복사
│   ├── game.loader.js
│   ├── game.framework.js
│   ├── game.wasm
│   └── game.data.part*
└── TemplateData/       ← Unity 빌드의 TemplateData 폴더 전체 복사
    ├── style.css
    └── ...
```

### 3️⃣ 파일 복사 방법

#### 방법 A: 수동 복사 (Windows)
1. Unity 빌드 폴더 열기: `Build/WebGL/`
2. 다음 파일/폴더를 복사:
   - `index.html` → `Frontend/public/game/index.html`
   - `Build/` 폴더 전체 → `Frontend/public/game/Build/`
   - `TemplateData/` 폴더 전체 → `Frontend/public/game/TemplateData/`
3. 기존 파일 덮어쓰기 확인

#### 방법 B: PowerShell 스크립트 사용
프로젝트 루트에서 다음 명령 실행:

```powershell
# Unity 빌드 경로 설정 (본인의 빌드 경로로 변경)
$unityBuildPath = "C:\Users\user\Downloads\Rhythm_game (1)\Rhythm_game (2)\Build\WebGL"
$targetPath = "Frontend\public\game"

# Build 폴더 복사
Copy-Item -Path "$unityBuildPath\Build\*" -Destination "$targetPath\Build\" -Recurse -Force

# TemplateData 폴더 복사
Copy-Item -Path "$unityBuildPath\TemplateData\*" -Destination "$targetPath\TemplateData\" -Recurse -Force

# index.html 복사
Copy-Item -Path "$unityBuildPath\index.html" -Destination "$targetPath\index.html" -Force

Write-Host "✅ Unity WebGL 빌드 파일 복사 완료!" -ForegroundColor Green
```

### 4️⃣ GitHub에 커밋 및 푸시

```bash
# 변경사항 확인
git status

# 변경된 파일 추가
git add Frontend/public/game/

# 커밋
git commit -m "Update Unity WebGL build"

# GitHub에 푸시
git push origin main
```

### 5️⃣ Render.com 자동 배포 확인

1. GitHub에 푸시하면 Render.com이 자동으로 감지
2. Render.com 대시보드에서 배포 상태 확인:
   - **Dashboard** → 해당 서비스 클릭
   - **Events** 탭에서 배포 진행 상황 확인
3. 배포 완료까지 약 2-5분 소요
4. 배포 완료 후 웹사이트에서 게임 테스트

---

## ⚠️ 주의사항

### 1. 파일 크기 제한
- GitHub는 100MB 이상 파일을 거부합니다
- `game.data` 파일이 크면 Unity가 자동으로 분할합니다 (`game.data.partaa`, `game.data.partab` 등)
- `.gitignore`에서 이미 예외 처리되어 있으므로 걱정하지 마세요

### 2. 빌드 설정 확인
- **Compression Format**: Brotli 권장 (더 작은 파일 크기)
- **Development Build**: 배포 시에는 체크 해제
- **Script Debugging**: 배포 시에는 체크 해제

### 3. 캐시 문제
- 브라우저 캐시로 인해 이전 버전이 보일 수 있습니다
- **Ctrl + F5** (강력 새로고침) 또는 **Ctrl + Shift + R**로 캐시 무시하고 새로고침

### 4. 빌드 경로 확인
- Unity 빌드 설정에서 **Build Path**가 올바른지 확인
- 빌드 후 생성된 폴더 구조 확인

---

## 🔍 문제 해결

### 문제 1: 게임이 로드되지 않음
**원인**: 파일 경로 불일치
**해결**: 
- `Frontend/public/game/index.html`에서 `buildUrl` 확인
- `Build/` 폴더 내 파일명 확인 (예: `game.loader.js`, `game.framework.js`)

### 문제 2: Render.com에서 배포 실패
**원인**: 파일 크기 초과 또는 Git LFS 미설정
**해결**:
- 큰 파일은 Git LFS 사용
- `.gitignore` 확인하여 불필요한 파일 제외

### 문제 3: 변경사항이 반영되지 않음
**원인**: 브라우저 캐시 또는 배포 미완료
**해결**:
- 브라우저 캐시 삭제 (Ctrl + Shift + Delete)
- Render.com 대시보드에서 배포 완료 확인
- 강력 새로고침 (Ctrl + F5)

---

## 📝 체크리스트

업데이트 전:
- [ ] Unity에서 변경사항 저장
- [ ] 빌드 설정 확인 (Development Build 해제)

빌드 중:
- [ ] WebGL 플랫폼 선택
- [ ] 빌드 경로 확인
- [ ] 빌드 완료 대기

복사 중:
- [ ] `Build/` 폴더 복사
- [ ] `TemplateData/` 폴더 복사
- [ ] `index.html` 복사

배포 중:
- [ ] Git 상태 확인 (`git status`)
- [ ] 변경사항 커밋 (`git commit`)
- [ ] GitHub 푸시 (`git push`)
- [ ] Render.com 배포 상태 확인

배포 후:
- [ ] 웹사이트 접속 확인
- [ ] 게임 로드 테스트
- [ ] 변경사항 반영 확인

---

## 🚀 빠른 업데이트 명령어

전체 프로세스를 한 번에 실행하려면:

```powershell
# 1. Unity 빌드 경로 설정
$unityBuildPath = "C:\Users\user\Downloads\Rhythm_game (1)\Rhythm_game (2)\Build\WebGL"
$targetPath = "Frontend\public\game"

# 2. 파일 복사
Copy-Item -Path "$unityBuildPath\Build\*" -Destination "$targetPath\Build\" -Recurse -Force
Copy-Item -Path "$unityBuildPath\TemplateData\*" -Destination "$targetPath\TemplateData\" -Recurse -Force
Copy-Item -Path "$unityBuildPath\index.html" -Destination "$targetPath\index.html" -Force

# 3. Git 커밋 및 푸시
git add Frontend/public/game/
git commit -m "Update Unity WebGL build - $(Get-Date -Format 'yyyy-MM-dd HH:mm')"
git push origin main

Write-Host "✅ 업데이트 완료! Render.com에서 배포를 확인하세요." -ForegroundColor Green
```

---

**💡 팁**: Unity 빌드 경로를 환경 변수나 스크립트에 저장해두면 매번 입력할 필요가 없습니다!

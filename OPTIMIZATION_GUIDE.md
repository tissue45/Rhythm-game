# 🗜️ 구글 드라이브 업로드 최적화 가이드

## 📊 현재 상태
- **현재 용량**: 약 3GB
- **목표 용량**: 500MB 이하
- **삭제 가능 용량**: 약 2.5GB

---

## 🎯 최적화 전략

### ✅ 안전하게 삭제 가능한 폴더 (자동 재생성)

#### 1. **Unity Library 폴더** (1-2GB)
```
Library/
```
- **용도**: Unity 에디터 캐시 및 임포트 데이터
- **재생성**: Unity 프로젝트를 열면 자동으로 재생성
- **삭제 안전성**: ✅ 100% 안전

#### 2. **Unity Temp 폴더** (10-100MB)
```
Temp/
```
- **용도**: Unity 임시 파일
- **재생성**: Unity 실행 시 자동 생성
- **삭제 안전성**: ✅ 100% 안전

#### 3. **Unity Logs 폴더** (1-10MB)
```
Logs/
```
- **용도**: Unity 에디터 로그
- **재생성**: Unity 실행 시 자동 생성
- **삭제 안전성**: ✅ 100% 안전

#### 4. **Unity obj 폴더** (10-50MB)
```
obj/
```
- **용도**: C# 빌드 캐시
- **재생성**: Unity 빌드 시 자동 생성
- **삭제 안전성**: ✅ 100% 안전

#### 5. **Frontend node_modules** (200-500MB)
```
Frontend/node_modules/
```
- **용도**: npm 패키지
- **재생성**: `npm install` 명령으로 재생성
- **삭제 안전성**: ✅ 100% 안전

#### 6. **Frontend dist/build** (10-50MB)
```
Frontend/dist/
Frontend/build/
Frontend/.vite/
```
- **용도**: 빌드 결과물
- **재생성**: `npm run build` 명령으로 재생성
- **삭제 안전성**: ✅ 100% 안전

#### 7. **Unity UserSettings** (1-5MB)
```
UserSettings/
```
- **용도**: 개인 에디터 설정
- **재생성**: Unity 실행 시 기본값으로 생성
- **삭제 안전성**: ✅ 안전 (개인 설정만 초기화)

---

## 🚀 자동 최적화 스크립트

### PowerShell 스크립트 (Windows)

아래 명령을 실행하면 **안전하게 삭제 가능한 폴더들을 자동으로 제거**합니다:

```powershell
# 프로젝트 루트에서 실행
$projectPath = "c:\Users\user\Downloads\Rhythm_game (2)"

# 삭제할 폴더 목록
$foldersToDelete = @(
    "Library",
    "Temp",
    "Logs",
    "obj",
    "UserSettings",
    "Frontend\node_modules",
    "Frontend\dist",
    "Frontend\build",
    "Frontend\.vite"
)

# 삭제 전 용량 확인
Write-Host "=== 삭제 전 용량 분석 ===" -ForegroundColor Cyan
foreach ($folder in $foldersToDelete) {
    $fullPath = Join-Path $projectPath $folder
    if (Test-Path $fullPath) {
        $size = (Get-ChildItem $fullPath -Recurse -File -ErrorAction SilentlyContinue | 
                 Measure-Object -Property Length -Sum).Sum / 1MB
        Write-Host "$folder : $([math]::Round($size, 2)) MB" -ForegroundColor Yellow
    }
}

# 사용자 확인
Write-Host "`n위 폴더들을 삭제하시겠습니까? (Y/N): " -ForegroundColor Green -NoNewline
$confirm = Read-Host

if ($confirm -eq 'Y' -or $confirm -eq 'y') {
    Write-Host "`n=== 폴더 삭제 중... ===" -ForegroundColor Cyan
    foreach ($folder in $foldersToDelete) {
        $fullPath = Join-Path $projectPath $folder
        if (Test-Path $fullPath) {
            Remove-Item -Path $fullPath -Recurse -Force -ErrorAction SilentlyContinue
            Write-Host "✓ 삭제 완료: $folder" -ForegroundColor Green
        } else {
            Write-Host "- 존재하지 않음: $folder" -ForegroundColor Gray
        }
    }
    Write-Host "`n✅ 최적화 완료!" -ForegroundColor Green
} else {
    Write-Host "`n❌ 취소되었습니다." -ForegroundColor Red
}
```

---

## 📋 수동 최적화 체크리스트

구글 드라이브 업로드 전 아래 폴더들을 수동으로 삭제하세요:

### Unity 폴더
- [ ] `Library/` 폴더 삭제
- [ ] `Temp/` 폴더 삭제
- [ ] `Logs/` 폴더 삭제
- [ ] `obj/` 폴더 삭제
- [ ] `UserSettings/` 폴더 삭제 (선택사항)

### Frontend 폴더
- [ ] `Frontend/node_modules/` 폴더 삭제
- [ ] `Frontend/dist/` 폴더 삭제
- [ ] `Frontend/build/` 폴더 삭제
- [ ] `Frontend/.vite/` 폴더 삭제

### 기타
- [ ] `.vs/` 폴더 삭제 (Visual Studio 캐시)
- [ ] `.vscode/` 폴더 삭제 (VS Code 설정, 선택사항)
- [ ] `.idea/` 폴더 삭제 (Rider 설정, 선택사항)

---

## 🔄 다운로드 후 복원 방법

### 1. Unity 프로젝트 복원
```bash
1. Unity Hub에서 프로젝트 열기
2. Unity가 자동으로 Library 폴더 재생성 (5-10분 소요)
3. 완료!
```

### 2. Frontend 복원
```bash
cd Frontend
npm install
# node_modules 폴더 자동 생성 (1-2분 소요)
```

---

## 📊 예상 용량 절감

| 항목 | 삭제 전 | 삭제 후 | 절감 |
|------|---------|---------|------|
| Library | 1-2GB | 0MB | -1.5GB |
| node_modules | 200-500MB | 0MB | -350MB |
| Temp/Logs/obj | 50-100MB | 0MB | -75MB |
| **총계** | **~3GB** | **~500MB** | **~2.5GB** |

---

## ⚠️ 절대 삭제하면 안 되는 것들

### ❌ 삭제 금지
- `Assets/` - 게임 에셋 (모델, 텍스처, 스크립트 등)
- `ProjectSettings/` - Unity 프로젝트 설정
- `Packages/` - Unity 패키지 매니페스트
- `Frontend/src/` - 소스 코드
- `Frontend/public/` - 정적 파일
- `Frontend/package.json` - npm 의존성 정의
- `unity_auth_server.js` - 백엔드 서버
- `README.md` - 프로젝트 문서
- `.gitignore` - Git 설정

---

## 🎯 최종 확인

최적화 후 프로젝트 구조:
```
Rhythm_game (2)/
├── Assets/              ✅ 유지 (필수)
├── Packages/            ✅ 유지 (필수)
├── ProjectSettings/     ✅ 유지 (필수)
├── Frontend/
│   ├── src/            ✅ 유지 (필수)
│   ├── public/         ✅ 유지 (필수)
│   ├── package.json    ✅ 유지 (필수)
│   ├── node_modules/   ❌ 삭제됨 (재생성 가능)
│   └── dist/           ❌ 삭제됨 (재생성 가능)
├── Library/            ❌ 삭제됨 (재생성 가능)
├── Temp/               ❌ 삭제됨 (재생성 가능)
├── Logs/               ❌ 삭제됨 (재생성 가능)
├── obj/                ❌ 삭제됨 (재생성 가능)
├── unity_auth_server.js ✅ 유지 (필수)
├── README.md           ✅ 유지 (필수)
└── .gitignore          ✅ 유지 (권장)
```

---

## 💡 추가 최적화 팁

### 1. 대용량 에셋 압축
- `.psd` 파일 → `.png`로 변환 (Photoshop 원본 제거)
- `.wav` 파일 → `.mp3`로 변환 (압축)
- 사용하지 않는 에셋 삭제

### 2. Git LFS 사용 (선택사항)
대용량 파일(모델, 텍스처)을 Git LFS로 관리하면 저장소 크기 절감

### 3. 압축 업로드
- 7-Zip으로 압축 후 업로드 (추가 30-50% 절감)
- 압축 형식: `.7z` 또는 `.zip`

---

**최적화 후 예상 용량: 500MB 이하** ✅

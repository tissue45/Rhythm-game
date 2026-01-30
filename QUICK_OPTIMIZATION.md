# 🗜️ 구글 드라이브 업로드 최적화 - 간단 가이드

## ⚡ 빠른 실행

### 방법 1: 수동 삭제 (추천)

아래 폴더들을 **Windows 탐색기에서 직접 삭제**하세요:

```
✅ 안전하게 삭제 가능 (Unity/npm이 자동 재생성)

📁 Library          (1-2GB) ← 가장 큰 용량!
📁 Temp             (10-100MB)
📁 Logs             (1-10MB)
📁 obj              (10-50MB)
📁 UserSettings     (1-5MB)
📁 Frontend\node_modules  (200-500MB)
📁 Frontend\dist    (10-50MB)
📁 Frontend\build   (있다면)
📁 Frontend\.vite   (있다면)
📁 .vs              (있다면, 10-50MB)
```

### 방법 2: PowerShell 명령어

```powershell
# 프로젝트 폴더에서 실행
cd "c:\Users\user\Downloads\Rhythm_game (2)"

# 한 번에 삭제
Remove-Item -Path "Library" -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item -Path "Temp" -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item -Path "Logs" -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item -Path "obj" -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item -Path "UserSettings" -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item -Path "Frontend\node_modules" -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item -Path "Frontend\dist" -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item -Path ".vs" -Recurse -Force -ErrorAction SilentlyContinue

Write-Host "최적화 완료!" -ForegroundColor Green
```

---

## 📊 예상 결과

| 항목 | 삭제 전 | 삭제 후 | 절감 |
|------|---------|---------|------|
| 전체 용량 | ~3GB | ~500MB | **-2.5GB** |

---

## 🔄 복원 방법

### Unity 프로젝트
```
1. Unity Hub에서 프로젝트 열기
2. Unity가 자동으로 Library 폴더 재생성 (5-10분)
3. 완료!
```

### Frontend
```bash
cd Frontend
npm install
```

---

## ❌ 절대 삭제 금지!

```
❌ Assets/          - 게임 에셋 (모델, 스크립트 등)
❌ ProjectSettings/ - Unity 프로젝트 설정
❌ Packages/        - Unity 패키지
❌ Frontend/src/    - 소스 코드
❌ Frontend/package.json - npm 설정
```

---

## ✅ 체크리스트

구글 드라이브 업로드 전:

- [ ] `Library/` 폴더 삭제
- [ ] `Temp/` 폴더 삭제
- [ ] `Logs/` 폴더 삭제
- [ ] `obj/` 폴더 삭제
- [ ] `Frontend/node_modules/` 폴더 삭제
- [ ] `Frontend/dist/` 폴더 삭제
- [ ] 용량 확인 (500MB 이하 목표)
- [ ] 구글 드라이브 업로드

---

**최적화 후 바로 업로드하세요! 🚀**

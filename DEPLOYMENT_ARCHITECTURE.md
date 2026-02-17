# 🚀 배포 구조 및 계획

## ⚡ 빠른 배포 가이드 (GitHub Pages)

### 3단계로 끝내는 배포

```bash
# 1. 빌드
cd Frontend
npm run build

# 2. 푸시
cd ..
git add .
git commit -m "Deploy"
git push origin main

# 3. GitHub Pages 활성화
# GitHub 저장소 → Settings → Pages → Source: main → Save
```

**배포 URL**: `https://[사용자명].github.io/[저장소명]`

---

## 📊 배포 아키텍처

### ✅ 정확한 배포 구조

```
┌─────────────────────────────────────────────────┐
│              사용자                              │
│  ┌──────────┐         ┌──────────┐             │
│  │ Unity    │         │ 폰 (QR)  │             │
│  │ (로컬PC) │         │          │             │
│  └────┬─────┘         └────┬─────┘             │
└───────┼──────────────────────┼──────────────────┘
        │                      │
        │ HTTP                 │ WebSocket
        ▼                      ▼
┌─────────────────────────────────────────────────┐
│           클라우드 서비스 (무료)                  │
├─────────────────────────────────────────────────┤
│                                                 │
│  ┌──────────────────────┐  ┌─────────────────┐ │
│  │ React 프론트엔드      │  │ Socket.IO       │ │
│  │ GitHub Pages (무료)  │  │ Render (무료)   │ │
│  │ ✅ 이미 렌더 있음     │  │ ✅ 이미 배포됨  │ │
│  └──────────┬───────────┘  └─────────────────┘ │
│             │                                   │
│             │ HTTP API                          │
│             ▼                                   │
│  ┌──────────────────────┐                      │
│  │ Unity 인증 서버       │                      │
│  │ Render (무료)        │                      │
│  │ ✅ 이미 배포됨        │                      │
│  └──────────┬───────────┘                      │
│             │                                   │
└─────────────┼───────────────────────────────────┘
              │ Database API
              ▼
┌─────────────────────────────────────────────────┐
│              Supabase (무료)                    │
│  - users (사용자 정보)                           │
│  - ranking (랭킹 데이터)                         │
│  ✅ 이미 설정됨                                  │
└─────────────────────────────────────────────────┘
```

---

## 🎯 배포 계획

### 1. **프론트엔드 (React 웹)**

#### 옵션 A: GitHub Pages (추천 - 무료)
```
장점:
✅ 완전 무료
✅ GitHub 저장소와 자동 연동
✅ HTTPS 자동 제공
✅ 빠른 배포

단점:
⚠️ 정적 사이트만 가능 (React는 OK)

배포 URL 예시:
https://tissue45.github.io/Rhythm-game
```

### 2. **백엔드 (Unity 인증 서버)**

#### ✅ Render.com (이미 배포됨)
```
현재 상태: 이미 올려둔 것 사용
URL: https://rhythm-game-website.onrender.com (또는 다른 URL)

필요 작업:
1. unity_auth_server.js 코드 확인
2. 환경 변수 설정 확인
3. 정상 작동 테스트
```

### 3. **Socket.IO 서버 (모바일 컨트롤러)**

#### ✅ Render.com (이미 배포됨)
```
현재 상태: 이미 배포되어 작동 중
URL: https://rhythm-game-website.onrender.com

필요 작업: 없음 (그대로 사용)
```

### 4. **데이터베이스**

#### ✅ Supabase (이미 설정됨)
```
현재 상태: 이미 설정되어 작동 중
URL: Supabase 클라우드

필요 작업: 없음 (그대로 사용)
```

---

## 📋 배포 순서

### ✅ 이미 완료된 것
- [x] Supabase 데이터베이스 설정
- [x] Socket.IO 서버 Render 배포
- [x] Unity 인증 서버 Render 배포 (확인 필요)
- [x] 환경 변수 시스템 구축
- [x] 코드 최적화 (localhost → 환경 변수)

### 🔜 남은 작업

#### 1. **Render 서버 확인** (5분)
```bash
# 현재 배포된 Render 서버 URL 확인
# 브라우저에서 접속:
https://rhythm-game-website.onrender.com/api/health

# 응답 확인:
{ "status": "OK" }
```

#### 2. **GitHub Pages 배포** (10분)

##### 단계별 가이드

**1단계: Frontend 빌드**
```bash
cd Frontend
npm install
npm run build
```
→ `Frontend/dist` 폴더에 빌드 결과물 생성됨

**2단계: GitHub 저장소에 푸시**
```bash
# 프로젝트 루트로 이동
cd ..

# 변경사항 커밋 및 푸시
git add .
git commit -m "Deploy to GitHub Pages"
git push origin main
```

**3단계: GitHub Pages 활성화**
1. GitHub 저장소 페이지 접속
2. **Settings** 탭 클릭
3. 왼쪽 메뉴에서 **Pages** 클릭
4. **Source** 선택:
   - **Branch**: `main` (또는 `gh-pages`)
   - **Folder**: `/Frontend/dist` 또는 `/docs` (빌드 결과물 위치)
5. **Save** 클릭

**4단계: 배포 URL 확인**
- 배포 완료까지 1-2분 소요
- 배포 URL: `https://[사용자명].github.io/[저장소명]`
- 예시: `https://tissue45.github.io/Rhythm-game`

**5단계: 환경 변수 설정 (필요시)**
- GitHub Pages는 환경 변수를 직접 설정할 수 없음
- `.env.production` 파일에 환경 변수 설정 후 빌드
- 또는 GitHub Secrets를 사용하여 GitHub Actions로 빌드

#### 3. **환경 변수 업데이트** (5분)
```
GitHub Pages 환경 변수:
- VITE_API_URL=https://[your-render-app].onrender.com
- VITE_SUPABASE_URL=https://[your-supabase-url].supabase.co
- VITE_SUPABASE_ANON_KEY=[your-anon-key]
- VITE_TOSS_CLIENT_KEY=test_ck_...
```

#### 4. **Unity 설정 업데이트** (2분)
```csharp
// LoginManager.cs
private string backendApiUrl = "https://rhythm-game-website.onrender.com";
```

#### 5. **전체 테스트** (10분)
```
- [ ] 웹 접속 테스트
- [ ] 로그인 기능 테스트
- [ ] 결제 시스템 테스트
- [ ] 모바일 컨트롤러 테스트
- [ ] 랭킹 시스템 테스트
```

---

## 🎯 최종 배포 URL (예상)

| 서비스 | URL |
|--------|-----|
| **프론트엔드** | https://tissue45.github.io/Rhythm-game |
| **백엔드 API** | https://rhythm-game-website.onrender.com |
| **Socket.IO** | https://rhythm-game-website.onrender.com |
| **데이터베이스** | Supabase (클라우드) |

---

## 💡 왜 이 구조인가?

### GitHub Pages vs Vercel

**GitHub Pages 선택 이유:**
1. ✅ **완전 무료** - Vercel도 무료지만 제한 있음
2. ✅ **GitHub 저장소와 통합** - 이미 GitHub 사용 중
3. ✅ **간단한 배포** - git push만으로 자동 배포
4. ✅ **정적 사이트 호스팅** - React 빌드 결과물 호스팅에 최적

**GitHub Pages를 선택한 이유:**
- GitHub 저장소와 바로 연동 가능
- 추가 계정 생성 불필요
- 설정이 간단함

### Render.com

**이미 사용 중:**
- ✅ 백엔드 서버 (unity_auth_server.js)
- ✅ Socket.IO 서버
- ✅ 무료 플랜으로 충분

---

## 🚀 다음 단계

1. **Render 서버 URL 확인**
   - 현재 배포된 Render 서버의 정확한 URL 확인
   - Health check 테스트

2. **GitHub Pages 배포**
   - Frontend 빌드
   - GitHub Pages 설정
   - 환경 변수 설정

3. **Unity 설정 업데이트**
   - 배포된 URL로 변경

4. **전체 테스트**
   - 모든 기능 검증

---

**총 소요 시간: 약 30분** ⏱️

**비용: 완전 무료** 💰

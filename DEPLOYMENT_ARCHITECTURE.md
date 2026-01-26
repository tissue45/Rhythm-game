# 🚀 배포 구조 및 계획

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
https://sohee9010.github.io/rhythm-game-website
```

#### 옵션 B: Vercel (대안 - 무료)
```
장점:
✅ 무료 플랜 제공
✅ 자동 배포
✅ 환경 변수 관리 쉬움

배포 URL 예시:
https://stepup-rhythm.vercel.app
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
URL: https://vraxfilfgphvluiokwlc.supabase.co

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
```bash
# 1. GitHub 저장소 생성 (이미 있다면 스킵)
# 2. Frontend 빌드
cd Frontend
npm run build

# 3. GitHub Pages 설정
# Settings → Pages → Source: GitHub Actions

# 4. 배포 스크립트 실행 (자동)
```

#### 3. **환경 변수 업데이트** (5분)
```
GitHub Pages 환경 변수:
- VITE_API_URL=https://rhythm-game-website.onrender.com
- VITE_SUPABASE_URL=https://vraxfilfgphvluiokwlc.supabase.co
- VITE_SUPABASE_ANON_KEY=sb_publishable_...
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
| **프론트엔드** | https://sohee9010.github.io/rhythm-game-website |
| **백엔드 API** | https://rhythm-game-website.onrender.com |
| **Socket.IO** | https://rhythm-game-website.onrender.com |
| **데이터베이스** | https://vraxfilfgphvluiokwlc.supabase.co |

---

## 💡 왜 이 구조인가?

### GitHub Pages vs Vercel

**GitHub Pages 선택 이유:**
1. ✅ **완전 무료** - Vercel도 무료지만 제한 있음
2. ✅ **GitHub 저장소와 통합** - 이미 GitHub 사용 중
3. ✅ **간단한 배포** - git push만으로 자동 배포
4. ✅ **정적 사이트 호스팅** - React 빌드 결과물 호스팅에 최적

**Vercel을 사용하지 않는 이유:**
- GitHub Pages로 충분함
- 추가 계정 생성 불필요
- 설정이 더 간단함

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

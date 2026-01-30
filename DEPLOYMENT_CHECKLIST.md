# 🚀 배포 환경 체크리스트

## ❌ 현재 상태: **배포 불가능**

### 🔴 문제점

#### 1. **하드코딩된 localhost URL**
```typescript
// ❌ 배포 시 작동 안 함
'http://localhost:3001/api/unity-login'
'http://localhost:3001/api/payment/confirm'
```

**영향받는 파일:**
- `Frontend/src/pages/UnityLoginPage.tsx` (43줄)
- `Frontend/src/pages/PaymentSuccessPage.tsx` (23줄)

#### 2. **Socket.IO 서버 URL**
```typescript
// ✅ 현재는 괜찮음 (상대 경로 사용)
const getSocketUrl = () => {
    return undefined; // window.location 자동 사용
};
```

#### 3. **Unity 인증 브릿지 서버**
- 현재: `http://localhost:3001`
- 필요: Render.com 또는 다른 서버에 배포 필요

---

## ✅ 배포 준비 작업

### 1. **환경 변수 설정**

#### `.env` 파일 생성
```env
# 로컬 개발
VITE_API_URL=http://localhost:3001

# 배포 환경 (예시)
# VITE_API_URL=https://your-backend.onrender.com
```

#### 코드 수정
```typescript
// ✅ 환경 변수 사용
const API_URL = import.meta.env.VITE_API_URL || 'http://localhost:3001'

fetch(`${API_URL}/api/unity-login`, { ... })
```

---

### 2. **모바일 컨트롤러 배포 시나리오**

#### 현재 구조
```
폰 (QR 접속) → React 웹 (localhost:5173)
                    ↓ Socket.IO
              Render 서버 (이미 배포됨)
                    ↓ WebSocket
              Unity 게임 (로컬 PC)
```

#### 배포 후 구조
```
폰 (QR 접속) → React 웹 (Vercel/Netlify)
                    ↓ Socket.IO
              Render 서버 (배포됨)
                    ↓ WebSocket
              Unity 게임 (로컬 PC)
```

**✅ 작동 가능!** 
- Socket.IO 서버가 이미 Render에 배포되어 있음
- 폰 → 웹 → Socket.IO 서버 → Unity (로컬) 연결 가능

---

### 3. **Unity 인증 브릿지 서버 배포**

#### 현재 문제
```javascript
// unity_auth_server.js
app.listen(3001, () => {
    console.log('Server running on http://localhost:3001');
});
```

#### 해결 방법
**Option A: Render.com에 배포**
```javascript
const PORT = process.env.PORT || 3001;
app.listen(PORT, () => {
    console.log(`Server running on port ${PORT}`);
});
```

**Option B: Vercel Serverless Functions**
- `api/unity-login.ts` 형태로 변환

---

## 📝 배포 전 체크리스트

### Frontend (React)
- [ ] 환경 변수 설정 (`.env.production`)
- [ ] `UnityLoginPage.tsx` - localhost 제거
- [ ] `PaymentSuccessPage.tsx` - localhost 제거
- [ ] Supabase URL/Key 환경 변수화
- [ ] 토스페이먼츠 키 환경 변수화

### Backend
- [ ] `unity_auth_server.js` Render 배포
- [ ] 환경 변수 설정 (PORT, SUPABASE_URL 등)
- [ ] CORS 설정 (배포된 프론트엔드 URL 허용)

### Unity
- [ ] NetworkManager - Socket.IO 서버 URL 확인
- [ ] WebGL 빌드 (선택사항)

---

## 🎯 배포 시나리오별 가이드

### Scenario 1: **Unity Desktop + React 웹 배포**
```
✅ 가능
- React 웹: Vercel/Netlify
- Unity 인증 서버: Render
- Socket.IO 서버: Render (이미 배포됨)
- Unity 게임: 로컬 PC에서 실행
```

**폰 컨트롤러:**
- ✅ 작동함
- 폰에서 QR 접속 → 배포된 웹 → Socket.IO 서버 → 로컬 Unity

---

### Scenario 2: **Unity WebGL + React 웹 배포**
```
✅ 가능
- React 웹: Vercel/Netlify
- Unity WebGL: 같은 도메인에 배포
- Unity 인증 서버: Render
- Socket.IO 서버: Render
```

**폰 컨트롤러:**
- ✅ 작동함
- 폰에서 QR 접속 → 배포된 웹 → Socket.IO 서버 → WebGL Unity

---

## 🔧 즉시 수정 필요한 파일

### 1. `Frontend/.env.production` (생성)
```env
VITE_API_URL=https://your-backend.onrender.com
VITE_SUPABASE_URL=https://zyqbuuovliissozugjfq.supabase.co
VITE_SUPABASE_ANON_KEY=eyJhbGci...
VITE_TOSS_CLIENT_KEY=test_ck_...
```

### 2. `Frontend/src/pages/UnityLoginPage.tsx`
```typescript
// 수정 전
const response = await fetch('http://localhost:3001/api/unity-login', { ... })

// 수정 후
const API_URL = import.meta.env.VITE_API_URL || 'http://localhost:3001'
const response = await fetch(`${API_URL}/api/unity-login`, { ... })
```

### 3. `Frontend/src/pages/PaymentSuccessPage.tsx`
```typescript
// 수정 전
const confirmResponse = await fetch('http://localhost:3001/api/payment/confirm', { ... })

// 수정 후
const API_URL = import.meta.env.VITE_API_URL || 'http://localhost:3001'
const confirmResponse = await fetch(`${API_URL}/api/payment/confirm`, { ... })
```

### 4. `unity_auth_server.js`
```javascript
// 수정 전
app.listen(3001, () => { ... })

// 수정 후
const PORT = process.env.PORT || 3001;
app.listen(PORT, '0.0.0.0', () => {
    console.log(`Server running on port ${PORT}`);
});
```

---

## 📊 배포 후 예상 URL

### 프론트엔드
- Vercel: `https://stepup-rhythm.vercel.app`
- Netlify: `https://stepup-rhythm.netlify.app`

### 백엔드
- Render: `https://stepup-auth-server.onrender.com`

### Socket.IO (이미 배포됨)
- Render: `https://rhythm-game-website.onrender.com`

---

## ✅ 결론

### 현재 상태
**❌ 배포 불가능** - localhost URL 하드코딩

### 수정 후
**✅ 배포 가능** - 환경 변수 사용

### 폰 컨트롤러
**✅ 배포 후에도 작동** - Socket.IO 서버가 이미 Render에 배포되어 있음

---

**다음 단계**: 위의 4개 파일을 수정하면 배포 준비 완료!

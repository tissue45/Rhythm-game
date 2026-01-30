# 🚀 Step Up - Unity 로그인 연동 빠른 시작 가이드

## ⚡ 5분 안에 로그인 연동 테스트하기

---

## 📌 필수 요구사항

- Node.js 설치됨
- Unity 에디터 열려있음
- 포트 3000에 백엔드 서버 실행 중 (당신의 데이터베이스 서버)

---

## 🎯 1단계: 모든 서버 시작 (30초)

### 방법 1: 자동 시작 (추천) ⭐
```bash
START_ALL_SERVERS.bat 더블클릭
```

### 방법 2: 수동 시작
**터미널 1: Auth Server**
```bash
npm install
npm start
```

**터미널 2: Frontend**
```bash
cd Frontend
npm install
npm run dev
```

---

## ✅ 2단계: 서버 확인 (10초)

브라우저에서 다음 URL들을 확인하세요:

1. **Auth Server 상태 확인**
   - URL: http://localhost:3001/api/health
   - 예상 응답: `{"status":"running","hasLoginData":false}`

2. **Frontend 확인**
   - URL: http://localhost:5173
   - 홈페이지가 정상적으로 보여야 함

3. **Unity 로그인 페이지 확인**
   - URL: http://localhost:5173/unity-login?unity=true
   - "UNITY LOGIN" 페이지가 보여야 함

---

## 🎮 3단계: Unity 설정 (1분)

1. **Unity 에디터 열기**
   - Main 씬을 엽니다

2. **버튼 설정 실행**
   - 상단 메뉴: **Rhythm Game → Fix All Buttons (Complete)**
   - Console에서 성공 메시지 확인

3. **Play 모드 시작**
   - Play 버튼 클릭

---

## 🧪 4단계: 로그인 테스트 (1분)

### Unity에서 로그인하기

1. **Unity 게임 화면에서 LOGIN 버튼 클릭**
   - 왼쪽 상단의 파란색 LOGIN 버튼

2. **웹 브라우저가 자동으로 열림**
   - Unity Login 페이지가 표시됨

3. **로그인 정보 입력**
   - 당신의 데이터베이스에 등록된 이메일/비밀번호 입력
   - LOGIN 버튼 클릭

4. **성공 메시지 확인**
   - "사용자이름님, 로그인 성공!" 메시지 표시
   - "이 창을 닫고 게임으로 돌아가세요" 안내

5. **브라우저 창 닫기**
   - 자동으로 닫히거나 수동으로 닫아도 됨

6. **Unity로 돌아가기**
   - Unity 게임 화면으로 전환

---

## ✨ 5단계: 로그인 성공 확인 (30초)

### Unity Console 확인
```
[LoginManager] Started polling Unity auth server
[LoginManager] Polling server: http://localhost:3001/api/unity-login
[LoginManager] Received login data from server: 홍길동
[LoginManager] Login successful: 홍길동 (hong@example.com)
[LoginManager] Stopped polling
```

### UI 변경 확인
- ✅ LOGIN 버튼이 **녹색**으로 변경됨
- ✅ 버튼 텍스트가 **"LOGOUT"**으로 변경됨

### 로그아웃 테스트
- LOGOUT 버튼 클릭
- 버튼이 다시 파란색 LOGIN으로 돌아감

---

## 🔄 작동 원리 (간단 설명)

```
Unity (LOGIN 클릭)
    ↓
웹 브라우저 열림 (localhost:5173/unity-login)
    ↓
사용자가 로그인 (포트 3000 백엔드로 인증)
    ↓
로그인 성공 → Auth Server로 정보 전송 (포트 3001)
    ↓
Unity가 Auth Server를 2초마다 폴링
    ↓
로그인 정보 발견 → Unity에 전달
    ↓
Unity PlayerPrefs에 저장
    ↓
LOGIN 버튼이 LOGOUT으로 변경 ✅
```

---

## 🔧 자주 발생하는 문제

### ❌ "Started polling Unity auth server" 메시지가 안 보임
**해결**: Unity 에디터에서 Play 모드를 다시 시작하세요

### ❌ 로그인 후 Unity에 전달 안 됨
**원인**: Auth Server가 실행 안 됨
**해결**: http://localhost:3001/api/health 확인 후 서버 재시작

### ❌ 웹 로그인이 실패함
**원인**: 백엔드 서버(포트 3000)가 실행 안 됨
**해결**: 백엔드 서버를 먼저 시작하세요

### ❌ CORS 에러
**해결**: Auth Server를 재시작하세요
```bash
node unity_auth_server.js
```

---

## 🎉 성공!

이제 Unity 게임에서 웹 로그인이 완벽하게 연동되었습니다!

### 다음 단계
- 로그인 필요한 기능에 `LoginManager.Instance.isLoggedIn` 체크 추가
- 사용자 정보 사용: `LoginManager.Instance.currentUserName`
- 게임 데이터 저장 시 사용자 이메일 활용

### 더 자세한 정보
- 전체 가이드: [LOGIN_SETUP_README.md](LOGIN_SETUP_README.md)
- 서버 코드: [unity_auth_server.js](unity_auth_server.js)
- Unity 코드: [Assets/scripts/LoginManager.cs](Assets/scripts/LoginManager.cs)

---

**문제가 계속되면 Unity Console과 브라우저 Console(F12)을 확인하세요!**

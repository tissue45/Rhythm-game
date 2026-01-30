# Step Up - 인게임 로그인 연동 가이드

## 🎮 Unity 인게임 LOGIN 버튼을 웹 로그인 페이지와 연동했습니다!

---

## 📋 설치 및 실행 방법

### 🚀 빠른 시작 (권장)

**모든 서버 한 번에 시작**
```bash
START_ALL_SERVERS.bat 더블클릭
```

이 명령어는 다음 서버들을 자동으로 시작합니다:
- Unity Auth Bridge Server (포트 3001)
- Frontend Dev Server (포트 5173)

### 📝 개별 서버 시작 방법

**1. Unity Auth Bridge Server (필수)**
```bash
START_UNITY_AUTH_SERVER.bat 더블클릭
```

**2. Frontend Dev Server (필수)**
```bash
cd Frontend
npm run dev
```

### 2단계: 서버 확인

브라우저에서 다음 URL을 확인하세요:
- **Unity 로그인**: http://localhost:5173/unity-login?unity=true
- **일반 홈페이지**: http://localhost:5173
- **Auth 서버 상태**: http://localhost:3001/api/health

### 3단계: Unity에서 버튼 설정

Unity 에디터에서:
1. **Rhythm Game → Fix All Buttons (Complete)** 실행
2. Main 씬 실행
3. **LOGIN 버튼** 클릭

---

## 🔧 작동 방식

### Unity → 웹
1. **LOGIN 버튼 클릭** → 기본 웹 브라우저로 로그인 페이지 열림
2. URL: `http://localhost:5173/unity-login?unity=true`
3. Unity는 백그라운드에서 Auth Server를 2초마다 폴링 시작

### 웹 → Auth Server → Unity
1. **로그인 성공** → 웹 페이지가 Unity Auth Server로 로그인 정보 전송
   - 엔드포인트: `POST http://localhost:3001/api/unity-login`
   - 데이터: `{ name: "사용자이름", email: "이메일" }`

2. **Auth Server** → 로그인 정보를 임시 저장 (5분 동안 유효)

3. **Unity 폴링** → Unity가 주기적으로 Auth Server에 로그인 데이터 요청
   - 엔드포인트: `GET http://localhost:3001/api/unity-login`
   - 간격: 2초마다

4. **로그인 데이터 발견** → Unity가 데이터를 받아서 PlayerPrefs에 저장
   - Auth Server는 데이터를 Unity에 전달한 후 즉시 삭제 (보안)

### Unity 내부
1. **PlayerPrefs에 저장**:
   - `userName`: 로그인한 사용자 이름
   - `userEmail`: 로그인한 사용자 이메일

2. **로그인 상태 표시**:
   - LOGIN 버튼 색상 변경 (파란색 → 녹색)
   - 버튼 텍스트 변경 (LOGIN → LOGOUT)

3. **자동 폴링**:
   - 로그인 안 된 상태면 자동으로 서버 폴링 시작
   - 로그인 성공 시 폴링 자동 중지

---

## 📁 추가/수정된 파일들

### Unity Scripts
- `Assets/scripts/LoginManager.cs` - 로그인 상태 관리 + 서버 폴링
- `Assets/scripts/Editor/FixAllButtons.cs` - 버튼 자동 설정

### 웹 페이지
- `Frontend/src/pages/UnityLoginPage.tsx` - Unity 전용 로그인 페이지
- `Frontend/src/App.tsx` - `/unity-login` 라우트 추가됨

### 서버
- `unity_auth_server.js` - Unity 인증 브리지 서버 (포트 3001)
- `package.json` - 서버 의존성 관리

### 실행 스크립트
- `START_ALL_SERVERS.bat` - 모든 서버 한 번에 시작 ⭐ 권장
- `START_UNITY_AUTH_SERVER.bat` - Auth 서버만 시작
- `SETUP_AND_START.bat` - Frontend 서버 시작

---

## 🎯 로그인 테스트 방법

### 1. 모든 서버 시작
```bash
START_ALL_SERVERS.bat 더블클릭
```

다음 서버들이 시작됩니다:
- Unity Auth Server (포트 3001) - 별도 창
- Frontend Dev Server (포트 5173) - 별도 창

### 2. 서버 확인
브라우저에서 http://localhost:3001/api/health 접속
- 응답: `{"status":"running", "hasLoginData":false}`

### 3. Unity 실행
1. Unity에서 Main 씬 열기
2. **Rhythm Game → Fix All Buttons (Complete)** 실행
3. Play 버튼 클릭

### 4. 로그인 테스트
1. Unity에서 **LOGIN 버튼 클릭**
2. 웹 브라우저에서 로그인 페이지가 자동으로 열림
3. 이메일/비밀번호 입력 후 로그인
4. 로그인 성공 메시지 확인
5. 브라우저 창은 자동으로 닫힘 (3초 후)
6. Unity로 돌아가기

### 5. 로그인 확인 (Unity Console)
Unity Console에서 다음과 같은 로그가 표시됩니다:
```
[LoginManager] Started polling Unity auth server
[LoginManager] Polling server: http://localhost:3001/api/unity-login
[LoginManager] Received login data from server: 사용자이름
[LoginManager] Login successful: 사용자이름 (이메일)
[LoginManager] Stopped polling
```

### 6. UI 변경 확인
- LOGIN 버튼이 **녹색**으로 변경됨
- 버튼 텍스트가 **"LOGOUT"**으로 변경됨
- LOGOUT 버튼 클릭 시 다시 파란색 LOGIN으로 돌아감

---

## 🔐 로그인 정보 관리

### Unity에서 로그인 상태 확인
```csharp
LoginManager loginManager = LoginManager.Instance;

if (loginManager.isLoggedIn)
{
    Debug.Log($"User: {loginManager.currentUserName}");
    Debug.Log($"Email: {loginManager.currentUserEmail}");
}
```

### 로그아웃
```csharp
LoginManager.Instance.Logout();
```

---

## 🌐 배포 시 설정

### 로컬 개발 (현재)
- **로그인 페이지**: `http://localhost:5173/unity-login?unity=true`
- **Auth Server**: `http://localhost:3001/api/unity-login`

### 프로덕션 배포
Unity `LoginManager` 컴포넌트에서 URL 변경:
```csharp
public string loginPageUrl = "https://your-domain.com/unity-login?unity=true";
public string unityAuthServerUrl = "https://your-domain.com/api/unity-login";
```

Auth Server를 프로덕션 환경에 배포:
- Node.js 서버 호스팅 (Heroku, AWS, Azure 등)
- 환경 변수로 포트 설정
- HTTPS 적용 필수

**주의**: 프로덕션에서는 Auth Server의 보안을 강화하세요:
- API 키 인증 추가
- Rate limiting 적용
- 로그인 데이터 암호화

---

## 🐛 문제 해결

### ❌ Auth Server가 시작되지 않는 경우
```bash
# 루트 디렉토리에서
npm install express cors
node unity_auth_server.js
```

### ❌ Frontend 서버가 실행되지 않는 경우
```bash
cd Frontend
npm install
npm run dev
```

### ❌ LOGIN 버튼이 작동하지 않는 경우
1. Unity 에디터에서 **Rhythm Game → Fix All Buttons (Complete)** 재실행
2. LoginManager GameObject가 존재하는지 확인
3. Console에서 에러 메시지 확인

### ❌ 로그인 후 정보가 Unity에 전달되지 않는 경우

**1단계: Auth Server 확인**
- http://localhost:3001/api/health 접속
- 서버가 실행 중인지 확인

**2단계: 브라우저 Console 확인 (F12)**
```
[UnityLogin] Login data sent to Unity auth server
```
위 메시지가 보이는지 확인

**3단계: Unity Console 확인**
```
[LoginManager] Started polling Unity auth server
[LoginManager] Received login data from server: 사용자이름
```
위 메시지가 보이는지 확인

**4단계: PlayerPrefs 확인**
```csharp
Debug.Log(PlayerPrefs.GetString("userName"));
Debug.Log(PlayerPrefs.GetString("userEmail"));
```

### ❌ 폴링이 작동하지 않는 경우
Unity Console에서 확인:
```
[LoginManager] Started polling Unity auth server
```
이 메시지가 안 보이면:
1. LoginManager의 `enableAutoPolling`이 true인지 확인
2. Unity 에디터에서 Play 모드 재시작

### ❌ CORS 에러가 발생하는 경우
Auth Server를 재시작하세요:
```bash
node unity_auth_server.js
```

---

## 📝 추가 기능

### 자동 로그인 상태 복원
게임을 다시 시작해도 로그인 상태 유지됨 (PlayerPrefs 사용)

### 로그인 필요한 기능 추가
```csharp
if (!LoginManager.Instance.isLoggedIn)
{
    Debug.LogWarning("로그인이 필요합니다!");
    LoginManager.Instance.OpenLoginPage();
    return;
}

// 로그인 후에만 실행되는 코드
```

---

## 🎉 완료!

이제 Unity 게임에서 LOGIN 버튼을 누르면 웹 로그인 페이지가 열리고,
로그인 성공 시 Unity로 정보가 전달됩니다!

문제가 있으면 Unity Console과 브라우저 Console을 확인하세요.

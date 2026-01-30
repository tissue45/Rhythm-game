# 🎮 Step Up - Cross-Platform Rhythm Game

Unity(PC), React(Web), 모바일 컨트롤러가 실시간으로 연동되는 **크로스플랫폼 하이브리드 리듬 게임**입니다.
학교를 배경으로 한 3D 리듬 액션을 PC와 모바일을 오가며 즐겨보세요.

---

## 📋 목차
- [주요 기능](#-주요-기능)
- [기술 스택](#-기술-스택)
- [실행 방법](#-실행-방법)
- [테스트 계정](#-테스트-계정)
- [배포 환경](#-배포-환경)
- [프로젝트 구조](#-프로젝트-구조)

---

## � 주요 기능

### 1. **Unity 게임 (Desktop/WebGL)**
- 🏫 **3D 학교 로비** - MMD 캐릭터 (미나, 민우) 상호작용
- 🎵 **리듬 게임** - Galaxias, Sodapop 2곡 지원
- 📱 **모바일 컨트롤러** - QR 코드로 스마트폰 연결
- 🏆 **실시간 랭킹** - Supabase 연동 자동 점수 제출
- 💎 **코인 시스템** - 게임 내 재화 관리
- 🛒 **인게임 샵** - 아이템 구매 (토스페이먼츠 연동)

### 2. **React 웹 애플리케이션**
- 🔐 **로그인/회원가입** - Supabase 인증
- 🏆 **랭킹 페이지** - 실시간 점수 확인
- 💳 **결제 시스템** - 토스페이먼츠 통합
- 📱 **모바일 컨트롤러** - WebSocket 실시간 통신
- 👤 **마이페이지** - 프로필 및 코인 관리

---

## 🛠 기술 스택

### Frontend
- **React** 18.3.1 + TypeScript
- **Vite** - 빌드 도구
- **React Router** - 라우팅
- **Tailwind CSS** - 스타일링
- **Socket.IO Client** - 실시간 통신

### Backend
- **Node.js** + Express - Unity 인증 브릿지 서버
- **Supabase** - 데이터베이스 + 인증
- **Socket.IO** - WebSocket 서버 (모바일 컨트롤러)

### Game Engine
- **Unity** 2020.3.x+
- **MMD4Mecanim** - MMD 모델 지원
- **TextMeshPro** - UI 텍스트

### Payment
- **토스페이먼츠** - 결제 시스템

---

## 🚀 실행 방법

### 1. **백엔드 서버 실행**

```bash
# Unity 인증 브릿지 서버
node unity_auth_server.js
# → http://localhost:3001
```

### 2. **React 웹 실행**

```bash
cd Frontend
npm install
npm run dev
# → http://localhost:5173
```

### 3. **Unity 게임 실행**

```
1. Unity Hub에서 프로젝트 열기
2. Assets/Scenes/SchoolLobby 씬 실행
3. Play 버튼 클릭
```

---

## 🔑 테스트 계정

### 웹 로그인
```
이메일: test@example.com
비밀번호: password123
```

### 결제 테스트 (토스페이먼츠)
```
카드번호: 아무 숫자나 16자리
유효기간: 미래 날짜
CVC: 아무 숫자 3자리
비밀번호: 아무 숫자 2자리
```
**주의**: 현재 테스트 모드로 실제 결제는 진행되지 않습니다.

---

## 🌐 배포 환경

### 현재 상태: **배포 준비 완료 ✅**

#### 실행 중인 서비스
| 서비스 | 로컬 URL | 배포 URL (예정) | 상태 |
|--------|----------|----------------|------|
| React 웹 | http://localhost:5173 | https://stepup-rhythm.vercel.app | ✅ 배포 준비 완료 |
| Unity 인증 서버 | http://localhost:3001 | https://stepup-auth-server.onrender.com | ✅ 배포 준비 완료 |
| Socket.IO 서버 | - | https://rhythm-game-website.onrender.com | ✅ 이미 배포됨 |
| Supabase DB | - | https://vraxfilfgphvluiokwlc.supabase.co | ✅ 클라우드 |

#### 환경 변수 설정

**로컬 개발 (`.env`)**
```env
VITE_SUPABASE_URL=https://vraxfilfgphvluiokwlc.supabase.co
VITE_SUPABASE_ANON_KEY=sb_publishable_pxwOmNrEqrVR6mVQwGQ-0Q_U72jXSYQ
VITE_API_URL=http://localhost:3001
VITE_TOSS_CLIENT_KEY=test_ck_D5GePWvyJnrK0W0k6q8gLzN97Eoq
```

**배포 환경 (`.env.production` / Vercel 환경 변수)**
```env
VITE_SUPABASE_URL=https://vraxfilfgphvluiokwlc.supabase.co
VITE_SUPABASE_ANON_KEY=sb_publishable_pxwOmNrEqrVR6mVQwGQ-0Q_U72jXSYQ
VITE_API_URL=https://stepup-auth-server.onrender.com
VITE_TOSS_CLIENT_KEY=test_ck_D5GePWvyJnrK0W0k6q8gLzN97Eoq
```

#### 배포 가이드
자세한 배포 방법은 [`DEPLOYMENT_GUIDE.md`](./DEPLOYMENT_GUIDE.md) 참고

#### 배포 체크리스트
- [x] **환경 변수 설정** - `.env`, `.env.production` 생성
- [x] **코드 수정** - localhost URL → 환경 변수 사용
- [x] **백엔드 준비** - PORT 환경 변수, 0.0.0.0 리스닝
- [ ] **Render 배포** - Unity 인증 서버
- [ ] **Vercel 배포** - React 웹 애플리케이션
- [ ] **Unity 설정** - 배포된 URL로 변경
- [ ] **테스트** - 모든 기능 검증

---

## 📁 프로젝트 구조

```
Rhythm_game/
├── Frontend/                    # React 웹 애플리케이션
│   ├── src/
│   │   ├── pages/              # 페이지 컴포넌트
│   │   │   ├── HomePage.tsx
│   │   │   ├── LoginPage.tsx
│   │   │   ├── RankingPage.tsx
│   │   │   ├── PaymentPage.tsx
│   │   │   └── GamePage.tsx
│   │   ├── components/         # 재사용 컴포넌트
│   │   ├── context/            # React Context
│   │   └── services/           # API 서비스
│   └── public/
│       └── game/               # Unity WebGL 빌드 (배포 시)
│
├── Assets/                      # Unity 게임 에셋
│   ├── Scenes/
│   │   ├── SchoolLobby.unity   # 메인 로비
│   │   ├── Game_first.unity    # Galaxias
│   │   └── Game_second.unity   # Sodapop
│   ├── scripts/
│   │   ├── GameManager.cs      # 게임 로직 관리
│   │   ├── NetworkManager.cs   # Supabase 연동
│   │   ├── LoginManager.cs     # 로그인 시스템
│   │   └── SchoolLobbyManager.cs
│   └── Editor/
│       └── FixAllButtons.cs    # UI 자동 생성
│
├── unity_auth_server.js         # Unity ↔ React 인증 브릿지
└── README.md
```

---

## 🎮 상세 게임 플레이 가이드

### 🕹️ 조작 방법 (Controls)

#### **1. 마우스 & 터치 플레이 (Mouse & Touch)**
PC에서는 마우스 클릭, 모바일/태블릿에서는 화면 터치로 플레이합니다.
- **입력 방식**: 판정선(Hit Zone)에 노트가 도달했을 때 해당 라인의 버튼을 **클릭(Click)** 또는 **터치(Touch)** 합니다.

<br>

#### **2. 모바일 컨트롤러 (Mobile Controller)**
스마트폰을 흔들어 플레이하는 신개념 모션 컨트롤 방식입니다.
- **연결 방법**:
    1. 로비 화면 우측 하단의 **QR 코드**를 스마트폰 카메라로 스캔합니다.
    2. 연결된 웹 컨트롤러 페이지에서 **"START"** 버튼을 누릅니다.
- **조작 방식**:
    - **Shake**: 스마트폰을 가볍게 흔들면 가장 가까운 노트가 자동으로 처리됩니다 (**Auto-Aim 기능**).
    - **Touch**: 컨트롤러 화면의 4개 레인 버튼을 직접 터치하여 플레이할 수도 있습니다.

---

### 📏 게임 규칙 (Rules)

#### **판정 시스템 (Judgment)**
노트가 판정선(Judge Line)에 얼마나 정확히 들어왔는지를 기준으로 4단계로 나뉩니다.

| 등급 | 판정 범위 (오차) | 점수 | 설명 |
|:---:|:---:|:---:|:---|
| **PERFECT** | `±0.1s` 이내 | **+100점** | 완벽한 타이밍! 화려한 이펙트가 발생합니다. |
| **GREAT** | `±0.3s` 이내 | **+75점** | 준수한 타이밍입니다. |
| **GOOD** | `±0.5s` 이내 | **+50점** | 조금 빗나갔지만 콤보는 유지됩니다. |
| **MISS** | `±0.5s` 초과 | **0점** | 콤보가 끊기고 **체력이 감소**합니다. |

#### **콤보 & 체력 (Combo & HP)**
- **Combo Bonus**: 콤보가 누적될수록 추가 점수가 부여되지 않습니다(현재 버전 기준, 기본 점수 합산).
- **HP Gauge**:
    - 게임 시작 시 HP 100%로 시작합니다.
    - **MISS** 발생 시 HP가 **10% 감소**합니다.
    - HP가 0이 되면 **Game Over** 상태가 되며 결과 화면으로 이동합니다.

---

### 🎵 수록곡 (Track List)

| 곡 제목 | 장르 | BPM | 난이도 | 특징 |
|:---:|:---:|:---:|:---:|:---|
| **GALAXIAS!** | Electro Pop | 158 | ★★★☆☆ | 빠른 비트와 신나는 전자음이 특징인 메인 테마곡입니다. |
| **Sodapop** | Bubblegum Pop | 128 | ★★☆☆☆ | 통통 튀는 리듬의 경쾌하고 귀여운 곡입니다. |

---

### 🌊 게임 흐름 (Game Flow)

1.  **Lobby (학교 로비)**
    - 캐릭터(미나, 민우)가 대기하고 있습니다.
    - **LOGIN**: 웹 서버와 연동하여 로그인하고 내 정보를 불러옵니다.
    - **SHOP**: 코인을 사용하여 아이템을 구매하거나 충전합니다.
    - **RANKING**: 전 세계 유저들의 실시간 랭킹을 확인합니다.
    - **OPTION**: 음량 등 게임 설정을 변경합니다.

2.  **Song Select (곡 선택)**
    - **GAME START** 버튼을 누르면 곡 선택 화면으로 진입합니다.
    - 앨범 아트를 좌우로 넘겨 곡을 선택합니다.

3.  **Gameplay (리듬 게임)**
    - 음악에 맞춰 내려오는 노트를 정확한 타이밍에 처리합니다.
    - 좌측 상단에 실시간 점수(Score)와 체력(HP) 게이지가 표시됩니다.

4.  **Result (결과 화면)**
    - 게임이 끝나면 최종 점수, 판정별 개수(Perfect/Great/Bad/Miss), 최대 콤보를 확인합니다.
    - 획득한 점수는 **자동으로 랭킹 서버에 전송**되어 기록됩니다.
    - 획득 점수에 비례하여 **코인(Gold)** 보상을 받습니다.

---

## 💳 결제 시스템

### 코인 패키지
| 코인 | 가격 | 보너스 |
|------|------|--------|
| 100 | ₩1,000 | - |
| 500 | ₩5,000 | +50 (⭐ 인기) |
| 1,000 | ₩10,000 | +150 |
| 5,000 | ₩50,000 | +1,000 |

### 결제 수단
- � 신용/체크카드
- 💬 카카오페이
- 🏦 계좌이체

### 결제 흐름
```
Unity 게임 SHOP → 웹 브라우저 자동 열림 → 결제 완료 → Supabase DB 코인 추가
```

---

## 🏆 랭킹 시스템

### 자동 제출
- 게임 종료 시 **자동으로 Supabase에 점수 제출**
- 로그인된 사용자만 가능

### 저장 데이터
```json
{
  "user_name": "닉네임",
  "song_name": "Galaxias | Sodapop",
  "score": 12345,
  "max_combo": 150,
  "perfect": 100,
  "great": 40,
  "bad": 5,
  "miss": 5,
  "created_at": "2026-01-23T10:00:00Z"
}
```

---

## 🔧 개발 환경 설정

### 필수 요구사항
- **Node.js** 18.x+
- **Unity** 2020.3.x+
- **npm** 또는 **yarn**

### 환경 변수 (.env)
```env
# Supabase
VITE_SUPABASE_URL=https://zyqbuuovliissozugjfq.supabase.co
VITE_SUPABASE_ANON_KEY=eyJhbGci...

# 토스페이먼츠
VITE_TOSS_CLIENT_KEY=test_ck_...
```

---

## 📝 주요 업데이트 (2026-01-23)

### ✅ 완료된 기능
- [x] Unity 게임 로그인 시스템
- [x] 웹 ↔ Unity 인증 연동
- [x] 토스페이먼츠 결제 시스템
- [x] 실시간 랭킹 시스템
- [x] 모바일 컨트롤러 (WebSocket)
- [x] 코인 시스템
- [x] 인게임 샵

### 🎨 UI/UX 개선
- [x] LOGIN 버튼 밝은 시안색 통일
- [x] 리듬게임 스타일 사용자 정보 패널
- [x] 결제 페이지 디자인
- [x] 랭킹 페이지 디자인

---

## 🐛 알려진 이슈

- Unity WebGL 빌드 시 MMD4Mecanim 라이선스 경고 (무시 가능)
- 한글 폰트 일부 깨짐 (TextMeshPro 폰트 에셋 필요)

---

## 📞 문의

프로젝트 관련 문의사항은 이슈로 등록해주세요.

---

**Last Updated**: 2026-01-23  
**Version**: 1.2.0  
**License**: MIT

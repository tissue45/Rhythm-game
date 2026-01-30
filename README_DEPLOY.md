# 리듬 게임 배포 가이드

## 1. 빌드 전 최종 확인 사항
- **컴파일 에러**: 모두 해결되었습니다. (플레이 모드 진입 가능)
- **서버 URL 설정 (중요)**: 
  - `SchoolLobbyManager` 스크립트의 `deployedFrontendUrl` 변수를 확인하세요.
  - **Vercel 사용자**: `https://stepup-rhythm.vercel.app` (기본값)
  - **GitHub Pages 사용자**: 본인의 깃허브 페이지 주소 (예: `https://아이디.github.io/레포지토리명`)로 **반드시 변경**해야 합니다.
- **로컬 개발 모드**: `useLocalDevelopment` 체크박스가 **해제(False)** 되어 있어야 합니다.

## 2. 기능 점검 (배포 환경)
- **랭킹 시스템**:
  - `NetworkManager.cs`를 통해 Supabase와 통신합니다.
- **결제 시스템 (토스 페이먼츠)**:
  - 상점(SHOP) -> **'코인 충전하기'** 버튼 클릭.
  - 설정한 URL/payment 페이지로 이동합니다.
- **모바일 컨트롤러**:
  - 플레이 모드 선택 -> **'Mobile'** 버튼 클릭.
  - QR 코드가 설정한 URL/controller 페이지로 연결됩니다.

## 3. GitHub Pages 배포 시 주의사항
- 리액트(React) 등으로 만든 페이지(`/payment`, `/controller`)가 포함된 경우, GitHub Pages에서는 새로고침 시 **404 에러**가 발생할 수 있습니다.
- 이를 방지하려면 프론트엔드 코드에서 `HashRouter`를 사용하거나, `404.html` 트릭을 사용하여 라우팅 문제를 해결해야 합니다.

## 4. 빌드 방법
1. **유니티 빌드**:
   - `File > Build Settings` -> `WebGL` 선택 -> `Build` 버튼 클릭.
2. **업로드**:
   - 빌드된 결과물(`index.html`, `Build` 폴더 등)을 GitHub 레포지토리에 푸시(Push)하고, Settings > Pages에서 배포 소스를 설정합니다.

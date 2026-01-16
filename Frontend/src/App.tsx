import React, { lazy, Suspense } from 'react'
import { BrowserRouter as Router, Routes, Route, useLocation } from 'react-router-dom'
import Header from './components/Header'
import Footer from './components/Footer'
import ScrollToTop from './components/ScrollToTop'
import Toast from './components/Toast'
import HomePage from './pages/HomePage'
import LoginPage from './pages/LoginPage'
import SignupPage from './pages/SignupPage'
import FindIdPage from './pages/FindIdPage'
import ForgotPasswordPage from './pages/ForgotPasswordPage'
import ResetPasswordPage from './pages/ResetPasswordPage'
import RankingPage from './pages/RankingPage'
import MyPage from './pages/MyPage'
import InquiryPage from './pages/InquiryPage'
import InquiryHistoryPage from './pages/InquiryHistoryPage'
import InquiryDetailPage from './pages/InquiryDetailPage'
import GamePage from './pages/GamePage'

import { UserProvider } from './context/UserContext'

// 관리자 앱을 lazy loading으로 로드
const AdminApp = lazy(() => import('./admin/App'))

// 헤더를 숨길 페이지들을 정의 (관리자 페이지 포함)
const hideHeaderPaths = ['/login', '/signup', '/find-id', '/forgot-password', '/reset-password']

// 관리자 페이지인지 확인하는 함수
const isAdminPath = (pathname: string) => pathname.startsWith('/admin')

// 조건부 헤더 렌더링을 위한 컴포넌트
const ConditionalHeader: React.FC = () => {
  const location = useLocation()
  const shouldHideHeader = hideHeaderPaths.includes(location.pathname) || isAdminPath(location.pathname)

  return shouldHideHeader ? null : <Header />
}

// 조건부 푸터 렌더링을 위한 컴포넌트
const ConditionalFooter: React.FC = () => {
  const location = useLocation()
  const shouldHideFooter = isAdminPath(location.pathname)

  return shouldHideFooter ? null : <Footer />
}

const AppContent: React.FC = () => {
  // Toast state management could be moved to a global context if needed, 
  // for now keeping it simple or removing if not used by remaining components.
  // Assuming Toast was used by WishlistContext, we might need a new UIContext or similar if we want global toasts.
  // For now, removing the hook usage as WishlistContext is gone.

  return (
    <Router basename={import.meta.env.BASE_URL}>
      <ScrollToTop />
      <div className="w-full min-h-screen m-0 p-0 bg-black text-white"> {/* Default dark theme */}
        <ConditionalHeader />
        <main className="w-full">
          <Routes>
            <Route path="/" element={<HomePage />} />
            <Route path="/login" element={<LoginPage />} />
            <Route path="/signup" element={<SignupPage />} />
            <Route path="/find-id" element={<FindIdPage />} />
            <Route path="/forgot-password" element={<ForgotPasswordPage />} />
            <Route path="/reset-password" element={<ResetPasswordPage />} />
            <Route path="/mypage" element={<MyPage />} />
            <Route path="/ranking" element={<RankingPage />} />

            {/* 문의 관련 라우트 유지 */}
            <Route path="/inquiry" element={<InquiryPage />} />
            <Route path="/inquiry-history" element={<InquiryHistoryPage />} />
            <Route path="/inquiry/:id" element={<InquiryDetailPage />} />

            {/* 게임 실행 페이지 */}
            <Route path="/game" element={<GamePage />} />

            {/* 관리자 라우트 유지 */}
            <Route path="/admin/*" element={
              <Suspense fallback={
                <div style={{
                  display: 'flex',
                  justifyContent: 'center',
                  alignItems: 'center',
                  height: '100vh',
                  fontSize: '18px',
                  color: 'white'
                }}>
                  관리자 페이지 로딩 중...
                </div>
              }>
                <AdminApp />
              </Suspense>
            } />
          </Routes>
        </main>
        <ConditionalFooter />
      </div>
    </Router>
  )
}

function App() {
  return (
    <UserProvider>
      <AppContent />
    </UserProvider>
  )
}

export default App
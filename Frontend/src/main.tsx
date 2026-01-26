import React from 'react'
import ReactDOM from 'react-dom/client'
import App from './App.tsx'
import './index.css'

// 디버그 로그 - main.tsx 로드 확인
console.log('=== main.tsx 로드됨 ===');
console.log('현재 URL:', window.location.href);
console.log('현재 경로:', window.location.pathname);
console.log('BASE_URL:', import.meta.env.BASE_URL);
console.log('VITE 환경 변수:', {
  VITE_SUPABASE_URL: import.meta.env.VITE_SUPABASE_URL ? '설정됨' : '없음',
  VITE_API_URL: import.meta.env.VITE_API_URL ? '설정됨' : '없음',
});

// Global Error Handler for Mobile Debugging
window.onerror = function (msg, source, lineno, colno, error) {
  console.error('=== 에러 발생 ===', { msg, source, lineno, colno, error });
  const root = document.getElementById('root');
  if (root) {
    root.innerHTML += `<div style="color:red; background:black; padding:20px; z-index:9999; position:fixed; top:0; left:0; width:100%; height:100%;">
      <h1>CRASH DETECTED</h1>
      <p>${msg}</p>
      <p>${source}:${lineno}</p>
      <pre>${error?.stack || 'No stack trace'}</pre>
    </div>`;
  }
};

class ErrorBoundary extends React.Component<{ children: React.ReactNode }, { hasError: boolean, error: any }> {
  constructor(props: any) {
    super(props);
    this.state = { hasError: false, error: null };
  }
  static getDerivedStateFromError(error: any) {
    return { hasError: true, error };
  }
  render() {
    if (this.state.hasError) {
      return (
        <div style={{ color: 'red', padding: '20px', background: 'black', minHeight: '100vh' }}>
          <h1>Something went wrong.</h1>
          <pre>{this.state.error?.toString()}</pre>
        </div>
      );
    }
    return this.props.children;
  }
}

ReactDOM.createRoot(document.getElementById('root')!).render(
  <React.StrictMode>
    <ErrorBoundary>
      <App />
    </ErrorBoundary>
  </React.StrictMode>,
)
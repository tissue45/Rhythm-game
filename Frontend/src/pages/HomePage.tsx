import React from 'react'
import { Link } from 'react-router-dom'

const HomePage: React.FC = () => {
  return (
    <div className="min-h-screen bg-black text-white font-neon selection:bg-purple-500 selection:text-white">

      {/* 
        HERO SECTION - NUCLEAR BLOCK LAYOUT 
        - Removed Flexbox completely from container
        - Using standard padding (pt-[220px])
        - Z-index 30 for content
      */}
      <div className="relative w-full min-h-screen overflow-hidden block pt-[220px]">

        {/* Spacer for safety removed as padding handles it */}

        {/* Background Elements (Low Z-index) */}
        <div className="absolute inset-0 z-0 pointer-events-none">
          <div className="absolute top-[10%] left-[10%] w-[300px] h-[300px] bg-purple-900/40 rounded-full blur-[80px]"></div>
          <div className="absolute bottom-[20%] right-[10%] w-[400px] h-[400px] bg-blue-900/30 rounded-full blur-[100px]"></div>
        </div>

        {/* Content Container (High Z-index, centered via Margin) */}
        <div className="relative z-10 text-center max-w-5xl mx-auto">

          <p className="text-2xl md:text-3xl text-gray-300 mb-8 font-light tracking-wider animate-float">
            비트를 느껴보세요
          </p>

          <h1 className="text-7xl md:text-9xl font-black text-white mb-12 tracking-tighter leading-none" style={{ textShadow: '0 0 30px rgba(168, 85, 247, 0.5)' }}>
            <span className="text-purple-400">PLAY</span><br />
            THE RHYTHM
          </h1>

          <div className="flex flex-col md:flex-row gap-8 justify-center items-center mt-12">

            <Link
              to="/game"
              className="inline-block px-14 py-6 bg-white text-black text-2xl font-bold rounded-full hover:bg-purple-500 hover:text-white transition-all shadow-[0_0_20px_rgba(255,255,255,0.3)]"
            >
              게임 시작
            </Link>

            <Link
              to="/controller"
              className="md:hidden inline-block px-10 py-5 border border-gray-600 bg-gray-900 text-gray-300 rounded-full font-medium"
            >
              📱 컨트롤러 연결
            </Link>

          </div>
        </div>

        {/* Scroll Hint (Absolute Bottom) */}
        <div className="absolute bottom-10 left-1/2 transform -translate-x-1/2 flex flex-col items-center opacity-70 z-30">
          <span className="mb-2 text-sm uppercase tracking-widest text-purple-400">Scroll</span>
          <svg className="w-6 h-6 text-white animate-bounce" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 14l-7 7m0 0l-7-7m7 7V3" /></svg>
        </div>

      </div>

      {/* Features Section */}
      <section className="py-32 relative z-20 bg-black/90">
        <div className="max-w-7xl mx-auto px-6">
          <div className="text-center mb-24">
            <h2 className="text-4xl md:text-5xl font-bold mb-4 text-white">게임 특징</h2>
            <div className="w-24 h-1 bg-purple-600 mx-auto"></div>
          </div>

          <div className="grid grid-cols-1 md:grid-cols-3 gap-8">
            <div className="p-8 bg-white/5 rounded-2xl border border-white/10">
              <div className="text-6xl mb-6">📱</div>
              <h3 className="text-2xl font-bold mb-4 text-purple-300">별도 장비 불필요</h3>
              <p className="text-lg text-gray-400"><span className="text-cyan-400 font-semibold">스마트폰</span> 하나면 충분합니다.</p>
            </div>

            <div className="p-8 bg-white/5 rounded-2xl border border-white/10">
              <div className="text-6xl mb-6">🏃</div>
              <h3 className="text-2xl font-bold mb-4 text-pink-300">실시간 모션 인식</h3>
              <p className="text-lg text-gray-400">지연 없는 <span className="text-pink-400 font-semibold">실시간 동기화</span>.</p>
            </div>

            <div className="p-8 bg-white/5 rounded-2xl border border-white/10">
              <div className="text-6xl mb-6">🏆</div>
              <h3 className="text-2xl font-bold mb-4 text-cyan-300">리듬 액션 & 랭킹</h3>
              <p className="text-lg text-gray-400"><span className="text-cyan-400 font-semibold">글로벌 랭킹</span>에 도전하세요.</p>
            </div>
          </div>
        </div>
      </section>

      {/* Footer */}
      <footer className="py-8 bg-neutral-900 border-t border-purple-900/30 text-center">
        <p className="text-gray-500 text-sm">Version: <span className="text-purple-400">v1.1.0</span> | Updated: 2026-01-21</p>
      </footer>

    </div>
  )
}

export default HomePage
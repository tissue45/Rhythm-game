import React from 'react'
import { Link } from 'react-router-dom'

const HomePage: React.FC = () => {
  // Parallax ref removed to fix scroll issues

  return (
    <div className="min-h-screen bg-black text-white overflow-hidden font-neon selection:bg-purple-500 selection:text-white">

      {/* Hero Section */}
      <section className="relative h-screen flex items-center justify-center overflow-hidden">
        {/* Background Particles/Gradients */}
        <div className="absolute inset-0 z-0 pointer-events-none">
          <div className="absolute top-[-20%] left-[-10%] w-[500px] h-[500px] bg-purple-600/30 rounded-full blur-[100px] animate-pulse-glow"></div>
          <div className="absolute bottom-[-20%] right-[-10%] w-[600px] h-[600px] bg-blue-600/20 rounded-full blur-[120px] animate-pulse-glow" style={{ animationDelay: '1s' }}></div>
          <div className="absolute top-1/2 left-1/2 transform -translate-x-1/2 -translate-y-1/2 w-full h-full bg-[radial-gradient(ellipse_at_center,_var(--tw-gradient-stops))] from-transparent via-black/40 to-black"></div>
        </div>

        <div className="relative z-10 text-center px-4 flex flex-col items-center">

          {/* Logo removed as requested */}

          <p className="text-2xl md:text-3xl text-gray-300 mb-12 max-w-2xl mx-auto font-light tracking-wider">
            비트를 느껴보세요 <br />
            <span className="text-purple-400 font-bold mt-2 block neon-text text-4xl md:text-5xl">
              PLAY THE RHYTHM
            </span>
          </p>

          <div className="flex flex-col md:flex-row gap-8 justify-center items-center">

            <Link
              to="/game"
              className="group relative inline-block focus:outline-none"
            >
              <div className="absolute inset-0 bg-gradient-to-r from-purple-600 to-blue-600 rounded-full blur opacity-75 group-hover:opacity-100 transition duration-1000 group-hover:duration-200 animate-pulse-glow"></div>
              <div className="relative px-12 py-5 bg-black ring-1 ring-gray-600/50 rounded-full leading-none flex items-center">
                <span className="space-x-2 text-gray-100 group-hover:text-purple-300 transition-colors duration-200">
                  <span className="text-3xl font-bold tracking-widest">게임 시작</span>
                </span>
                <svg className="w-8 h-8 ml-4 text-purple-400 animate-pulse" fill="currentColor" viewBox="0 0 20 20">
                  <path fillRule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zM9.555 7.168A1 1 0 008 8v4a1 1 0 001.555.832l3-2a1 1 0 000-1.664l-3-2z" clipRule="evenodd" />
                </svg>
              </div>
            </Link>

          </div>
        </div>
      </section>

      {/* Version Footer */}
      <footer className="py-6 bg-black/90 border-t border-purple-500/20">
        <div className="max-w-7xl mx-auto px-6 text-center">
          <p className="text-gray-500 text-sm">
            Version: <span className="text-purple-400 font-mono">v1.1.0</span>
            {' '} | {' '}
            Build: <span className="text-cyan-400 font-mono">6859ebf</span>
            {' '} | {' '}
            Updated: <span className="text-gray-400">2026-01-13</span>
          </p>
          <p className="text-gray-600 text-xs mt-2">
            🎵 Audio Separation Fix - Galaxias & Sodapop No Longer Mix
          </p>
        </div>
      </footer>
    </div>
  )
}

export default HomePage
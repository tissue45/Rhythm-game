import React, { useEffect, useRef } from 'react'
import { Link } from 'react-router-dom'

const HomePage: React.FC = () => {
  const heroRef = useRef<HTMLDivElement>(null)

  useEffect(() => {
    const handleScroll = () => {
      if (heroRef.current) {
        const scrolled = window.scrollY
        heroRef.current.style.transform = `translateY(${scrolled * 0.5}px)`
      }
    }
    window.addEventListener('scroll', handleScroll)
    return () => window.removeEventListener('scroll', handleScroll)
  }, [])

  return (
    <div className="min-h-screen bg-black text-white overflow-hidden">
      {/* Hero Section */}
      <section className="relative h-screen flex items-center justify-center overflow-hidden">
        {/* Animated Background Elements */}
        <div className="absolute inset-0 z-0">
          <div className="absolute top-1/4 left-1/4 w-96 h-96 bg-purple-600/20 rounded-full blur-[100px] animate-pulse-glow"></div>
          <div className="absolute bottom-1/4 right-1/4 w-96 h-96 bg-blue-600/20 rounded-full blur-[100px] animate-pulse-glow" style={{ animationDelay: '1s' }}></div>
        </div>

        <div className="relative z-10 text-center px-4" ref={heroRef}>
          <h1 className="text-6xl md:text-8xl font-black tracking-tighter mb-6 neon-text animate-float">
            RHYTHM<br />
            <span className="text-transparent bg-clip-text bg-gradient-to-r from-purple-400 to-blue-400">MOTION</span>
          </h1>
          <p className="text-xl md:text-2xl text-gray-300 mb-10 max-w-2xl mx-auto font-light">
            Connect your phone. Control your avatar. <br />
            Experience the rhythm with your whole body.
          </p>
          <div className="flex flex-col md:flex-row gap-6 justify-center">
            <Link
              to="/signup"
              className="px-10 py-4 bg-purple-600 text-white text-lg font-bold rounded-full hover:bg-purple-700 hover:scale-105 hover:shadow-[0_0_30px_rgba(168,85,247,0.6)] transition-all duration-300 no-underline"
            >
              START PLAYING
            </Link>
            <a
              href="#features"
              className="px-10 py-4 bg-transparent border border-gray-600 text-white text-lg font-bold rounded-full hover:border-white hover:bg-white/10 transition-all duration-300 no-underline"
            >
              LEARN MORE
            </a>
          </div>
        </div>

        {/* Scroll Indicator */}
        <div className="absolute bottom-10 left-1/2 transform -translate-x-1/2 animate-bounce">
          <svg className="w-6 h-6 text-gray-500" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 14l-7 7m0 0l-7-7m7 7V3" />
          </svg>
        </div>
      </section>

      {/* Features Section */}
      <section id="features" className="py-32 relative z-10 bg-gradient-to-b from-black to-gray-900">
        <div className="max-w-7xl mx-auto px-6">
          <div className="text-center mb-24">
            <h2 className="text-4xl md:text-5xl font-bold mb-6">HOW IT WORKS</h2>
            <div className="w-24 h-1 bg-purple-600 mx-auto rounded-full"></div>
          </div>

          <div className="grid grid-cols-1 md:grid-cols-3 gap-12">
            {/* Feature 1 */}
            <div className="bg-gray-800/50 p-10 rounded-2xl border border-gray-700 hover:border-purple-500 transition-all duration-300 group">
              <div className="text-5xl mb-6 group-hover:scale-110 transition-transform duration-300">📱</div>
              <h3 className="text-2xl font-bold mb-4 text-white">Phone Camera</h3>
              <p className="text-gray-400 leading-relaxed">
                No expensive VR gear needed. Just use your smartphone camera to track your movements in real-time.
              </p>
            </div>

            {/* Feature 2 */}
            <div className="bg-gray-800/50 p-10 rounded-2xl border border-gray-700 hover:border-blue-500 transition-all duration-300 group">
              <div className="text-5xl mb-6 group-hover:scale-110 transition-transform duration-300">💃</div>
              <h3 className="text-2xl font-bold mb-4 text-white">Motion Sync</h3>
              <p className="text-gray-400 leading-relaxed">
                Your movements are instantly synchronized with your Unity avatar. Dance, jump, and move to the beat.
              </p>
            </div>

            {/* Feature 3 */}
            <div className="bg-gray-800/50 p-10 rounded-2xl border border-gray-700 hover:border-pink-500 transition-all duration-300 group">
              <div className="text-5xl mb-6 group-hover:scale-110 transition-transform duration-300">🎮</div>
              <h3 className="text-2xl font-bold mb-4 text-white">Rhythm Action</h3>
              <p className="text-gray-400 leading-relaxed">
                Hit the notes by matching poses and actions. Compete with players around the world for the high score.
              </p>
            </div>
          </div>
        </div>
      </section>

      {/* CTA Section */}
      <section className="py-32 bg-black relative overflow-hidden">
        <div className="absolute inset-0 bg-purple-900/10"></div>
        <div className="max-w-5xl mx-auto px-6 text-center relative z-10">
          <h2 className="text-4xl md:text-6xl font-bold mb-8">READY TO DANCE?</h2>
          <p className="text-xl text-gray-400 mb-12">
            Join thousands of players and experience the future of rhythm games.
          </p>
          <Link
            to="/signup"
            className="inline-block px-12 py-5 bg-white text-black text-xl font-bold rounded-full hover:bg-gray-200 hover:scale-105 transition-all duration-300 no-underline"
          >
            JOIN NOW FOR FREE
          </Link>
        </div>
      </section>
    </div>
  )
}

export default HomePage
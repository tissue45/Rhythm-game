import React, { useState } from 'react'
import { Link } from 'react-router-dom'
import { useUser } from '../context/UserContext'

const Header: React.FC = () => {
  const { currentUser, logout } = useUser()
  const [isMenuOpen, setIsMenuOpen] = useState(false)

  const handleLogout = async () => {
    try {
      await logout()
      alert('로그아웃되었습니다.')
      window.location.replace('/')
    } catch (error) {
      console.error('Logout error:', error)
    }
  }

  const toggleMenu = () => setIsMenuOpen(!isMenuOpen)

  return (
    <header className="fixed top-0 left-0 right-0 w-full bg-black/90 backdrop-blur-md border-b border-purple-900/50 z-50 text-white">
      <div className="max-w-7xl mx-auto px-6 py-4 flex justify-between items-center">
        {/* Logo */}
        <Link to="/" className="text-2xl font-bold tracking-tighter hover:text-purple-400 transition-colors flex items-center gap-2 no-underline text-white">
          <span className="text-purple-500 text-3xl">⚡</span>
          <span>Step up</span>
        </Link>

        {/* Desktop Navigation */}
        <nav className="hidden md:flex items-center gap-8">
          <Link to="/" className="text-gray-300 hover:text-purple-400 font-medium transition-colors no-underline">HOME</Link>
          <Link to="/game" className="text-gray-300 hover:text-purple-400 font-medium transition-colors no-underline">GAME START</Link>
          <Link to="/ranking" className="text-gray-300 hover:text-purple-400 font-medium transition-colors no-underline">RANKING</Link>
          <Link to="/payment" className="text-gray-300 hover:text-purple-400 font-medium transition-colors no-underline">💎 SHOP</Link>

          {currentUser ? (
            <div className="flex items-center gap-6 ml-4 border-l border-gray-700 pl-6">
              <Link to="/mypage" className="text-white font-semibold hover:text-purple-400 transition-colors no-underline">
                {currentUser.name}님
              </Link>
              <button
                onClick={handleLogout}
                className="bg-transparent border border-purple-500 text-purple-400 px-4 py-1.5 rounded-full hover:bg-purple-500 hover:text-white transition-all duration-300 cursor-pointer"
              >
                LOGOUT
              </button>
            </div>
          ) : (
            <div className="flex items-center gap-4 ml-4">
              <Link to="/login" className="text-gray-300 hover:text-white font-medium transition-colors no-underline">LOGIN</Link>
              <Link
                to="/game"
                className="bg-purple-600 text-white px-5 py-2 rounded-full font-bold hover:bg-purple-700 hover:shadow-[0_0_15px_rgba(168,85,247,0.5)] transition-all duration-300 no-underline"
              >
                PLAY NOW
              </Link>
            </div>
          )}
        </nav>

        {/* Mobile Menu Button */}
        <button className="md:hidden text-white text-2xl" onClick={toggleMenu}>
          {isMenuOpen ? '✕' : '☰'}
        </button>
      </div>

      {/* Mobile Menu */}
      {isMenuOpen && (
        <div className="md:hidden bg-black border-t border-gray-800 p-4 flex flex-col gap-4">
          <Link to="/" className="text-gray-300 hover:text-purple-400 py-2 block no-underline" onClick={() => setIsMenuOpen(false)}>HOME</Link>
          <Link to="/game" className="text-gray-300 hover:text-purple-400 py-2 block no-underline" onClick={() => setIsMenuOpen(false)}>GAME START</Link>
          <Link to="/ranking" className="text-gray-300 hover:text-purple-400 py-2 block no-underline" onClick={() => setIsMenuOpen(false)}>RANKING</Link>
          <Link to="/payment" className="text-gray-300 hover:text-purple-400 py-2 block no-underline" onClick={() => setIsMenuOpen(false)}>💎 SHOP</Link>
          {currentUser ? (
            <>
              <Link to="/mypage" className="text-white font-semibold py-2 block no-underline" onClick={() => setIsMenuOpen(false)}>MY PAGE</Link>
              <button onClick={handleLogout} className="text-left text-gray-400 py-2 block w-full">LOGOUT</button>
            </>
          ) : (
            <>
              <Link to="/login" className="text-gray-300 py-2 block no-underline" onClick={() => setIsMenuOpen(false)}>LOGIN</Link>
              <Link to="/signup" className="text-purple-400 font-bold py-2 block no-underline" onClick={() => setIsMenuOpen(false)}>SIGN UP</Link>
            </>
          )}
        </div>
      )}
    </header>
  )
}

export default Header
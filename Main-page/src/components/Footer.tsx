import React from 'react'
import { Link } from 'react-router-dom'

const Footer: React.FC = () => {
  return (
    <footer className="w-full bg-black text-gray-400 py-12 border-t border-gray-800">
      <div className="max-w-7xl mx-auto px-6">
        <div className="flex flex-col md:flex-row justify-between items-center mb-8">
          <div className="mb-6 md:mb-0 text-center md:text-left">
            <h2 className="text-2xl font-bold text-white mb-2 tracking-tighter">
              <span className="text-purple-500">⚡</span> RHYTHM MOTION
            </h2>
            <p className="text-sm text-gray-500">
              Experience the next generation of rhythm games.<br />
              Connect your body, control the avatar.
            </p>
          </div>

          <div className="flex gap-6">
            <a href="#" className="text-gray-400 hover:text-purple-400 transition-colors text-2xl">
              <i className="fab fa-discord"></i> {/* Placeholder for icons */}
              <span>Discord</span>
            </a>
            <a href="#" className="text-gray-400 hover:text-purple-400 transition-colors text-2xl">
              <i className="fab fa-twitter"></i>
              <span>Twitter</span>
            </a>
            <a href="#" className="text-gray-400 hover:text-purple-400 transition-colors text-2xl">
              <i className="fab fa-youtube"></i>
              <span>YouTube</span>
            </a>
          </div>
        </div>

        <div className="border-t border-gray-800 pt-8 flex flex-col md:flex-row justify-between items-center text-xs">
          <div className="flex gap-4 mb-4 md:mb-0">
            <Link to="/terms" className="hover:text-white transition-colors no-underline text-gray-500">Terms of Service</Link>
            <Link to="/privacy" className="hover:text-white transition-colors no-underline text-gray-500">Privacy Policy</Link>
          </div>
          <p className="text-gray-600">
            © 2025 RHYTHM MOTION TEAM. All rights reserved.
          </p>
        </div>
      </div>
    </footer>
  )
}

export default Footer
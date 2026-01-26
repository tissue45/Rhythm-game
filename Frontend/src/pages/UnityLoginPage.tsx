import React, { useState, useEffect } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { login } from '../services/userService'
import { useUser } from '../context/UserContext'
import { authLogger } from '../utils/logger'

/**
 * Unity 연동용 로그인 페이지
 * 로그인 성공 시 Unity로 데이터 전달
 */
const UnityLoginPage: React.FC = () => {
  const navigate = useNavigate()
  const { setCurrentUser } = useUser()
  const [formData, setFormData] = useState({
    email: '',
    password: ''
  })
  const [isLoading, setIsLoading] = useState(false)
  const [isUnityMode, setIsUnityMode] = useState(false)

  useEffect(() => {
    // URL 파라미터로 Unity 모드 확인
    const params = new URLSearchParams(window.location.search)
    const unityMode = params.get('unity') === 'true'
    setIsUnityMode(unityMode)

    if (unityMode) {
      console.log('[UnityLogin] Unity mode activated')
    }
  }, [])

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const { name, value } = e.target
    setFormData(prev => ({
      ...prev,
      [name]: value
    }))
  }

  const sendToUnity = async (userName: string, userEmail: string) => {
    try {
      // 환경 변수에서 API URL 가져오기 (배포 환경 대응)
      const API_URL = import.meta.env.VITE_API_URL || 'http://localhost:3001'

      // Unity 인증 서버로 로그인 정보 전송
      const response = await fetch(`${API_URL}/api/unity-login`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({
          name: userName,
          email: userEmail
        })
      })

      if (response.ok) {
        console.log('[UnityLogin] Login data sent to Unity auth server')
      } else {
        console.error('[UnityLogin] Failed to send to Unity auth server')
      }

      // Unity WebGL용 메시지 전달 (WebGL 빌드인 경우)
      if (window.unityInstance) {
        const loginData = {
          name: userName,
          email: userEmail
        }
        window.unityInstance.SendMessage(
          'LoginManager',
          'ReceiveLoginData',
          JSON.stringify(loginData)
        )
        console.log('[UnityLogin] Sent data to Unity WebGL:', loginData)
      }

      // 로컬스토리지에도 저장 (백업)
      localStorage.setItem('unity_user_name', userName)
      localStorage.setItem('unity_user_email', userEmail)
      localStorage.setItem('unity_login_success', 'true')
      localStorage.setItem('unity_login_time', Date.now().toString())

      console.log('[UnityLogin] Login data saved successfully')
    } catch (error) {
      console.error('[UnityLogin] Failed to send to Unity:', error)
    }
  }

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    setIsLoading(true)

    try {
      const result = await login({
        email: formData.email,
        password: formData.password
      })

      if (result.success && result.user) {
        setCurrentUser(result.user)

        // Unity 모드인 경우 Unity로 데이터 전달
        if (isUnityMode) {
          sendToUnity(result.user.name, result.user.email)

          // 성공 메시지 표시 후 창 닫기
          alert(`${result.user.name}님, 로그인 성공!\n이 창을 닫고 게임으로 돌아가세요.`)

          // 3초 후 자동으로 창 닫기 시도
          setTimeout(() => {
            window.close()
          }, 3000)
        } else {
          alert(`${result.user.name}님, 환영합니다!`)
          navigate('/')
        }
      } else {
        alert(result.error || '로그인에 실패했습니다.')
      }
    } catch (error) {
      authLogger.error('Login error:', error)
      alert('로그인 중 오류가 발생했습니다.')
    } finally {
      setIsLoading(false)
    }
  }

  return (
    <div className="h-screen w-full bg-black overflow-y-auto">
      <div className="min-h-full flex items-center justify-center py-12 px-4 sm:px-6 lg:px-8">
        <div className="bg-gray-900/50 p-12 w-full max-w-[420px] rounded-2xl border border-gray-800 shadow-[0_0_30px_rgba(168,85,247,0.1)] backdrop-blur-sm">
          <div className="text-center mb-8">
            <h1 className="text-3xl font-bold text-white mb-2 tracking-tight">
              {isUnityMode ? 'UNITY LOGIN' : 'LOGIN'}
            </h1>
            <p className="text-gray-400 text-sm m-0">
              {isUnityMode ? 'Login for Step Up Game' : 'Welcome back to Step up'}
            </p>
            {isUnityMode && (
              <p className="text-purple-400 text-xs mt-2">
                게임과 연동된 로그인 페이지입니다
              </p>
            )}
          </div>

          <form className="mb-8" onSubmit={handleSubmit}>
            <div className="mb-5">
              <input
                type="email"
                name="email"
                placeholder="Email Address"
                value={formData.email}
                onChange={handleChange}
                required
                className="w-full py-4 px-5 border border-gray-700 rounded-lg text-base outline-none transition-all duration-300 bg-gray-800 text-white focus:border-purple-500 focus:shadow-[0_0_10px_rgba(168,85,247,0.3)] placeholder:text-gray-500"
              />
            </div>

            <div className="mb-5">
              <input
                type="password"
                name="password"
                placeholder="Password"
                value={formData.password}
                onChange={handleChange}
                required
                className="w-full py-4 px-5 border border-gray-700 rounded-lg text-base outline-none transition-all duration-300 bg-gray-800 text-white focus:border-purple-500 focus:shadow-[0_0_10px_rgba(168,85,247,0.3)] placeholder:text-gray-500"
              />
            </div>

            <div className="flex justify-between items-center mb-6 text-sm">
              <label className="flex items-center cursor-pointer text-gray-400 hover:text-gray-300">
                <input type="checkbox" className="hidden peer" />
                <span className="w-[18px] h-[18px] border border-gray-600 rounded-sm mr-2 relative transition-all duration-300 peer-checked:bg-purple-600 peer-checked:border-purple-600 after:content-[''] after:absolute after:left-[5px] after:top-[2px] after:w-[6px] after:h-[10px] after:border-white after:border-r-2 after:border-b-2 after:rotate-45 after:opacity-0 peer-checked:after:opacity-100"></span>
                Remember Me
              </label>
              <div className="flex items-center gap-2">
                <Link to="/find-id" className="text-gray-500 no-underline transition-colors duration-300 text-sm hover:text-purple-400">
                  Find ID
                </Link>
                <span className="text-gray-700 text-xs">|</span>
                <Link to="/forgot-password" className="text-gray-500 no-underline transition-colors duration-300 text-sm hover:text-purple-400">
                  Forgot Password
                </Link>
              </div>
            </div>

            <button
              type="submit"
              disabled={isLoading}
              className="w-full py-4 bg-purple-600 text-white border-none rounded-lg text-base font-bold cursor-pointer transition-all duration-300 hover:bg-purple-700 hover:shadow-[0_0_20px_rgba(168,85,247,0.5)] disabled:bg-gray-700 disabled:cursor-not-allowed"
            >
              {isLoading ? 'Logging in...' : 'LOGIN'}
            </button>
          </form>

          {!isUnityMode && (
            <div className="text-center">
              <div className="mb-4 text-sm text-gray-500">
                <span>Not a member yet?</span>
              </div>
              <Link
                to="/signup"
                className="inline-block w-full py-3.5 bg-transparent text-white border border-gray-600 rounded-lg text-base font-semibold no-underline text-center cursor-pointer transition-all duration-300 hover:border-white hover:bg-white/5"
              >
                Sign Up
              </Link>
            </div>
          )}
        </div>
      </div>
    </div>
  )
}

// TypeScript: Window 인터페이스 확장
declare global {
  interface Window {
    unityInstance?: any
  }
}

export default UnityLoginPage

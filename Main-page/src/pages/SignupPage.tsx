import React, { useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { signup, checkEmailExists } from '../services/userService'

const SignupPage: React.FC = () => {
  const navigate = useNavigate()
  const [formData, setFormData] = useState({
    email: '',
    password: '',
    confirmPassword: '',
    name: '',
    phone: '',
    address: '',
    agreeTerms: false,
    agreePrivacy: false,
    agreeMarketing: false
  })
  const [isLoading, setIsLoading] = useState(false)

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const { name, value, type, checked } = e.target
    setFormData(prev => ({
      ...prev,
      [name]: type === 'checkbox' ? checked : value
    }))
  }

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()

    if (formData.password !== formData.confirmPassword) {
      alert('비밀번호가 일치하지 않습니다.')
      return
    }

    if (!formData.agreeTerms || !formData.agreePrivacy) {
      alert('필수 약관에 동의해주세요.')
      return
    }

    setIsLoading(true)

    try {
      const emailExists = await checkEmailExists(formData.email)
      if (emailExists) {
        alert('이미 가입된 이메일입니다.')
        setIsLoading(false)
        return
      }

      const result = await signup({
        email: formData.email,
        password: formData.password,
        name: formData.name,
        phone: formData.phone,
        address: formData.address,
        agreeMarketing: formData.agreeMarketing
      })

      if (result.success) {
        alert('회원가입이 완료되었습니다! 로그인하여 서비스를 이용하세요.')
        navigate('/login')
      } else {
        alert(result.error || '회원가입에 실패했습니다.')
      }
    } catch (error) {
      console.error('Signup error:', error)
      alert('회원가입 중 오류가 발생했습니다.')
    } finally {
      setIsLoading(false)
    }
  }

  return (
    <div className="min-h-screen flex items-center justify-center bg-black py-10 px-5">
      <div className="bg-gray-900/50 p-12 w-full max-w-[480px] rounded-2xl border border-gray-800 shadow-[0_0_30px_rgba(168,85,247,0.1)] backdrop-blur-sm">
        <div className="text-center mb-8">
          <h1 className="text-3xl font-bold text-white mb-2 tracking-tight">SIGN UP</h1>
          <p className="text-gray-400 text-sm m-0">Join the rhythm revolution</p>
        </div>

        <form className="mb-8" onSubmit={handleSubmit}>
          <div className="mb-5">
            <input
              type="text"
              name="name"
              placeholder="Name"
              value={formData.name}
              onChange={handleChange}
              required
              className="w-full py-4 px-5 border border-gray-700 rounded-lg text-base outline-none transition-all duration-300 bg-gray-800 text-white focus:border-purple-500 focus:shadow-[0_0_10px_rgba(168,85,247,0.3)] placeholder:text-gray-500"
            />
          </div>

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
              type="tel"
              name="phone"
              placeholder="Phone Number"
              value={formData.phone}
              onChange={handleChange}
              required
              className="w-full py-4 px-5 border border-gray-700 rounded-lg text-base outline-none transition-all duration-300 bg-gray-800 text-white focus:border-purple-500 focus:shadow-[0_0_10px_rgba(168,85,247,0.3)] placeholder:text-gray-500"
            />
          </div>

          <div className="mb-5">
            <input
              type="text"
              name="address"
              placeholder="Address"
              value={formData.address}
              onChange={handleChange}
              required
              className="w-full py-4 px-5 border border-gray-700 rounded-lg text-base outline-none transition-all duration-300 bg-gray-800 text-white focus:border-purple-500 focus:shadow-[0_0_10px_rgba(168,85,247,0.3)] placeholder:text-gray-500"
            />
          </div>

          <div className="mb-5">
            <input
              type="password"
              name="password"
              placeholder="Password (8+ characters)"
              value={formData.password}
              onChange={handleChange}
              required
              minLength={8}
              className="w-full py-4 px-5 border border-gray-700 rounded-lg text-base outline-none transition-all duration-300 bg-gray-800 text-white focus:border-purple-500 focus:shadow-[0_0_10px_rgba(168,85,247,0.3)] placeholder:text-gray-500"
            />
          </div>

          <div className="mb-8">
            <input
              type="password"
              name="confirmPassword"
              placeholder="Confirm Password"
              value={formData.confirmPassword}
              onChange={handleChange}
              required
              className="w-full py-4 px-5 border border-gray-700 rounded-lg text-base outline-none transition-all duration-300 bg-gray-800 text-white focus:border-purple-500 focus:shadow-[0_0_10px_rgba(168,85,247,0.3)] placeholder:text-gray-500"
            />
          </div>

          <div className="mb-8 space-y-4">
            <div className="flex items-center justify-between py-3 border-b border-gray-800">
              <label className="flex items-center cursor-pointer text-gray-400 hover:text-gray-300">
                <input
                  type="checkbox"
                  name="agreeTerms"
                  checked={formData.agreeTerms}
                  onChange={handleChange}
                  className="hidden peer"
                />
                <span className="w-[18px] h-[18px] border border-gray-600 rounded-sm mr-3 relative transition-all duration-300 peer-checked:bg-purple-600 peer-checked:border-purple-600 after:content-[''] after:absolute after:left-[5px] after:top-[2px] after:w-[6px] after:h-[10px] after:border-white after:border-r-2 after:border-b-2 after:rotate-45 after:opacity-0 peer-checked:after:opacity-100"></span>
                <span className="text-purple-500 font-semibold mr-1">[Required]</span> Terms of Service
              </label>
            </div>

            <div className="flex items-center justify-between py-3 border-b border-gray-800">
              <label className="flex items-center cursor-pointer text-gray-400 hover:text-gray-300">
                <input
                  type="checkbox"
                  name="agreePrivacy"
                  checked={formData.agreePrivacy}
                  onChange={handleChange}
                  className="hidden peer"
                />
                <span className="w-[18px] h-[18px] border border-gray-600 rounded-sm mr-3 relative transition-all duration-300 peer-checked:bg-purple-600 peer-checked:border-purple-600 after:content-[''] after:absolute after:left-[5px] after:top-[2px] after:w-[6px] after:h-[10px] after:border-white after:border-r-2 after:border-b-2 after:rotate-45 after:opacity-0 peer-checked:after:opacity-100"></span>
                <span className="text-purple-500 font-semibold mr-1">[Required]</span> Privacy Policy
              </label>
            </div>
          </div>

          <button
            type="submit"
            disabled={isLoading}
            className="w-full py-4 bg-purple-600 text-white border-none rounded-lg text-base font-bold cursor-pointer transition-all duration-300 hover:bg-purple-700 hover:shadow-[0_0_20px_rgba(168,85,247,0.5)] disabled:bg-gray-700 disabled:cursor-not-allowed"
          >
            {isLoading ? 'Creating Account...' : 'SIGN UP'}
          </button>
        </form>

        <div className="text-center">
          <div className="mb-4 text-sm text-gray-500">
            <span>Already have an account?</span>
          </div>
          <Link
            to="/login"
            className="inline-block w-full py-3.5 bg-transparent text-white border border-gray-600 rounded-lg text-base font-semibold no-underline text-center cursor-pointer transition-all duration-300 hover:border-white hover:bg-white/5"
          >
            Login
          </Link>
        </div>
      </div>
    </div>
  )
}

export default SignupPage
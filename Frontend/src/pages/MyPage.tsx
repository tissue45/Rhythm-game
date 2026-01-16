import React, { useState, useEffect } from 'react'
import { useNavigate } from 'react-router-dom'
import { useUser, User } from '../context/UserContext'
import { verifyPasswordForAccess, changePassword } from '../services/userService'

const MyPage: React.FC = () => {
    const { currentUser, setCurrentUser } = useUser()
    const navigate = useNavigate()
    const [loading, setLoading] = useState(true)

    // Game Stats (Placeholder)
    const [gameStats, setGameStats] = useState({
        highScore: 0,
        rank: 'UNRANKED',
        totalPlayTime: '0h 0m',
        songsCleared: 0
    })

    // User Info Management State
    const [showPasswordModal, setShowPasswordModal] = useState(false)
    const [password, setPassword] = useState('')
    const [selectedAction, setSelectedAction] = useState('') // 'editInfo' or 'changePassword'

    const [showEditForm, setShowEditForm] = useState(false)
    const [editUserInfo, setEditUserInfo] = useState({
        name: '',
        email: '',
        phone: '',
        address: ''
    })

    const [showChangePasswordForm, setShowChangePasswordForm] = useState(false)
    const [passwordForm, setPasswordForm] = useState({
        current: '',
        new: '',
        confirm: ''
    })

    useEffect(() => {
        if (!currentUser) {
            navigate('/login')
            return
        }
        setEditUserInfo({
            name: currentUser.name || '',
            email: currentUser.email || '',
            phone: currentUser.phone || '',
            address: currentUser.address || ''
        })
        setLoading(false)

        // Simulate fetching game stats
        setTimeout(() => {
            setGameStats({
                highScore: 1250000,
                rank: 'GOLD',
                totalPlayTime: '12h 45m',
                songsCleared: 42
            })
        }, 500)
    }, [currentUser, navigate])

    const handleAuthAction = (action: string) => {
        setSelectedAction(action)
        setShowPasswordModal(true)
    }

    const handlePasswordSubmit = async () => {
        if (!password) return alert('비밀번호를 입력해주세요.')

        try {
            const result = await verifyPasswordForAccess(password)
            if (result.success) {
                setShowPasswordModal(false)
                setPassword('')
                if (selectedAction === 'editInfo') setShowEditForm(true)
                if (selectedAction === 'changePassword') setShowChangePasswordForm(true)
            } else {
                alert(result.error || '비밀번호가 일치하지 않습니다.')
            }
        } catch (error) {
            console.error(error)
            alert('오류가 발생했습니다.')
        }
    }

    const handleInfoUpdate = () => {
        if (!currentUser) return

        const updatedUser: User = {
            ...currentUser,
            ...editUserInfo
        }

        // Update Local Storage (Mock DB)
        localStorage.setItem('currentUser', JSON.stringify(updatedUser))
        const users = JSON.parse(localStorage.getItem('users') || '[]')
        const updatedUsers = users.map((u: any) => u.id === currentUser.id ? updatedUser : u)
        localStorage.setItem('users', JSON.stringify(updatedUsers))

        setCurrentUser(updatedUser)
        setShowEditForm(false)
        alert('회원정보가 수정되었습니다.')
    }

    const handlePasswordChange = async () => {
        if (passwordForm.new !== passwordForm.confirm) return alert('새 비밀번호가 일치하지 않습니다.')
        if (passwordForm.new.length < 8) return alert('비밀번호는 8자 이상이어야 합니다.')

        try {
            const result = await changePassword(passwordForm.current, passwordForm.new)
            if (result.success) {
                alert('비밀번호가 변경되었습니다.')
                setShowChangePasswordForm(false)
                setPasswordForm({ current: '', new: '', confirm: '' })
            } else {
                alert(result.error)
            }
        } catch (error) {
            alert('오류가 발생했습니다.')
        }
    }

    if (loading || !currentUser) return <div className="min-h-screen bg-black text-white flex items-center justify-center">Loading...</div>

    return (
        <div className="min-h-screen bg-black text-white py-20 px-4">
            <div className="max-w-4xl mx-auto">
                {/* Profile Header */}
                <div className="flex items-center gap-8 mb-12 p-8 bg-gray-900 rounded-2xl border border-gray-800 shadow-[0_0_30px_rgba(168,85,247,0.1)]">
                    <div className="w-24 h-24 rounded-full bg-gradient-to-br from-purple-600 to-blue-600 flex items-center justify-center text-4xl font-bold shadow-lg">
                        {currentUser.name.charAt(0)}
                    </div>
                    <div>
                        <h1 className="text-3xl font-bold mb-2">{currentUser.name}</h1>
                        <p className="text-gray-400">{currentUser.email}</p>
                        <div className="flex gap-4 mt-4">
                            <button
                                onClick={() => handleAuthAction('editInfo')}
                                className="px-4 py-2 bg-gray-800 hover:bg-gray-700 rounded-lg text-sm transition-colors border border-gray-700"
                            >
                                Edit Profile
                            </button>
                            <button
                                onClick={() => handleAuthAction('changePassword')}
                                className="px-4 py-2 bg-gray-800 hover:bg-gray-700 rounded-lg text-sm transition-colors border border-gray-700"
                            >
                                Change Password
                            </button>
                        </div>
                    </div>
                </div>

                {/* Game Stats Grid */}
                <h2 className="text-2xl font-bold mb-6 flex items-center gap-2">
                    <span className="text-purple-500">📊</span> GAME STATISTICS
                </h2>
                <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6 mb-12">
                    <div className="bg-gray-900 p-6 rounded-xl border border-gray-800 hover:border-purple-500/50 transition-colors">
                        <div className="text-gray-400 text-sm mb-2">High Score</div>
                        <div className="text-2xl font-bold text-purple-400">{gameStats.highScore.toLocaleString()}</div>
                    </div>
                    <div className="bg-gray-900 p-6 rounded-xl border border-gray-800 hover:border-purple-500/50 transition-colors">
                        <div className="text-gray-400 text-sm mb-2">Rank</div>
                        <div className="text-2xl font-bold text-yellow-400">{gameStats.rank}</div>
                    </div>
                    <div className="bg-gray-900 p-6 rounded-xl border border-gray-800 hover:border-purple-500/50 transition-colors">
                        <div className="text-gray-400 text-sm mb-2">Total Play Time</div>
                        <div className="text-2xl font-bold text-blue-400">{gameStats.totalPlayTime}</div>
                    </div>
                    <div className="bg-gray-900 p-6 rounded-xl border border-gray-800 hover:border-purple-500/50 transition-colors">
                        <div className="text-gray-400 text-sm mb-2">Songs Cleared</div>
                        <div className="text-2xl font-bold text-green-400">{gameStats.songsCleared}</div>
                    </div>
                </div>

                {/* Modals */}
                {showPasswordModal && (
                    <div className="fixed inset-0 bg-black/80 flex items-center justify-center z-50">
                        <div className="bg-gray-900 p-8 rounded-xl border border-gray-700 w-96">
                            <h3 className="text-xl font-bold mb-4">Password Verification</h3>
                            <input
                                type="password"
                                value={password}
                                onChange={(e) => setPassword(e.target.value)}
                                className="w-full bg-gray-800 border border-gray-700 rounded p-3 text-white mb-4 focus:border-purple-500 outline-none"
                                placeholder="Enter your password"
                            />
                            <div className="flex justify-end gap-2">
                                <button onClick={() => setShowPasswordModal(false)} className="px-4 py-2 text-gray-400 hover:text-white">Cancel</button>
                                <button onClick={handlePasswordSubmit} className="px-4 py-2 bg-purple-600 rounded hover:bg-purple-700">Confirm</button>
                            </div>
                        </div>
                    </div>
                )}

                {showEditForm && (
                    <div className="fixed inset-0 bg-black/80 flex items-center justify-center z-50">
                        <div className="bg-gray-900 p-8 rounded-xl border border-gray-700 w-[500px]">
                            <h3 className="text-xl font-bold mb-6">Edit Profile</h3>
                            <div className="space-y-4">
                                <div>
                                    <label className="block text-sm text-gray-400 mb-1">Name</label>
                                    <input
                                        value={editUserInfo.name}
                                        onChange={(e) => setEditUserInfo({ ...editUserInfo, name: e.target.value })}
                                        className="w-full bg-gray-800 border border-gray-700 rounded p-3 text-white focus:border-purple-500 outline-none"
                                    />
                                </div>
                                <div>
                                    <label className="block text-sm text-gray-400 mb-1">Email</label>
                                    <input
                                        value={editUserInfo.email}
                                        disabled
                                        className="w-full bg-gray-800/50 border border-gray-700 rounded p-3 text-gray-500 cursor-not-allowed"
                                    />
                                </div>
                                <div>
                                    <label className="block text-sm text-gray-400 mb-1">Phone</label>
                                    <input
                                        value={editUserInfo.phone}
                                        onChange={(e) => setEditUserInfo({ ...editUserInfo, phone: e.target.value })}
                                        className="w-full bg-gray-800 border border-gray-700 rounded p-3 text-white focus:border-purple-500 outline-none"
                                    />
                                </div>
                                <div>
                                    <label className="block text-sm text-gray-400 mb-1">Address</label>
                                    <input
                                        value={editUserInfo.address}
                                        onChange={(e) => setEditUserInfo({ ...editUserInfo, address: e.target.value })}
                                        className="w-full bg-gray-800 border border-gray-700 rounded p-3 text-white focus:border-purple-500 outline-none"
                                    />
                                </div>
                            </div>
                            <div className="flex justify-end gap-2 mt-8">
                                <button onClick={() => setShowEditForm(false)} className="px-4 py-2 text-gray-400 hover:text-white">Cancel</button>
                                <button onClick={handleInfoUpdate} className="px-4 py-2 bg-purple-600 rounded hover:bg-purple-700">Save Changes</button>
                            </div>
                        </div>
                    </div>
                )}
            </div>
        </div>
    )
}

export default MyPage
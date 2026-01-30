import React, { useState, useEffect } from 'react'
import { useNavigate, useSearchParams } from 'react-router-dom'
import { useUser } from '../context/UserContext'
import { tossPaymentsService } from '../services/tossPayments'

interface CoinPackage {
    id: string
    coins: number
    price: number
    bonus: number
    popular?: boolean
}

const coinPackages: CoinPackage[] = [
    { id: 'pack_100', coins: 100, price: 1000, bonus: 0 },
    { id: 'pack_500', coins: 500, price: 5000, bonus: 50, popular: true },
    { id: 'pack_1000', coins: 1000, price: 10000, bonus: 150 },
    { id: 'pack_5000', coins: 5000, price: 50000, bonus: 1000 },
]

const PaymentPage: React.FC = () => {
    const navigate = useNavigate()
    const [searchParams] = useSearchParams()
    const { currentUser } = useUser()
    const [selectedPackage, setSelectedPackage] = useState<CoinPackage | null>(null)
    const [paymentMethod, setPaymentMethod] = useState<'카드' | '카카오페이' | '계좌이체'>('카드')
    const [isProcessing, setIsProcessing] = useState(false)

    useEffect(() => {
        // URL에서 패키지 ID 가져오기
        const packageId = searchParams.get('package')
        if (packageId) {
            const pkg = coinPackages.find(p => p.id === packageId)
            if (pkg) setSelectedPackage(pkg)
        }

        // 로그인 확인
        if (!currentUser) {
            alert('로그인이 필요합니다.')
            navigate('/login')
        }
    }, [searchParams, currentUser, navigate])

    const handlePayment = async () => {
        if (!selectedPackage || !currentUser) return

        setIsProcessing(true)

        try {
            const orderId = tossPaymentsService.generateOrderId()
            const baseUrl = window.location.origin + import.meta.env.BASE_URL

            const paymentData = {
                amount: selectedPackage.price,
                orderId: orderId,
                orderName: `게임 코인 ${selectedPackage.coins + selectedPackage.bonus}개`,
                customerName: currentUser.name,
                customerEmail: currentUser.email,
                successUrl: `${baseUrl}/payment/success`,
                failUrl: `${baseUrl}/payment/fail`,
            }

            // 결제 수단에 따라 다른 메서드 호출
            if (paymentMethod === '카드') {
                await tossPaymentsService.requestPayment(paymentData)
            } else if (paymentMethod === '카카오페이') {
                await tossPaymentsService.requestKakaoPayment(paymentData)
            } else if (paymentMethod === '계좌이체') {
                await tossPaymentsService.requestTransferPayment(paymentData)
            }
        } catch (error) {
            console.error('결제 실패:', error)
            alert('결제 요청 중 오류가 발생했습니다.')
            setIsProcessing(false)
        }
    }

    if (!selectedPackage) {
        return (
            <div className="min-h-screen bg-gradient-to-b from-gray-900 via-purple-900 to-black flex items-center justify-center">
                <div className="text-center">
                    <h2 className="text-2xl text-white mb-4">코인 패키지를 선택해주세요</h2>
                    <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6 max-w-6xl mx-auto p-6">
                        {coinPackages.map((pkg) => (
                            <div
                                key={pkg.id}
                                onClick={() => setSelectedPackage(pkg)}
                                className={`relative bg-gray-800/50 backdrop-blur-sm border-2 ${pkg.popular ? 'border-yellow-500' : 'border-gray-700'
                                    } rounded-xl p-6 cursor-pointer hover:scale-105 transition-transform`}
                            >
                                {pkg.popular && (
                                    <div className="absolute -top-3 left-1/2 transform -translate-x-1/2 bg-yellow-500 text-black px-4 py-1 rounded-full text-sm font-bold">
                                        인기
                                    </div>
                                )}
                                <div className="text-center">
                                    <div className="text-4xl mb-2">💎</div>
                                    <div className="text-3xl font-bold text-cyan-400 mb-2">
                                        {pkg.coins.toLocaleString()}
                                    </div>
                                    {pkg.bonus > 0 && (
                                        <div className="text-sm text-yellow-400 mb-2">
                                            +{pkg.bonus} 보너스
                                        </div>
                                    )}
                                    <div className="text-2xl font-bold text-white mb-4">
                                        ₩{pkg.price.toLocaleString()}
                                    </div>
                                    <button className="w-full bg-purple-600 hover:bg-purple-700 text-white font-bold py-2 px-4 rounded-lg transition-colors">
                                        선택
                                    </button>
                                </div>
                            </div>
                        ))}
                    </div>
                    <button
                        onClick={() => navigate('/game')}
                        className="mt-8 bg-gray-700 hover:bg-gray-600 text-white font-bold py-3 px-8 rounded-lg transition-colors"
                    >
                        돌아가기
                    </button>
                </div>
            </div>
        )
    }

    return (
        <div className="min-h-screen bg-gradient-to-b from-gray-900 via-purple-900 to-black flex items-center justify-center p-4">
            <div className="bg-gray-800/80 backdrop-blur-md border border-gray-700 rounded-2xl p-8 max-w-lg w-full shadow-2xl">
                <h1 className="text-3xl font-bold text-white mb-6 text-center">코인 충전</h1>

                {/* 선택한 패키지 정보 */}
                <div className="bg-gray-900/50 rounded-xl p-6 mb-6 border border-cyan-500/30">
                    <div className="flex justify-between items-center mb-4">
                        <span className="text-gray-400">선택한 패키지</span>
                        <button
                            onClick={() => setSelectedPackage(null)}
                            className="text-cyan-400 hover:text-cyan-300 text-sm"
                        >
                            변경
                        </button>
                    </div>
                    <div className="text-center">
                        <div className="text-5xl mb-3">💎</div>
                        <div className="text-4xl font-bold text-cyan-400 mb-2">
                            {selectedPackage.coins.toLocaleString()}
                            {selectedPackage.bonus > 0 && (
                                <span className="text-2xl text-yellow-400"> +{selectedPackage.bonus}</span>
                            )}
                        </div>
                        <div className="text-3xl font-bold text-white">
                            ₩{selectedPackage.price.toLocaleString()}
                        </div>
                    </div>
                </div>

                {/* 결제 수단 선택 */}
                <div className="mb-6">
                    <h3 className="text-lg font-semibold text-white mb-3">결제 수단</h3>
                    <div className="space-y-3">
                        <button
                            onClick={() => setPaymentMethod('카드')}
                            className={`w-full p-4 rounded-lg border-2 transition-all ${paymentMethod === '카드'
                                    ? 'border-cyan-500 bg-cyan-500/10'
                                    : 'border-gray-600 bg-gray-700/50 hover:border-gray-500'
                                }`}
                        >
                            <div className="flex items-center justify-between">
                                <div className="flex items-center gap-3">
                                    <span className="text-2xl">💳</span>
                                    <span className="text-white font-semibold">신용/체크카드</span>
                                </div>
                                {paymentMethod === '카드' && <span className="text-cyan-400">✓</span>}
                            </div>
                        </button>

                        <button
                            onClick={() => setPaymentMethod('카카오페이')}
                            className={`w-full p-4 rounded-lg border-2 transition-all ${paymentMethod === '카카오페이'
                                    ? 'border-yellow-500 bg-yellow-500/10'
                                    : 'border-gray-600 bg-gray-700/50 hover:border-gray-500'
                                }`}
                        >
                            <div className="flex items-center justify-between">
                                <div className="flex items-center gap-3">
                                    <span className="text-2xl">💬</span>
                                    <span className="text-white font-semibold">카카오페이</span>
                                </div>
                                {paymentMethod === '카카오페이' && <span className="text-yellow-400">✓</span>}
                            </div>
                        </button>

                        <button
                            onClick={() => setPaymentMethod('계좌이체')}
                            className={`w-full p-4 rounded-lg border-2 transition-all ${paymentMethod === '계좌이체'
                                    ? 'border-green-500 bg-green-500/10'
                                    : 'border-gray-600 bg-gray-700/50 hover:border-gray-500'
                                }`}
                        >
                            <div className="flex items-center justify-between">
                                <div className="flex items-center gap-3">
                                    <span className="text-2xl">🏦</span>
                                    <span className="text-white font-semibold">계좌이체</span>
                                </div>
                                {paymentMethod === '계좌이체' && <span className="text-green-400">✓</span>}
                            </div>
                        </button>
                    </div>
                </div>

                {/* 결제 버튼 */}
                <button
                    onClick={handlePayment}
                    disabled={isProcessing}
                    className={`w-full py-4 rounded-lg font-bold text-lg transition-all ${isProcessing
                            ? 'bg-gray-600 cursor-not-allowed'
                            : 'bg-gradient-to-r from-cyan-500 to-purple-600 hover:from-cyan-600 hover:to-purple-700 shadow-lg hover:shadow-cyan-500/50'
                        } text-white`}
                >
                    {isProcessing ? '처리 중...' : `₩${selectedPackage.price.toLocaleString()} 결제하기`}
                </button>

                <button
                    onClick={() => navigate('/game')}
                    className="w-full mt-4 py-3 bg-gray-700 hover:bg-gray-600 text-white font-semibold rounded-lg transition-colors"
                >
                    취소
                </button>

                {/* 안내 문구 */}
                <p className="text-xs text-gray-400 text-center mt-6">
                    테스트 환경입니다. 실제 결제가 진행되지 않습니다.
                </p>
            </div>
        </div>
    )
}

export default PaymentPage

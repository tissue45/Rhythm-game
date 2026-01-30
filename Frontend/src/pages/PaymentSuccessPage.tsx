import React, { useEffect, useState } from 'react';
import { useSearchParams, useNavigate } from 'react-router-dom';
import { supabase } from '../services/supabase';

const PaymentSuccessPage: React.FC = () => {
  const [searchParams] = useSearchParams();
  const navigate = useNavigate();
  const [status, setStatus] = useState('Processing...');

  useEffect(() => {
    const processPayment = async () => {
      const paymentKey = searchParams.get('paymentKey');
      const orderId = searchParams.get('orderId');
      const amount = Number(searchParams.get('amount'));

      if (!paymentKey || !orderId || !amount) {
        setStatus('결제 정보가 올바르지 않습니다.');
        return;
      }

      try {
        // 환경 변수에서 API URL 가져오기 (배포 환경 대응)
        const API_URL = import.meta.env.VITE_API_URL || 'http://localhost:3001'

        // 1. 백엔드 서버로 결제 검증 요청
        const confirmResponse = await fetch(`${API_URL}/api/payment/confirm`, {
          method: 'POST',
          headers: {
            'Content-Type': 'application/json',
          },
          body: JSON.stringify({
            paymentKey,
            orderId,
            amount,
          }),
        });

        const confirmData = await confirmResponse.json();

        if (!confirmData.success) {
          throw new Error(confirmData.error || '결제 검증 실패');
        }

        console.log('결제 검증 성공:', confirmData);

        // 2. 사용자 정보 가져오기
        const user = sessionStorage.getItem('currentUser');
        if (!user) {
          setStatus('결제는 성공했지만 로그인이 필요합니다. 로그인 후 코인이 지급됩니다.');
          return;
        }

        const u = JSON.parse(user);

        // 3. Supabase에서 현재 코인 가져오기
        const { data: userData, error: fetchError } = await supabase
          .from('users')
          .select('coins')
          .eq('id', u.id)
          .single();

        if (fetchError) throw fetchError;

        // 4. 코인 계산 (1원 = 1코인)
        const coinsToAdd = amount;
        const newBalance = (userData?.coins || 0) + coinsToAdd;

        // 5. Supabase 코인 업데이트
        const { error: updateError } = await supabase
          .from('users')
          .update({ coins: newBalance })
          .eq('id', u.id);

        if (updateError) throw updateError;

        setStatus(`결제 완료! ${coinsToAdd.toLocaleString()} 코인이 충전되었습니다.`);

        // 6. 세션 스토리지 업데이트
        u.coins = newBalance;
        sessionStorage.setItem('currentUser', JSON.stringify(u));

      } catch (err: any) {
        console.error('결제 처리 실패:', err);
        setStatus(`결제 처리 중 오류가 발생했습니다: ${err.message}`);
      }
    };

    processPayment();
  }, [searchParams]);

  return (
    <div className="flex flex-col items-center justify-center min-h-screen bg-black text-white p-4">
      <h1 className="text-3xl font-bold text-green-400 mb-4">Payment Successful!</h1>
      <p className="text-xl mb-8">{status}</p>

      <button
        onClick={() => navigate('/game')}
        className="bg-purple-600 hover:bg-purple-700 text-white font-bold py-3 px-8 rounded-lg shadow-lg transition-all"
      >
        Return to Game
      </button>
    </div>
  );
};

export default PaymentSuccessPage;
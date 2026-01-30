import React from 'react';
import { useSearchParams, useNavigate } from 'react-router-dom';

const PaymentFailPage: React.FC = () => {
  const [searchParams] = useSearchParams();
  const navigate = useNavigate();
  const message = searchParams.get('message') || 'Payment failed due to an error.';
  const code = searchParams.get('code');

  return (
    <div className="flex flex-col items-center justify-center min-h-screen bg-black text-white p-4">
      <h1 className="text-3xl font-bold text-red-500 mb-4">Payment Failed</h1>
      <p className="text-lg text-gray-300 mb-2">Error Code: {code}</p>
      <p className="text-xl mb-8">{message}</p>

      <button
        onClick={() => navigate('/game')}
        className="bg-gray-600 hover:bg-gray-700 text-white font-bold py-3 px-8 rounded-lg shadow-lg transition-all"
      >
        Return to Game
      </button>
    </div>
  );
};

export default PaymentFailPage;
const express = require('express');
const cors = require('cors');

const app = express();
// 배포 환경(Render.com)에서는 환경 변수 PORT 사용
const PORT = process.env.PORT || 3001;

// 로그인 데이터를 임시 저장할 메모리 저장소
let loginData = null;
let loginTimestamp = null;

// CORS 설정 (모든 출처 허용)
app.use(cors());
app.use(express.json());

// Unity가 로그인 데이터를 요청하는 엔드포인트 (GET)
app.get('/api/unity-login', (req, res) => {
    // 데이터가 있고 5분 이내인 경우만 반환
    if (loginData && loginTimestamp && (Date.now() - loginTimestamp < 300000)) {
        console.log('[Unity Auth Server] Unity requested login data:', loginData);

        // 데이터 반환
        const data = { ...loginData };

        // 데이터 반환 후 삭제 (한 번만 사용)
        loginData = null;
        loginTimestamp = null;

        res.json({
            success: true,
            data: data
        });
    } else {
        // 로그인 데이터 없음
        res.json({
            success: false,
            data: null
        });
    }
});

// 웹에서 로그인 데이터를 보내는 엔드포인트 (POST)
app.post('/api/unity-login', (req, res) => {
    const { name, email } = req.body;

    if (name && email) {
        // 로그인 데이터 저장
        loginData = { name, email };
        loginTimestamp = Date.now();

        console.log('[Unity Auth Server] Received login data from web:', loginData);
        console.log('[Unity Auth Server] Data will expire in 5 minutes');

        res.json({
            success: true,
            message: 'Login data saved for Unity'
        });
    } else {
        res.status(400).json({
            success: false,
            error: 'Missing name or email'
        });
    }
});

// 서버 상태 확인 엔드포인트
app.get('/api/health', (req, res) => {
    res.json({
        status: 'running',
        timestamp: Date.now(),
        hasLoginData: loginData !== null
    });
});

// [NEW] 토스페이먼츠 결제 검증 엔드포인트
app.post('/api/payment/confirm', async (req, res) => {
    const { paymentKey, orderId, amount } = req.body;

    // 토스페이먼츠 시크릿 키 (테스트용)
    const SECRET_KEY = 'test_sk_zXLkKEypNArWmo50nX3lmeaxYG5R';

    try {
        // 토스페이먼츠 API로 결제 승인 요청
        const response = await fetch('https://api.tosspayments.com/v1/payments/confirm', {
            method: 'POST',
            headers: {
                'Authorization': `Basic ${Buffer.from(SECRET_KEY + ':').toString('base64')}`,
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                paymentKey,
                orderId,
                amount
            })
        });

        const paymentData = await response.json();

        if (response.ok) {
            console.log('[Payment] Payment confirmed:', paymentData);

            // 여기서 DB에 코인 추가 로직을 구현
            // 현재는 성공 응답만 반환
            res.json({
                success: true,
                payment: paymentData
            });
        } else {
            console.error('[Payment] Payment confirmation failed:', paymentData);
            res.status(400).json({
                success: false,
                error: paymentData.message || 'Payment confirmation failed'
            });
        }
    } catch (error) {
        console.error('[Payment] Error confirming payment:', error);
        res.status(500).json({
            success: false,
            error: 'Internal server error'
        });
    }
});

// 서버 시작 (배포 환경에서는 0.0.0.0에서 리스닝)
app.listen(PORT, '0.0.0.0', () => {
    console.log('=================================');
    console.log('Unity Authentication Bridge Server');
    console.log('=================================');
    console.log(`Server running on port ${PORT}`);
    console.log(`Health check: http://localhost:${PORT}/api/health`);
    console.log('');
    console.log('This server acts as a bridge between web login and Unity game');
    console.log('- Web sends login data here (POST)');
    console.log('- Unity polls for login data (GET)');
    console.log('=================================');
});

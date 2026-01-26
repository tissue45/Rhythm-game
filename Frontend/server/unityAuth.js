// Unity 로그인 정보 임시 저장용 간단한 서버
const express = require('express');
const cors = require('cors');
const fs = require('fs');
const path = require('path');

const app = express();
const PORT = 3001;

// Unity 로그인 정보 저장
let currentLoginData = null;

app.use(cors());
app.use(express.json());

// 로그인 정보 저장 (웹에서 호출)
app.post('/api/unity-login', (req, res) => {
  const { name, email } = req.body;

  if (!name || !email) {
    return res.status(400).json({ error: 'Name and email are required' });
  }

  currentLoginData = {
    name,
    email,
    timestamp: Date.now()
  };

  console.log('[Unity Auth] Login data saved:', currentLoginData);

  res.json({ success: true, message: 'Login data saved for Unity' });
});

// 로그인 정보 가져오기 (Unity에서 호출)
app.get('/api/unity-login', (req, res) => {
  if (!currentLoginData) {
    return res.status(404).json({ error: 'No login data available' });
  }

  // 5분 이상 지난 데이터는 무효
  const fiveMinutes = 5 * 60 * 1000;
  if (Date.now() - currentLoginData.timestamp > fiveMinutes) {
    currentLoginData = null;
    return res.status(404).json({ error: 'Login data expired' });
  }

  console.log('[Unity Auth] Login data retrieved by Unity:', currentLoginData);

  const data = { ...currentLoginData };

  // 한 번 가져가면 삭제 (보안)
  currentLoginData = null;

  res.json({
    success: true,
    name: data.name,
    email: data.email
  });
});

// 상태 확인
app.get('/api/status', (req, res) => {
  res.json({
    status: 'running',
    hasLoginData: !!currentLoginData
  });
});

app.listen(PORT, () => {
  console.log(`\n=================================`);
  console.log(`Unity Auth Server running!`);
  console.log(`Port: ${PORT}`);
  console.log(`Save Login: POST http://localhost:${PORT}/api/unity-login`);
  console.log(`Get Login: GET http://localhost:${PORT}/api/unity-login`);
  console.log(`=================================\n`);
});

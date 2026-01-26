const http = require('http');

console.log('==========================================');
console.log('Testing Server Connectivity');
console.log('==========================================\n');

// Test Auth Server
function testAuthServer() {
    return new Promise((resolve) => {
        console.log('1. Testing Unity Auth Server (port 3001)...');
        const req = http.get('http://localhost:3001/api/health', (res) => {
            let data = '';
            res.on('data', chunk => data += chunk);
            res.on('end', () => {
                if (res.statusCode === 200) {
                    console.log('   ✅ Auth Server is running');
                    console.log(`   Response: ${data}\n`);
                    resolve(true);
                } else {
                    console.log(`   ❌ Auth Server returned status ${res.statusCode}\n`);
                    resolve(false);
                }
            });
        });

        req.on('error', () => {
            console.log('   ❌ Auth Server is NOT running');
            console.log('   Please run: START_UNITY_AUTH_SERVER.bat\n');
            resolve(false);
        });

        req.setTimeout(2000, () => {
            req.destroy();
            console.log('   ❌ Auth Server timeout\n');
            resolve(false);
        });
    });
}

// Test Frontend Server
function testFrontendServer() {
    return new Promise((resolve) => {
        console.log('2. Testing Frontend Dev Server (port 5173)...');
        const req = http.get('http://localhost:5173', (res) => {
            if (res.statusCode === 200) {
                console.log('   ✅ Frontend Server is running\n');
                resolve(true);
            } else {
                console.log(`   ❌ Frontend Server returned status ${res.statusCode}\n`);
                resolve(false);
            }
        });

        req.on('error', () => {
            console.log('   ❌ Frontend Server is NOT running');
            console.log('   Please run: cd Frontend && npm run dev\n');
            resolve(false);
        });

        req.setTimeout(2000, () => {
            req.destroy();
            console.log('   ❌ Frontend Server timeout\n');
            resolve(false);
        });
    });
}

// Test Backend Server (user's database server)
function testBackendServer() {
    return new Promise((resolve) => {
        console.log('3. Testing Backend Server (port 3000)...');
        const req = http.get('http://localhost:3000', (res) => {
            if (res.statusCode === 200 || res.statusCode === 404) {
                console.log('   ✅ Backend Server is running\n');
                resolve(true);
            } else {
                console.log(`   ⚠️  Backend Server returned status ${res.statusCode}\n`);
                resolve(true); // Still consider it running
            }
        });

        req.on('error', () => {
            console.log('   ⚠️  Backend Server is NOT running');
            console.log('   This is your database server. Make sure it\'s started.\n');
            resolve(false);
        });

        req.setTimeout(2000, () => {
            req.destroy();
            console.log('   ⚠️  Backend Server timeout\n');
            resolve(false);
        });
    });
}

// Run all tests
async function runTests() {
    const authOk = await testAuthServer();
    const frontendOk = await testFrontendServer();
    const backendOk = await testBackendServer();

    console.log('==========================================');
    console.log('Test Results:');
    console.log('==========================================');
    console.log(`Auth Server (3001):     ${authOk ? '✅ Ready' : '❌ Not Ready'}`);
    console.log(`Frontend Server (5173): ${frontendOk ? '✅ Ready' : '❌ Not Ready'}`);
    console.log(`Backend Server (3000):  ${backendOk ? '✅ Ready' : '⚠️  Check Status'}`);
    console.log('==========================================\n');

    if (authOk && frontendOk) {
        console.log('🎉 All required servers are ready!');
        console.log('\nNext steps:');
        console.log('1. Open Unity and run Main scene');
        console.log('2. Click LOGIN button in Unity');
        console.log('3. Login in the browser that opens');
        console.log('4. Watch Unity Console for login success\n');
    } else {
        console.log('⚠️  Some servers are not running.');
        console.log('Run START_ALL_SERVERS.bat to start all servers.\n');
    }

    if (!backendOk) {
        console.log('Note: Backend server (port 3000) should be running');
        console.log('      This is your database/authentication server.\n');
    }
}

runTests();

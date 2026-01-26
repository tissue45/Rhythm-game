import React, { useEffect, useState } from 'react';
import { io, Socket } from 'socket.io-client';

// Define socket URL based on current host but port 5000 (Flask server)
// Socket URL is now relative (handled by Vite Proxy)
const getSocketUrl = () => {
    return undefined; // Let io() determine URL (defaults to window.location)
};

const ControllerPage: React.FC = () => {
    const [socket, setSocket] = useState<Socket | null>(null);
    const [status, setStatus] = useState<string>('Connecting...');
    const [logs, setLogs] = useState<string[]>([]);

    const [permissionGranted, setPermissionGranted] = useState<boolean>(false);

    // On-screen logger
    useEffect(() => {
        const originalLog = console.log;
        const originalError = console.error;
        console.log = (...args) => {
            setLogs(prev => [...prev.slice(-4), `LOG: ${args.join(' ')}`]);
            originalLog(...args);
        };
        console.error = (...args) => {
            setLogs(prev => [...prev.slice(-4), `ERR: ${args.join(' ')}`]);
            originalError(...args);
        };
        window.onerror = (msg) => {
            setLogs(prev => [...prev.slice(-4), `CRASH: ${msg}`]);
            return false;
        };
        return () => {
            console.log = originalLog;
            console.error = originalError;
            window.onerror = null;
        };
    }, []);

    useEffect(() => {
        const newSocket = io(getSocketUrl(), { transports: ['websocket'] });
        newSocket.on('connect', () => {
            setStatus('Connected');
            newSocket.emit('client_ready');
            triggerHaptic([50]);
        });
        newSocket.on('disconnect', () => setStatus('Disconnected'));
        setSocket(newSocket);
        return () => { newSocket.disconnect(); };
    }, []);

    const triggerHaptic = (pattern: number[]) => {
        if (navigator.vibrate) navigator.vibrate(pattern);
    };

    const handleTap = () => {
        if (!socket) return;
        triggerHaptic([15]);
        const payload = {
            type: 'tap',
            lane: 99,
            timestamp: Date.now()
        };
        socket.emit('mobile_input', payload);
    };

    // Handle Motion (Tilt)
    useEffect(() => {
        const handleMotion = (event: DeviceOrientationEvent) => {
            if (!socket) return;

            // Gamma: Left/Right tilt (-90 to 90)
            // Beta: Front/Back tilt (-180 to 180)
            const gamma = event.gamma || 0;

            // Normalize: -45 to 45 degrees -> -1.0 to 1.0
            let normalizedTilt = gamma / 45.0;
            if (normalizedTilt > 1.0) normalizedTilt = 1.0;
            if (normalizedTilt < -1.0) normalizedTilt = -1.0;

            // Threshold to avoid drift
            if (Math.abs(normalizedTilt) < 0.1) normalizedTilt = 0;

            const payload = {
                type: 'tilt',
                value: normalizedTilt,
                raw: gamma
            };
            socket.emit('mobile_input', payload);
        };

        if (permissionGranted || typeof (DeviceOrientationEvent as any).requestPermission !== 'function') {
            window.addEventListener('deviceorientation', handleMotion);
        }

        return () => {
            window.removeEventListener('deviceorientation', handleMotion);
        };
    }, [socket, permissionGranted]);

    // iOS Permission
    const requestPermission = async () => {
        if (typeof (DeviceOrientationEvent as any).requestPermission === 'function') {
            try {
                const s = await (DeviceOrientationEvent as any).requestPermission();
                if (s === 'granted') {
                    setPermissionGranted(true);
                    alert('Motion Permission Granted!');
                } else {
                    alert('Permission Denied');
                }
            } catch (e) { alert(e); }
        } else {
            // Android / Non-iOS 13+
            setPermissionGranted(true);
        }
    };

    return (
        <div
            className="w-full h-screen bg-gray-900 overflow-hidden flex flex-col items-center justify-center touch-none select-none relative"
            onTouchStart={(e) => {
                e.preventDefault();
                handleTap();
            }}
        >
            {/* Status Indicator */}
            <div className="absolute top-4 left-4 text-xs font-mono text-gray-500">
                STATUS: <span className={status === 'Connected' ? 'text-green-500' : 'text-red-500'}>{status}</span>
            </div>

            {/* iOS Permission Button (Visible only if needed) */}
            {typeof (DeviceOrientationEvent as any).requestPermission === 'function' && !permissionGranted && (
                <button
                    onClick={(e) => {
                        e.stopPropagation(); // Prevent tap event
                        requestPermission();
                    }}
                    className="absolute top-20 left-1/2 transform -translate-x-1/2 bg-blue-600 text-white px-6 py-3 rounded-full shadow-lg z-50 animate-bounce"
                >
                    Enable Motion Control
                </button>
            )}

            {/* Main Visuals */}
            <div className="text-center pointer-events-none">
                <div className="text-6xl font-black text-white opacity-20 tracking-wider animate-pulse">
                    TAP
                </div>
                <div className="text-xl text-blue-400 opacity-40 mt-2 font-mono">
                    TILT & PLAY
                </div>
            </div>

            {/* Visual Tilt Indicator (Optional) */}
            <div className="absolute bottom-20 w-32 h-2 bg-gray-800 rounded-full overflow-hidden">
                <div className="h-full bg-blue-500 opacity-50 w-full transform origin-center scale-x-0" id="tilt-bar"></div>
            </div>

            {/* Tap Ripple Effect Container */}
            <div className="absolute inset-0 bg-white opacity-0 active:opacity-10 transition-opacity duration-75 pointer-events-none" />

            {/* Logs */}
            <div className="absolute bottom-2 left-2 right-2 pointer-events-none opacity-50">
                {logs.map((l, i) => <div key={i} className="text-[10px] text-yellow-500">{l}</div>)}
            </div>
        </div>
    );
};

export default ControllerPage;

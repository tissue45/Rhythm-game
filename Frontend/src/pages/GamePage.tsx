import React, { useEffect, useRef } from 'react';
import { io } from 'socket.io-client';

const GamePage: React.FC = () => {
    const iframeRef = useRef<HTMLIFrameElement>(null);

    useEffect(() => {
        // Redirect to homepage on page refresh
        const handleBeforeUnload = (e: BeforeUnloadEvent) => {
            // Mark that we're refreshing
            sessionStorage.setItem('isRefreshing', 'true');
        };

        // Check if this is a refresh
        if (sessionStorage.getItem('isRefreshing') === 'true') {
            sessionStorage.removeItem('isRefreshing');
            window.location.href = '/rhythmgame/';
            return;
        }

        window.addEventListener('beforeunload', handleBeforeUnload);

        // SocketIO 연결
        const socket = io("https://rhythm-game-website.onrender.com");

        socket.on("connect", () => {
            console.log("Connected to Relay Server via WebSocket");
            const gameWindow = iframeRef.current?.contentWindow as any;
            if (gameWindow && gameWindow.gameInstance) {
                gameWindow.gameInstance.SendMessage("NetworkManager", "ReceiveInput", "CONNECT");
            }
        });

        socket.on("game_input", (data: any) => {
            // Unity WebGL Iframe으로 메시지 전달
            const gameWindow = iframeRef.current?.contentWindow as any;
            if (gameWindow && gameWindow.gameInstance) {
                // data.lane: "0", "1", "2", "3"
                gameWindow.gameInstance.SendMessage("NetworkManager", "ReceiveInput", data.lane.toString());
            } else {
                console.warn("Game instance not found in Iframe");
            }
        });

        return () => {
            window.removeEventListener('beforeunload', handleBeforeUnload);
            socket.disconnect();
        };
    }, []);

    return (
        <div className="flex flex-col items-center justify-center h-screen overflow-hidden bg-black p-4">
            <h1 className="text-2xl text-white font-bold mb-2">Step up</h1>
            <div className="w-full max-w-[1280px] aspect-video relative group shadow-2xl">
                <button
                    onClick={() => {
                        const iframe = iframeRef.current;
                        if (iframe) {
                            if (iframe.requestFullscreen) {
                                iframe.requestFullscreen();
                            } else if ((iframe as any).webkitRequestFullscreen) {
                                (iframe as any).webkitRequestFullscreen();
                            } else if ((iframe as any).msRequestFullscreen) {
                                (iframe as any).msRequestFullscreen();
                            }
                        }
                    }}
                    className="absolute top-4 right-4 z-20 bg-black/50 hover:bg-purple-600 text-white p-2 rounded-lg transition-all duration-300 opacity-0 group-hover:opacity-100 backdrop-blur-sm border border-white/20"
                    title="Toggle Fullscreen"
                >
                    <svg xmlns="http://www.w3.org/2000/svg" className="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4 8V4m0 0h4M4 4l5 5m11-1V4m0 0h-4m4 0l-5 5M4 16v4m0 0h4m-4 0l5-5m11 5l-5-5m5 5v-4m0 4h-4" />
                    </svg>
                </button>
                <iframe
                    ref={iframeRef}
                    src={import.meta.env.VITE_GAME_URL || `${import.meta.env.BASE_URL}game/index.html`}
                    title="Rhythm Game"
                    width="100%"
                    height="100%"
                    className="w-full h-full border-0 rounded-lg shadow-[0_0_50px_rgba(168,85,247,0.2)] bg-black"
                    allowFullScreen
                />
            </div>
            <p className="text-gray-500 text-sm mt-2">
                모바일 컨트롤러의 QR 코드를 스캔하여 연결하세요!
            </p>
        </div>
    );
};

export default GamePage;

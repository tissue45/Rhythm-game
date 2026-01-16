import eventlet
eventlet.monkey_patch()

from flask import Flask, render_template, request
from flask_socketio import SocketIO
import socket
import qrcode
import io
import sys
import os

app = Flask(__name__)
socketio = SocketIO(app, cors_allowed_origins="*")

# Unity UDP 설정
UDP_IP = "127.0.0.1"
UDP_PORT = 7777
sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)

def get_all_ips():
    ip_list = []
    try:
        # 호스트 이름으로 모든 IP 조회
        hostname = socket.gethostname()
        for ip in socket.gethostbyname_ex(hostname)[2]:
            if not ip.startswith("127."):
                ip_list.append(ip)
    except:
        pass
    
    # 소켓 연결로 주 IP 확인 (기존 방식)
    try:
        s = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
        s.connect(("8.8.8.8", 80))
        primary_ip = s.getsockname()[0]
        s.close()
        if primary_ip not in ip_list:
            ip_list.insert(0, primary_ip)
    except:
        pass
        
    return ip_list if ip_list else ["127.0.0.1"]

@app.route('/')
def index():
    print(f"HTTP Request received from: {request.remote_addr}")
    return render_template('controller.html')

@socketio.on('connect')
def test_connect():
    print(f"Client connected: {request.remote_addr}")
    # 연결 시에는 아무것도 하지 않음 (클라이언트가 준비되면 client_ready 보냄)

@socketio.on('client_ready')
def handle_ready():
    print(f"Client Ready: {request.remote_addr}")
    try:
        sock.sendto(b"CONNECT", (UDP_IP, UDP_PORT))
        print("Sent CONNECT signal to Unity")
    except Exception as e:
        print(f"Failed to send CONNECT signal: {e}")

@socketio.on('pose_event')
def handle_pose(data):
    pose = data.get('pose')
    print(f"Pose detected: {pose}")
    
    # 포즈를 레인 인덱스로 매핑
    lane_map = {
        "O": "0",    # Red
        "X": "1",    # Green
        "HIGH": "2", # Blue
        "SIDE": "3"  # Yellow
    }
    
    if pose in lane_map:
        msg = lane_map[pose]
    if pose in lane_map:
        msg = lane_map[pose]
        # UDP 전송 (Unity Editor/PC 버전용)
        try:
            sock.sendto(msg.encode(), (UDP_IP, UDP_PORT))
        except:
            pass
            
        # Web Socket 전송 (WebGL 버전용)
        # 모든 클라이언트에게 game_input 이벤트 전송
        socketio.emit('game_input', {'lane': msg})

if __name__ == '__main__':
    all_ips = get_all_ips()
    port = int(os.environ.get("PORT", 5000))
    primary_ip = all_ips[0]
    import time
    url = f"https://{primary_ip}:{port}?v={int(time.time())}"
    
    print("\n" + "="*50)
    print(f"Web Controller Server Started!")
    print(f"Primary Access URL: {url}")
    print(f"Other detected IPs: {all_ips[1:]}")
    print("If the QR code doesn't work, try these IPs manually.")
    print("Scan the QR Code below with your phone:")
    print("="*50 + "\n")
    
    # QR 코드 생성 및 출력
    qr = qrcode.QRCode()
    qr.add_data(url)
    qr.make(fit=True)
    qr.print_ascii(invert=True)

    # QR 코드 이미지 저장 (Unity용)
    try:
        img = qr.make_image(fill_color="black", back_color="white")
        # 절대 경로로 저장 (사용자 환경에 맞게 수정 필요할 수 있음)
        save_path = r"c:\Users\user\Downloads\Rhythm_game (2)\Assets\Resources\qrcode.png"
        img.save(save_path)
        print(f"QR Code saved to: {save_path}")
    except Exception as e:
        print(f"Failed to save QR code image: {e}")

    # HTTPS 설정을 위한 인증서 생성 (Werkzeug 사용)
    from werkzeug.serving import make_ssl_devcert
    import os
    
    if not os.path.exists('cert.pem') or not os.path.exists('key.pem'):
        print("Generating self-signed SSL certificate...")
        make_ssl_devcert('./ssl', host='0.0.0.0')
        # make_ssl_devcert creates ssl.crt and ssl.key
        os.rename('ssl.crt', 'cert.pem')
        os.rename('ssl.key', 'key.pem')

    print("Starting server with HTTP (No SSL)...")
    socketio.run(app, host='0.0.0.0', port=port)

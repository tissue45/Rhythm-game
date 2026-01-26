import eventlet
eventlet.monkey_patch()

from flask import Flask, render_template, request, jsonify, send_from_directory
from flask_socketio import SocketIO, emit
from flask_cors import CORS
import socket
import qrcode
import io
import sys
import os

# Static Folder Path (Absolute)
base_dir = os.path.abspath(os.path.dirname(__file__))
static_dir = os.path.join(base_dir, 'Frontend', 'dist')

if not os.path.exists(static_dir):
    print(f"WARNING: Static folder not found at {static_dir}")
    print("Did you run 'npm run build' in Frontend?")

app = Flask(__name__, 
            static_url_path='',
            static_folder=static_dir,
            template_folder=static_dir)

# CORS 허용
CORS(app)

# SocketIO 설정
socketio = SocketIO(app, cors_allowed_origins="*", async_mode='eventlet')

# UDP 소켓 설정
UDP_IP = "127.0.0.1"
UDP_PORT = 5005
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
    full_path = os.path.join(app.static_folder, 'index.html')
    print(f"Attempting to serve: {full_path}")
    
    if not os.path.exists(full_path):
        # 404 발생 시 폴더 목록을 보여줘서 디버깅
        files = os.listdir(app.static_folder) if os.path.exists(app.static_folder) else f"Folder not found: {app.static_folder}"
        return f"CRITICAL ERROR: File not found at {full_path}. <br> Contents of {app.static_folder}: <br> {files}", 404
        
    try:
        with open(full_path, 'r', encoding='utf-8') as f:
            return f.read()
    except Exception as e:
        return f"Error reading file: {e}", 500

@app.route('/controller')
def controller():
    return index()

# Explicitly serve static assets if default handling fails
@app.route('/assets/<path:path>')
def serve_assets(path):
    return send_from_directory(os.path.join(app.static_folder, 'assets'), path)

# Catch-all for other paths (SPA support)
@app.route('/<path:path>')
def catch_all(path):
    # Check if it's a static file first
    if os.path.exists(os.path.join(app.static_folder, path)):
        return send_from_directory(app.static_folder, path)
    return index()

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
    
    # 포즈를 레인 인덱스로 매핑 (Legacy Support)
    lane_map = {
        "O": "0",    # Red
        "X": "1",    # Green
        "HIGH": "2", # Blue
        "SIDE": "3"  # Yellow
    }
    
    if pose in lane_map:
        msg = lane_map[pose]
        # UDP 전송 (Unity Editor/PC 버전용)
        try:
            sock.sendto(msg.encode(), (UDP_IP, UDP_PORT))
        except:
            pass
            
        # Web Socket 전송 (WebGL 버전용)
        socketio.emit('game_input', {'lane': msg})

@socketio.on('mobile_input')
def handle_mobile_input(data):
    """
    Handle generic mobile input (Tap, Tilt, Shake)
    Payload example: {'type': 'tap', 'lane': 0, 'timestamp': 123456}
    """
    import json
    # print(f"Mobile Input: {data}") # Log reduced for performance
    
    try:
        # Convert to JSON string
        json_msg = json.dumps(data)
        
        # Send to Unity via UDP
        sock.sendto(json_msg.encode(), (UDP_IP, UDP_PORT))
        
        # Broadcast to other web clients (if needed)
        socketio.emit('mobile_input_broadcast', data)
    except Exception as e:
        print(f"Error forwarding mobile input: {e}")

if __name__ == '__main__':
    all_ips = get_all_ips()
    port = int(os.environ.get("PORT", 5000))
    primary_ip = all_ips[0]
    
    print("\n" + "="*50)
    print(f"Web Controller Server Starting")
    print(f"Local URL: http://{primary_ip}:{port}/controller")
    
    # Ask for Tunnel URL to generate correct QR Code
    print("\n[OPTIONAL] Enter public Tunnel URL (e.g. https://xxxx.loca.lt) to update QR Code.")
    print("Press ENTER to skip and use Local URL.")
    tunnel_url = input("Tunnel URL > ").strip()
    
    final_url = f"{tunnel_url}/controller" if tunnel_url else f"http://{primary_ip}:{port}/controller"
    
    print(f"\nFinal Controller URL: {final_url}")
    print("="*50 + "\n")

    # Generate QR Code
    qr = qrcode.QRCode()
    qr.add_data(final_url)
    qr.make(fit=True)
    
    try:
        img = qr.make_image(fill_color="black", back_color="white")
        save_path = os.path.join(base_dir, 'Assets', 'Resources', 'qrcode.png')
        
        # Resources 폴더가 없으면 생성 시도
        os.makedirs(os.path.dirname(save_path), exist_ok=True)
        
        img.save(save_path)
        print(f"QR Code saved to: {save_path}")
    except Exception as e: 
        print(f"Failed to save QR Code: {e}")

    # 3. 서버 실행 (HTTP)
    print(f"Listening on 0.0.0.0:{port} (HTTP)...")
    try:
        socketio.run(app, host='0.0.0.0', port=port, debug=True, use_reloader=False) 
    except Exception as e:
        print(f"CRITICAL ERROR: {e}")
        input("Press Enter to exit...")

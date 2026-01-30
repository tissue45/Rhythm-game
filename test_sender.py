import socket
import time
import random

UDP_IP = "127.0.0.1"
UDP_PORT = 7777

sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)

print(f"Sending UDP packets to {UDP_IP}:{UDP_PORT}")
print("Press Ctrl+C to stop.")

try:
    while True:
        # 0~3 사이의 랜덤한 레인 번호 전송
        lane = random.randint(0, 3)
        message = str(lane).encode()
        sock.sendto(message, (UDP_IP, UDP_PORT))
        print(f"Sent: {lane}")
        
        # 0.5초마다 전송 (너무 빠르면 정신없으니까)
        time.sleep(0.5)
except KeyboardInterrupt:
    print("Test stopped.")

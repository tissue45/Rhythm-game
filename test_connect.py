import socket

UDP_IP = "127.0.0.1"
UDP_PORT = 7777

sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
sock.sendto(b"CONNECT", (UDP_IP, UDP_PORT))
print("Sent CONNECT signal to Unity")

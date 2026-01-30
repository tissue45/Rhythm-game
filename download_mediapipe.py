import os
import requests

BASE_DIR = os.path.dirname(os.path.abspath(__file__))
STATIC_DIR = os.path.join(BASE_DIR, "static", "mediapipe")

# MediaPipe Pose files
POSE_URL = "https://cdn.jsdelivr.net/npm/@mediapipe/pose/"
POSE_FILES = [
    "pose.js",
    "pose_solution_packed_assets_loader.js",
    "pose_solution_simd_wasm_bin.js",
    "pose_solution_wasm_bin.js",
    "pose_solution_packed_assets.data",
    "pose_landmark_full.tflite",
    "pose_landmark_lite.tflite",
    "pose_landmark_heavy.tflite"
]

# Camera Utils
CAMERA_URL = "https://cdn.jsdelivr.net/npm/@mediapipe/camera_utils/"
CAMERA_FILES = ["camera_utils.js"]

# Drawing Utils
DRAWING_URL = "https://cdn.jsdelivr.net/npm/@mediapipe/drawing_utils/"
DRAWING_FILES = ["drawing_utils.js"]

def download_file(url, filename):
    save_path = os.path.join(STATIC_DIR, filename)
    if os.path.exists(save_path):
        print(f"Skipping {filename} (already exists)")
        return

    print(f"Downloading {filename}...")
    try:
        response = requests.get(url + filename)
        if response.status_code == 200:
            with open(save_path, 'wb') as f:
                f.write(response.content)
            print(f"Saved {filename}")
        else:
            print(f"Failed to download {filename}: {response.status_code}")
    except Exception as e:
        print(f"Error downloading {filename}: {e}")

if __name__ == "__main__":
    if not os.path.exists(STATIC_DIR):
        os.makedirs(STATIC_DIR)
        print(f"Created directory: {STATIC_DIR}")

    print("Downloading MediaPipe Pose files...")
    for f in POSE_FILES:
        download_file(POSE_URL, f)

    print("Downloading Camera Utils...")
    for f in CAMERA_FILES:
        download_file(CAMERA_URL, f)

    print("Downloading Drawing Utils...")
    for f in DRAWING_FILES:
        download_file(DRAWING_URL, f)

    print("Download complete!")

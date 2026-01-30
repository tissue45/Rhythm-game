# Assets\scripts\SchoolLobbyManager.cs 파일 삭제 또는 교체

import shutil
import os

# 소문자 scripts 폴더의 파일 (문제 파일)
problem_file = r"c:\Users\user\Downloads\Rhythm_game (2)\Assets\scripts\SchoolLobbyManager.cs"

# 대문자 Scripts 폴더의 파일 (정상 파일)
correct_file = r"c:\Users\user\Downloads\Rhythm_game (2)\Assets\Scripts\SchoolLobbyManager.cs"

# 백업
backup_file = problem_file + ".backup"

try:
    # 1. 문제 파일 백업
    if os.path.exists(problem_file):
        shutil.copy2(problem_file, backup_file)
        print(f"✓ Backed up: {backup_file}")
    
    # 2. 정상 파일로 교체
    if os.path.exists(correct_file):
        shutil.copy2(correct_file, problem_file)
        print(f"✓ Replaced with correct file")
    else:
        # 정상 파일이 없으면 문제 파일 삭제
        os.remove(problem_file)
        print(f"✓ Removed problem file")
    
    print("\n✓ Fixed! Unity should compile now.")
    print("  Please return to Unity and wait for recompilation.")
    
except Exception as e:
    print(f"✗ Error: {e}")

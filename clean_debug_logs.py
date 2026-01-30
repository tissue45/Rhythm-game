# 모든 개선 스크립트의 Debug.Log 제거

import os
import re

scripts_to_clean = [
    r"c:\Users\user\Downloads\Rhythm_game (2)\Assets\Scripts\MainSceneButtonEnhancer.cs",
    r"c:\Users\user\Downloads\Rhythm_game (2)\Assets\Scripts\MainMenuEnhancer.cs",
    r"c:\Users\user\Downloads\Rhythm_game (2)\Assets\Scripts\CharacterLightingEnhancer.cs",
    r"c:\Users\user\Downloads\Rhythm_game (2)\Assets\Scripts\FloorMaterialFixer.cs",
    r"c:\Users\user\Downloads\Rhythm_game (2)\Assets\Scripts\PostProcessingSetup.cs",
]

def clean_debug_logs(file_path):
    if not os.path.exists(file_path):
        print(f"⚠ File not found: {file_path}")
        return False
    
    try:
        with open(file_path, 'r', encoding='utf-8') as f:
            content = f.read()
        
        original_lines = len(content.split('\n'))
        
        # Remove Debug.Log lines (but keep Debug.LogError and Debug.LogWarning for important stuff)
        content = re.sub(r'^\s*Debug\.Log\(.*?\);\s*$', '', content, flags=re.MULTILINE)
        content = re.sub(r'^\s*Debug\.Log\(.*?\);\s*\n', '', content, flags=re.MULTILINE)
        
        # Remove empty lines that were left
        lines = content.split('\n')
        cleaned_lines = []
        prev_empty = False
        for line in lines:
            if line.strip() == '':
                if not prev_empty:
                    cleaned_lines.append(line)
                    prev_empty = True
            else:
                cleaned_lines.append(line)
                prev_empty = False
        
        content = '\n'.join(cleaned_lines)
        
        # Change showDebugLogs default to false
        content = re.sub(r'public bool showDebugLogs = true;', 'public bool showDebugLogs = false;', content)
        
        with open(file_path, 'w', encoding='utf-8') as f:
            f.write(content)
        
        new_lines = len(content.split('\n'))
        removed = original_lines - new_lines
        
        filename = os.path.basename(file_path)
        print(f"✓ {filename}: Removed {removed} debug log lines")
        return True
        
    except Exception as e:
        print(f"✗ Error cleaning {file_path}: {e}")
        return False

print("Cleaning debug logs from enhancement scripts...\n")

cleaned_count = 0
for script_path in scripts_to_clean:
    if clean_debug_logs(script_path):
        cleaned_count += 1

print(f"\n✓ Cleaned {cleaned_count}/{len(scripts_to_clean)} files")
print("  Console will be much cleaner now!")

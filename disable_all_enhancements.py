# 모든 UI 개선 스크립트를 비활성화

import os

scripts_to_disable = [
    r"c:\Users\user\Downloads\Rhythm_game (2)\Assets\Scripts\MainSceneButtonEnhancer.cs",
    r"c:\Users\user\Downloads\Rhythm_game (2)\Assets\Scripts\MainMenuEnhancer.cs",
    r"c:\Users\user\Downloads\Rhythm_game (2)\Assets\Scripts\RhythmGameButtonStyle.cs",
    r"c:\Users\user\Downloads\Rhythm_game (2)\Assets\Scripts\UIButtonHover.cs",
    r"c:\Users\user\Downloads\Rhythm_game (2)\Assets\Scripts\CharacterLightingEnhancer.cs",
    r"c:\Users\user\Downloads\Rhythm_game (2)\Assets\Scripts\FloorMaterialFixer.cs",
    r"c:\Users\user\Downloads\Rhythm_game (2)\Assets\Scripts\PostProcessingSetup.cs",
]

def disable_script(file_path):
    if not os.path.exists(file_path):
        return False
    
    try:
        # Rename to .cs.disabled
        new_path = file_path + ".disabled"
        os.rename(file_path, new_path)
        print(f"✓ Disabled: {os.path.basename(file_path)}")
        return True
    except Exception as e:
        print(f"✗ Error: {e}")
        return False

print("Disabling all UI enhancement scripts...\n")

disabled_count = 0
for script in scripts_to_disable:
    if disable_script(script):
        disabled_count += 1

print(f"\n✓ Disabled {disabled_count} scripts")
print("\nNow Unity will use the original button design.")
print("Play mode should be consistent now!")

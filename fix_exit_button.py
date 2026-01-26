import re

file_path = r"c:\Users\user\Downloads\Rhythm_game (2)\Assets\Scripts\SchoolLobbyManager.cs"

# Read with error handling
try:
    with open(file_path, 'rb') as f:
        raw_data = f.read()
    
    # Try UTF-8 first, then fall back to latin-1 (which never fails)
    try:
        content = raw_data.decode('utf-8')
    except:
        content = raw_data.decode('latin-1')
    
    print(f"✓ File read successfully ({len(content)} characters)")
    
    # Find and replace the Btn_Exit binding
    old_line = 'BindButton("Btn_Exit", () => Application.Quit());'
    
    new_line = '''BindButton("Btn_Exit", () => {
            Debug.Log("[EXIT] Quitting game...");
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #else
            Application.Quit();
            #endif
        });'''
    
    if old_line in content:
        content = content.replace(old_line, new_line)
        print("✓ EXIT button binding updated")
        
        # Write back
        with open(file_path, 'wb') as f:
            f.write(content.encode('utf-8'))
        
        print("\n✓ SchoolLobbyManager.cs updated successfully!")
        print("  - EXIT button now works in Unity Editor")
        print("  - EXIT button will quit game in builds")
    else:
        print("⚠ Pattern not found, searching for similar patterns...")
        # Search for the line
        for i, line in enumerate(content.split('\n'), 1):
            if 'Btn_Exit' in line and 'Application.Quit' in line:
                print(f"  Found at line {i}: {line.strip()}")
        
except Exception as e:
    print(f"✗ Error: {e}")

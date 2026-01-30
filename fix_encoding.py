file_path = r"c:\Users\user\Downloads\Rhythm_game (2)\Assets\Scripts\SchoolLobbyManager.cs"

try:
    with open(file_path, 'rb') as f:
        raw = f.read()
    
    # Decode
    try:
        content = raw.decode('utf-8')
    except:
        content = raw.decode('latin-1')
    
    # Fix line 473 - remove problematic Korean comment
    lines = content.split('\n')
    
    # Replace line 472 (index 472, which is line 473)
    if len(lines) > 472:
        old_line = lines[472]
        # Replace with simple English comment
        lines[472] = "        // [DEBUG] Mouse click debug log (UI debugging)"
        print(f"Fixed line 473:")
        print(f"  Old: {old_line[:50]}...")
        print(f"  New: {lines[472]}")
    
    # Write back
    new_content = '\n'.join(lines)
    with open(file_path, 'w', encoding='utf-8') as f:
        f.write(new_content)
    
    print("\n✓ Fixed encoding issue in SchoolLobbyManager.cs!")
    print("  Unity should compile now.")
    
except Exception as e:
    print(f"✗ Error: {e}")

file_path = r"c:\Users\user\Downloads\Rhythm_game (2)\Assets\scripts\SchoolLobbyManager.cs"

try:
    with open(file_path, 'rb') as f:
        raw = f.read()
    
    # Try UTF-8, fallback to latin-1
    try:
        content = raw.decode('utf-8')
    except:
        content = raw.decode('latin-1')
    
    lines = content.split('\n')
    
    print(f"Total lines: {len(lines)}")
    print(f"\nLines 468-478:")
    for i in range(468, min(478, len(lines))):
        print(f"{i+1}: {lines[i]}")
    
    # Find lines with ?? or syntax errors
    print(f"\n\nSearching for problematic lines around 471...")
    for i in range(465, min(480, len(lines))):
        line = lines[i]
        if '??' in line or 'if' in line and not line.strip().startswith('//'):
            print(f"{i+1}: {line}")
            
except Exception as e:
    print(f"Error: {e}")

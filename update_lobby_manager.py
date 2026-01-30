import chardet

# Detect encoding
file_path = r"c:\Users\user\Downloads\Rhythm_game (2)\Assets\Scripts\SchoolLobbyManager.cs"

with open(file_path, 'rb') as f:
    raw_data = f.read()
    result = chardet.detect(raw_data)
    print(f"Detected encoding: {result['encoding']} (confidence: {result['confidence']})")

# Try to read with detected encoding
try:
    content = raw_data.decode(result['encoding'])
    print(f"✓ Successfully decoded with {result['encoding']}")
    print(f"File length: {len(content)} characters")
    
    # Now make the replacements
    # Replacement 1: Adjust logo position
    old_logo_pos = "logoRect.anchoredPosition = new Vector2(0, 0);\n            logoRect.localScale = Vector3.one * 1.5f;"
    
    new_logo_pos = "// [ENHANCED] Move logo to upper-center for better layout\n            logoRect.anchoredPosition = new Vector2(0, 150); // Upper-center position\n            logoRect.localScale = Vector3.one * 1.2f; // Slightly smaller for breathing room"
    
    if old_logo_pos in content:
        content = content.replace(old_logo_pos, new_logo_pos)
        print("✓ Logo position updated")
    else:
        print("⚠ Logo position pattern not found")
    
    # Replacement 2: Add ButtonStyleEnhancer
    old_version = "// [NEW] Create Version Display        \n            CreateVersionDisplay();\n        }\n    }"
    
    new_version = """// [NEW] Create Version Display        
            CreateVersionDisplay();

            // [ENHANCED] Apply button style enhancements
            ButtonStyleEnhancer styleEnhancer = gameObject.GetComponent<ButtonStyleEnhancer>();
            if (styleEnhancer == null)
            {
                styleEnhancer = gameObject.AddComponent<ButtonStyleEnhancer>();
                Debug.Log("[SchoolLobbyManager] Added ButtonStyleEnhancer to enhance all lobby buttons");
            }
        }
    }"""
    
    if old_version in content:
        content = content.replace(old_version, new_version)
        print("✓ ButtonStyleEnhancer integration added")
    else:
        print("⚠ Version display pattern not found")
    
    # Write back with the same encoding
    with open(file_path, 'wb') as f:
        f.write(content.encode(result['encoding']))
    
    print("\n✓ SchoolLobbyManager.cs updated successfully!")
    
except Exception as e:
    print(f"✗ Error: {e}")

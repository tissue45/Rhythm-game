import os

input_file = "Frontend/public/game/Build/game.data"
chunk_size = 50 * 1024 * 1024 # 50MB

if not os.path.exists(input_file):
    print(f"File not found: {input_file}")
    exit(1)

file_number = 0
with open(input_file, 'rb') as f:
    while True:
        chunk = f.read(chunk_size)
        if not chunk:
            break
        
        # Determine suffix like 'aa', 'ab', 'ac' ...
        # 'aa' is 0, 'ab' is 1... basic base 26 logic or just mimic split
        # standard split uses aa, ab, ac...
        first_char = chr(ord('a') + (file_number // 26))
        second_char = chr(ord('a') + (file_number % 26))
        suffix = f"{first_char}{second_char}"
        
        output_filename = f"{input_file}.part{suffix}"
        
        with open(output_filename, 'wb') as out_f:
            out_f.write(chunk)
            
        print(f"Created {output_filename}")
        file_number += 1

print("Done splitting.")

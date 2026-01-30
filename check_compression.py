import gzip
import os

input_file = "Frontend/public/game/Build/game.data"
output_file = "Frontend/public/game/Build/game.data.gz.test"

if os.path.exists(input_file):
    original_size = os.path.getsize(input_file)
    print(f"Original size: {original_size / (1024*1024):.2f} MB")
    
    with open(input_file, 'rb') as f_in:
        with gzip.open(output_file, 'wb') as f_out:
            f_out.writelines(f_in)
            
    compressed_size = os.path.getsize(output_file)
    print(f"Compressed size: {compressed_size / (1024*1024):.2f} MB")
    
    os.remove(output_file)
else:
    print("File not found")

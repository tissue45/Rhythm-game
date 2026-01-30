import os

source_file = r"Frontend\public\game\Build\game.data"
output_dir = r"docs\game\Build"
chunk_size = 90 * 1024 * 1024  # 90MB

def split_file():
    if not os.path.exists(output_dir):
        os.makedirs(output_dir)
    
    file_number = 1
    with open(source_file, 'rb') as src:
        while True:
            chunk = src.read(chunk_size)
            if not chunk:
                break
            part_name = f"game.data.part{file_number}"
            part_path = os.path.join(output_dir, part_name)
            with open(part_path, 'wb') as part:
                part.write(chunk)
            print(f"Created {part_name}")
            file_number += 1

if __name__ == "__main__":
    split_file()

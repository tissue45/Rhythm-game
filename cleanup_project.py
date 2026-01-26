import os

# Unity Project Assets Path
project_path = r"c:\Users\user\Downloads\Rhythm_game (2)\Assets"

def delete_disabled_files(root_dir):
    deleted_count = 0
    for root, dirs, files in os.walk(root_dir):
        for file in files:
            if file.endswith(".disabled"):
                file_path = os.path.join(root, file)
                try:
                    os.remove(file_path)
                    print(f"Deleted: {file}")
                    deleted_count += 1
                    
                    # Try to delete associated .meta file if it exists
                    meta_path = file_path.replace(".disabled", ".meta") # Usually meta is for the original file, but check carefully
                    # Actually, if we renamed .cs to .cs.disabled, there might be a .cs.disabled.meta or the original .cs.meta is now widowed.
                    # Let's clean up widowed .meta files for the original .cs names if they exist
                    
                    original_cs_path = file_path[:-9] # remove .disabled
                    original_meta = original_cs_path + ".meta"
                    if os.path.exists(original_meta):
                         os.remove(original_meta)
                         print(f"Deleted Meta: {os.path.basename(original_meta)}")

                    # Also check for .cs.disabled.meta
                    disabled_meta = file_path + ".meta"
                    if os.path.exists(disabled_meta):
                        os.remove(disabled_meta)
                        print(f"Deleted Meta: {os.path.basename(disabled_meta)}")
                        
                except Exception as e:
                    print(f"Error deleting {file}: {e}")
    return deleted_count

print("Starting cleanup of disabled scripts...")
count = delete_disabled_files(project_path)
print(f"\nCleanup complete. Removed {count} disabled files.")

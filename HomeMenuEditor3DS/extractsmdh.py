import struct
import os
from PIL import Image
from concurrent.futures import ThreadPoolExecutor, as_completed

def find_smdh_offset(file_path):
    try:
        with open(file_path, 'rb') as f:
            # Search for the "SMDH" magic string (4 bytes)
            magic_string = b"SMDH"
            file_data = f.read()
            
            # Find the first occurrence of the magic string and return the offset
            offset = file_data.find(magic_string)
            if offset == -1:
                raise ValueError("Magic string 'SMDH' not found in the file.")
            return offset
    except Exception as e:
        print(f"Error finding SMDH offset: {e}")
        raise

def extract_smdh(file_path, smdh_offset, smdh_size=0x36B0):
    try:
        with open(file_path, 'rb') as f:
            # Seek to the location of the first "SMDH" magic string
            f.seek(smdh_offset)  # SMDH header starts at the found offset
            smdh_data = f.read(smdh_size)  # Read the full SMDH data (14,016 bytes)
        
        return smdh_data
    except Exception as e:
        print(f"Error extracting SMDH: {e}")
        raise

def save_smdh_to_file(smdh_data, title_id, output_dir):
    try:
        # Create the output directory if it doesn't exist
        if not os.path.exists(output_dir):
            os.makedirs(output_dir)
        
        # Save the SMDH data with the title ID as the filename
        output_file = os.path.join(output_dir, f"{title_id}.smdh")
        with open(output_file, 'wb') as f:
            f.write(smdh_data)
        print(f"SMDH saved to {output_file}")
    except Exception as e:
        print(f"Error saving SMDH to file: {e}")
        raise

def process_file(input_file, output_dir):
    try:
        # Extract the first 16 characters of the file name as the title ID
        title_id = os.path.basename(input_file)[:16]
        print(f"Processing file: {input_file}")
        
        # Step 1: Find the magic string and get the offset
        smdh_offset = find_smdh_offset(input_file)
        print(f"SMDH magic string found at offset {smdh_offset:X}")

        # Step 2: Extract the SMDH data (14,016 bytes, size adjusted)
        smdh_data = extract_smdh(input_file, smdh_offset, smdh_size=0x36C0)
        
        # Step 3: Save the SMDH data to a file
        save_smdh_to_file(smdh_data, title_id, output_dir)
    except ValueError as e:
        print(f"Error processing {input_file}: {e}")
    except Exception as e:
        print(f"Unexpected error with {input_file}: {e}")

def process_directory(input_dir, output_dir, num_threads=4):
    try:
        # Ensure the output directory exists
        if not os.path.exists(output_dir):
            os.makedirs(output_dir)
        
        # List all the files in the directory
        files_to_process = [os.path.join(input_dir, filename) for filename in os.listdir(input_dir) if filename.endswith('.cia')]
        
        # Use a ThreadPoolExecutor to process the files concurrently
        with ThreadPoolExecutor(max_workers=num_threads) as executor:
            futures = [executor.submit(process_file, file, output_dir) for file in files_to_process]
            
            # Wait for all threads to complete
            for future in as_completed(futures):
                future.result()  # This will raise any exceptions that occurred in the thread
    except Exception as e:
        print(f"Error processing the directory: {e}")
        raise

def main():
    input_dir = os.getcwd()  # current directory
    output_dir = os.path.join(os.getcwd(), 'smdh')  
    num_threads = 4 
    
    try:
        # Process the directory and extract SMDH data from each .cia file
        process_directory(input_dir, output_dir, num_threads)
    except Exception as e:
        print(f"Error in main execution: {e}")

if __name__ == '__main__':
    main()


import os
import sys
from pyctr.type.smdh import SMDH
from PIL import Image

def extract_icons_from_directory(directory):
    try:
        for filename in os.listdir(directory):
            if filename.endswith(".smdh"):
                smdh_file_path = os.path.join(directory, filename)
                extract_icons(smdh_file_path)
    except Exception as e:
        print(f"Error while scanning directory: {e}")

def extract_icons(smdh_file_path):
    try:
        # Load SMDH file
        with open(smdh_file_path, 'rb') as f:
            smdh = SMDH.load(f)

        # Debug prints
        print(f"Small icon array dimensions: {len(smdh.icon_small_array)} x {len(smdh.icon_small_array[0])}")
        print(f"Large icon array dimensions: {len(smdh.icon_large_array)} x {len(smdh.icon_large_array[0])}")

        # Extract small icon
        if smdh.icon_small_array:
            save_image(smdh.icon_small_array, smdh_file_path, "_smallicon")
        else:
            print(f"No small icon found in {os.path.basename(smdh_file_path)}")

        # Extract large icon
        if smdh.icon_large_array:
            save_image(smdh.icon_large_array, smdh_file_path, "_largeicon")
        else:
            print(f"No large icon found in {os.path.basename(smdh_file_path)}")

        print(f"Icons extracted from {os.path.basename(smdh_file_path)} successfully.")
    except Exception as e:
        print(f"Error while extracting icons from {os.path.basename(smdh_file_path)}: {e}")

def save_image(icon_array, smdh_file_path, suffix):
    # Create image file name
    file_name = os.path.splitext(os.path.basename(smdh_file_path))[0] + suffix + ".png"
    file_path = os.path.join(os.path.dirname(smdh_file_path), file_name)

    # Convert icon array to image
    img = Image.new('RGB', (48, 48))
    pixels = img.load()
    for y in range(48):
        for x in range(48):
            pixels[x, y] = icon_array[y][x]

    # Save image
    img.save(file_path)
    print(f"Image saved as: {file_path}")

if __name__ == "__main__":
    if len(sys.argv) == 1:
        directory = os.getcwd()
    elif len(sys.argv) == 2:
        directory = sys.argv[1]
        if not os.path.isdir(directory):
            print("Error: Invalid directory.")
            sys.exit(1)
    else:
        print("Usage: python extract_icons.py <directory>")
        sys.exit(1)

    extract_icons_from_directory(directory)

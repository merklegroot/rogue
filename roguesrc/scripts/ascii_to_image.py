from PIL import Image, ImageDraw
import os

# Constants from drawUtil.cs
SIDE_PADDING = 8
TOP_PADDING = 10
CHAR_WIDTH = 8
CHAR_HEIGHT = 14
CHAR_H_GAP = 1
CHAR_V_GAP = 2

def get_char_rect(char_num):
    """Get the rectangle coordinates for a character in the CP437 charset."""
    source_x = char_num % 32
    source_y = char_num // 32
    
    x = SIDE_PADDING + (source_x * (CHAR_WIDTH + CHAR_H_GAP))
    y = TOP_PADDING + (source_y * (CHAR_HEIGHT + CHAR_V_GAP))
    
    return (x, y, x + CHAR_WIDTH, y + CHAR_HEIGHT)

def draw_char(draw, char_num, x, y):
    """Draw a CP437 character at the specified position."""
    # Load the CP437 charset
    charset = Image.open('../RogueLib/images/Codepage-437-transparent.png')
    
    # Get the character from the charset
    char_rect = get_char_rect(char_num)
    char_img = charset.crop(char_rect)
    
    # Paste the character onto the target image
    draw.bitmap((x, y), char_img)

def create_skull_image():
    # Read the skull pattern from file
    with open('data/skull.txt', 'r') as f:
        skull_lines = f.readlines()
    
    # Remove empty lines and strip whitespace
    skull_lines = [line.rstrip() for line in skull_lines if line.strip()]
    
    # Calculate dimensions
    width = max(len(line) for line in skull_lines)
    height = len(skull_lines)
    
    # Create a new image with transparent background
    image = Image.new('RGBA', (width * (CHAR_WIDTH + CHAR_H_GAP), height * (CHAR_HEIGHT + CHAR_V_GAP)), (0, 0, 0, 0))
    draw = ImageDraw.Draw(image)
    
    # Convert ASCII characters to CP437 characters
    char_map = {
        '_': 0xCD,  # ═
        '/': 0x2F,  # /
        '\\': 0x5C,  # \
        '|': 0xBA,  # ║
        '(': 0x28,  # (
        ')': 0x29,  # )
        '^': 0x5E,  # ^
        ' ': 0,     # space
    }
    
    # Draw each character
    for y, line in enumerate(skull_lines):
        for x, char in enumerate(line):
            if char in char_map:
                draw_char(draw, char_map[char], 
                         x * (CHAR_WIDTH + CHAR_H_GAP), 
                         y * (CHAR_HEIGHT + CHAR_V_GAP))
    
    # Save the image
    output_path = '../RogueLib/images/skull.png'
    image.save(output_path)
    print(f"Skull image saved to {output_path}")

def create_sword_image():
    # Read the sword pattern from file
    with open('data/sword.txt', 'r') as f:
        sword_lines = f.readlines()
    
    # Remove empty lines and strip whitespace
    sword_lines = [line.rstrip() for line in sword_lines if line.strip()]
    
    # Calculate dimensions
    width = max(len(line) for line in sword_lines)
    height = len(sword_lines)
    
    # Create a new image with transparent background
    image = Image.new('RGBA', (width * (CHAR_WIDTH + CHAR_H_GAP), height * (CHAR_HEIGHT + CHAR_V_GAP)), (0, 0, 0, 0))
    draw = ImageDraw.Draw(image)
    
    # Convert ASCII characters to CP437 characters
    char_map = {
        '^': 0x5E,  # ^
        '|': 0xBA,  # ║
        '=': 0xCD,  # ═
        ' ': 0,     # space
    }
    
    # Draw each character
    for y, line in enumerate(sword_lines):
        for x, char in enumerate(line):
            if char in char_map:
                draw_char(draw, char_map[char], 
                         x * (CHAR_WIDTH + CHAR_H_GAP), 
                         y * (CHAR_HEIGHT + CHAR_V_GAP))
    
    # Save the image
    output_path = '../RogueLib/images/sword.png'
    image.save(output_path)
    print(f"Sword image saved to {output_path}")

if __name__ == "__main__":
    create_skull_image()
    create_sword_image() 
import random

def generate_random_map(width=40, height=20):
    """
    Generate a random map with a single rectangular room.
    
    Args:
        width (int): Width of the map
        height (int): Height of the map
    
    Returns:
        list: 2D list representing the map
    """
    # Initialize empty map with spaces
    map_grid = [[' ' for _ in range(width)] for _ in range(height)]
    
    # Generate random room dimensions (ensuring minimum size of 3x3)
    room_width = random.randint(5, min(15, width - 4))
    room_height = random.randint(4, min(8, height - 4))
    
    # Generate random room position (ensuring it fits within map boundaries)
    room_x = random.randint(1, width - room_width - 1)
    room_y = random.randint(1, height - room_height - 1)
    
    # Draw the room walls
    for x in range(room_x, room_x + room_width):
        for y in range(room_y, room_y + room_height):
            # Corners
            if x == room_x and y == room_y:
                map_grid[y][x] = '╔'
            elif x == room_x + room_width - 1 and y == room_y:
                map_grid[y][x] = '╗'
            elif x == room_x and y == room_y + room_height - 1:
                map_grid[y][x] = '╚'
            elif x == room_x + room_width - 1 and y == room_y + room_height - 1:
                map_grid[y][x] = '╝'
            # Horizontal walls
            elif y == room_y or y == room_y + room_height - 1:
                map_grid[y][x] = '═'
            # Vertical walls
            elif x == room_x or x == room_x + room_width - 1:
                map_grid[y][x] = '║'
            # Floor
            else:
                map_grid[y][x] = '.'
    
    # Convert map to string
    map_str = '\n'.join([''.join(row) for row in map_grid])
    return map_str

def save_map_to_file(map_str, filename="../resources/map.txt"):
    """Save the generated map to a file"""
    with open(filename, 'w') as f:
        f.write(map_str)

if __name__ == "__main__":
    random_map = generate_random_map()
    print(random_map)
    save_map_to_file(random_map)
    print(f"Map saved to RogueLib/resources/map.txt")
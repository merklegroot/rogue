import random

def generate_random_map(width=40, height=20):
    """
    Generate a random map with two rectangular rooms with doors.
    
    Args:
        width (int): Width of the map
        height (int): Height of the map
    
    Returns:
        str: String representation of the map
    """
    # Initialize empty map with spaces
    map_grid = [[' ' for _ in range(width)] for _ in range(height)]
    
    # Generate first room
    room1_width = random.randint(7, min(15, width - 4))
    room1_height = random.randint(5, min(8, height - 4))
    room1_x = random.randint(1, width // 2 - room1_width - 1)
    room1_y = random.randint(1, height - room1_height - 1)
    
    # Draw the first room
    draw_room(map_grid, room1_x, room1_y, room1_width, room1_height)
    
    # Generate second room (ensuring it doesn't overlap with the first)
    room2_width = random.randint(7, min(15, width - 4))
    room2_height = random.randint(5, min(8, height - 4))
    
    # Try to place the second room in the right half of the map
    max_attempts = 20
    for _ in range(max_attempts):
        room2_x = random.randint(width // 2, width - room2_width - 1)
        room2_y = random.randint(1, height - room2_height - 1)
        
        # Check if rooms overlap
        if not rooms_overlap(room1_x, room1_y, room1_width, room1_height,
                            room2_x, room2_y, room2_width, room2_height):
            break
    
    # Draw the second room
    draw_room(map_grid, room2_x, room2_y, room2_width, room2_height)
    
    # Convert map to string
    map_str = '\n'.join([''.join(row) for row in map_grid])
    return map_str

def draw_room(map_grid, room_x, room_y, room_width, room_height):
    """
    Draw a rectangular room on the map grid and add doors.
    
    Args:
        map_grid (list): 2D list representing the map
        room_x (int): X coordinate of the top-left corner of the room
        room_y (int): Y coordinate of the top-left corner of the room
        room_width (int): Width of the room
        room_height (int): Height of the room
    """
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
    
    # Add doors to the walls
    add_doors_to_room(map_grid, room_x, room_y, room_width, room_height)

def rooms_overlap(x1, y1, w1, h1, x2, y2, w2, h2, padding=1):
    """
    Check if two rooms overlap (including a padding space between them).
    
    Args:
        x1, y1: Top-left coordinates of first room
        w1, h1: Width and height of first room
        x2, y2: Top-left coordinates of second room
        w2, h2: Width and height of second room
        padding: Minimum space between rooms
        
    Returns:
        bool: True if rooms overlap, False otherwise
    """
    # Check if one room is to the left of the other
    if x1 + w1 + padding <= x2 or x2 + w2 + padding <= x1:
        return False
    
    # Check if one room is above the other
    if y1 + h1 + padding <= y2 or y2 + h2 + padding <= y1:
        return False
    
    # If neither of the above, the rooms overlap
    return True

def add_doors_to_room(map_grid, room_x, room_y, room_width, room_height):
    """
    Add doors to the walls of a room.
    Doors are more likely to spawn near the center of walls.
    Proximity of other doors reduces the chance of another door.
    Doors may not be placed directly adjacent to another door.
    
    Args:
        map_grid (list): 2D list representing the map
        room_x (int): X coordinate of the top-left corner of the room
        room_y (int): Y coordinate of the top-left corner of the room
        room_width (int): Width of the room
        room_height (int): Height of the room
    """
    # Define door character
    door_char = '╬'
    
    # Track door positions for proximity calculations
    door_positions = []
    
    # Process each wall
    walls = [
        # Top wall (excluding corners)
        [(room_x + i, room_y) for i in range(1, room_width - 1)],
        # Bottom wall (excluding corners)
        [(room_x + i, room_y + room_height - 1) for i in range(1, room_width - 1)],
        # Left wall (excluding corners)
        [(room_x, room_y + i) for i in range(1, room_height - 1)],
        # Right wall (excluding corners)
        [(room_x + room_width - 1, room_y + i) for i in range(1, room_height - 1)]
    ]
    
    for wall in walls:
        # Determine number of doors for this wall (0-2)
        num_doors = random.choices([0, 1, 2], weights=[0.3, 0.5, 0.2])[0]
        
        if num_doors > 0 and len(wall) > 0:
            # Calculate center of the wall
            center_idx = len(wall) // 2
            
            # Calculate weights based on distance from center
            weights = []
            valid_positions = []
            
            for i, pos in enumerate(wall):
                x, y = pos
                
                # Check if position is adjacent to an existing door
                adjacent_to_door = False
                for door_x, door_y in door_positions:
                    dx = abs(x - door_x)
                    dy = abs(y - door_y)
                    if dx + dy <= 1:  # Manhattan distance of 1 or less means adjacent
                        adjacent_to_door = True
                        break
                
                if adjacent_to_door:
                    continue  # Skip this position
                
                # Add to valid positions
                valid_positions.append(i)
                
                # Higher weight for positions closer to center
                center_distance = abs(i - center_idx)
                weight = max(1, 10 - center_distance * 2)
                
                # Reduce weight if close to existing doors (but not adjacent)
                for door_pos in door_positions:
                    dx = abs(x - door_pos[0])
                    dy = abs(y - door_pos[1])
                    distance = dx + dy
                    if 1 < distance < 3:  # Not adjacent but still close
                        weight = weight * 0.3  # Significant reduction if door is nearby
                
                weights.append(weight)
            
            # Select door positions based on weights
            for _ in range(num_doors):
                if not valid_positions or sum(weights) == 0:
                    break
                    
                # Choose position based on weights
                chosen_idx_idx = random.choices(range(len(valid_positions)), weights=weights)[0]
                chosen_idx = valid_positions[chosen_idx_idx]
                door_x, door_y = wall[chosen_idx]
                
                # Place door
                map_grid[door_y][door_x] = door_char
                door_positions.append((door_x, door_y))
                
                # Update valid positions and weights
                # Remove positions adjacent to the new door
                new_valid_positions = []
                new_weights = []
                
                for i, pos_idx in enumerate(valid_positions):
                    if pos_idx == chosen_idx:
                        continue  # Skip the chosen position
                    
                    x, y = wall[pos_idx]
                    
                    # Check if adjacent to the new door
                    dx = abs(x - door_x)
                    dy = abs(y - door_y)
                    if dx + dy <= 1:  # Adjacent to new door
                        continue
                    
                    new_valid_positions.append(pos_idx)
                    
                    # Update weight based on proximity to new door
                    weight = weights[i]
                    if 1 < dx + dy < 3:  # Close but not adjacent
                        weight = weight * 0.3
                    
                    new_weights.append(weight)
                
                valid_positions = new_valid_positions
                weights = new_weights


map_file_name = "../resources/map.txt"

def save_map_to_file(map_str, filename=map_file_name):
    """Save the generated map to a file"""
    with open(filename, 'w') as f:
        f.write(map_str)

if __name__ == "__main__":
    random_map = generate_random_map()
    print(random_map)
    save_map_to_file(random_map)
    print(f"Map saved to {map_file_name}")

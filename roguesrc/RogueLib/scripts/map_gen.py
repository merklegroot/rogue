import random

def generate_random_map(width=40, height=20):
    """
    Generate a random map with a single rectangular room with doors.
    
    Args:
        width (int): Width of the map
        height (int): Height of the map
    
    Returns:
        str: String representation of the map
    """
    # Initialize empty map with spaces
    map_grid = [[' ' for _ in range(width)] for _ in range(height)]
    
    # Generate random room dimensions (ensuring minimum size of 5x5)
    room_width = random.randint(7, min(15, width - 4))
    room_height = random.randint(5, min(8, height - 4))
    
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
    
    # Add doors to the walls
    add_doors_to_room(map_grid, room_x, room_y, room_width, room_height)
    
    # Convert map to string
    map_str = '\n'.join([''.join(row) for row in map_grid])
    return map_str

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
    door_char = '+'
    
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

import random
import hashlib
from collections import deque

class InfiniteRogueMap:
    """
    A procedurally generated infinite map in the style of classic Rogue.
    Uses a seed to deterministically generate map sections on demand.
    """
    
    def __init__(self, seed=None):
        """
        Initialize the infinite map with a seed.
        
        Args:
            seed: Random seed for map generation
        """
        self.seed = seed if seed is not None else random.randint(0, 1000000)
        self.room_cache = {}  # Cache of generated rooms by coordinates
        self.hallway_cache = {}  # Cache of generated hallways
        self.processing_sections = set()  # Track sections being processed to avoid recursion
        
        # Map tile characters
        self.FLOOR = '.'
        self.WALL_H = '═'
        self.WALL_V = '║'
        self.CORNER_TL = '╔'
        self.CORNER_TR = '╗'
        self.CORNER_BL = '╚'
        self.CORNER_BR = '╝'
        self.DOOR = '╬'
        self.HALLWAY = 'X'
        self.EMPTY = ' '
        
        # Room size constraints
        self.MIN_ROOM_WIDTH = 5
        self.MAX_ROOM_WIDTH = 12
        self.MIN_ROOM_HEIGHT = 4
        self.MAX_ROOM_HEIGHT = 8
        
        # Section size (each section can contain one room)
        self.CHUNK_SIZE = 20
    
    def get_tile(self, x, y):
        """
        Get the map tile at the specified coordinates.
        
        Args:
            x (int): X coordinate
            y (int): Y coordinate
            
        Returns:
            str: Character representing the map tile
        """
        # Determine which section this coordinate belongs to
        section_x = x // self.CHUNK_SIZE
        section_y = y // self.CHUNK_SIZE
        
        # Get or generate the room for this section
        room = self._get_or_generate_room(section_x, section_y)
        
        # Check if the coordinate is within the room
        if room and self._is_in_room(x, y, room):
            return self._get_room_tile(x, y, room)
        
        # Check if the coordinate is in a hallway
        hallway_tile = self._get_hallway_tile(x, y)
        if hallway_tile:
            return hallway_tile
        
        # Otherwise, it's empty space
        return self.EMPTY
    
    def _get_or_generate_room(self, section_x, section_y):
        """
        Get a cached room or generate a new one for the given section.
        
        Args:
            section_x (int): Section X coordinate
            section_y (int): Section Y coordinate
            
        Returns:
            dict: Room data or None if no room in this section
        """
        section_key = (section_x, section_y)
        
        # Return cached room if available
        if section_key in self.room_cache:
            return self.room_cache[section_key]
        
        # Check if we're already processing this section (avoid recursion)
        if section_key in self.processing_sections:
            return None
        
        # Mark this section as being processed
        self.processing_sections.add(section_key)
        
        # Generate a new room with deterministic randomness
        room_rng = self._get_section_rng(section_x, section_y)
        
        # Decide if this section should have a room (80% chance)
        if room_rng.random() > 0.2:
            # Determine room size
            room_width = room_rng.randint(self.MIN_ROOM_WIDTH, self.MAX_ROOM_WIDTH)
            room_height = room_rng.randint(self.MIN_ROOM_HEIGHT, self.MAX_ROOM_HEIGHT)
            
            # Position the room within the section (with some randomness)
            margin_x = self.CHUNK_SIZE - room_width
            margin_y = self.CHUNK_SIZE - room_height
            
            room_x = section_x * self.CHUNK_SIZE + room_rng.randint(1, max(1, margin_x - 1))
            room_y = section_y * self.CHUNK_SIZE + room_rng.randint(1, max(1, margin_y - 1))
            
            # Generate doors
            doors = self._generate_doors(room_x, room_y, room_width, room_height, room_rng)
            
            room = {
                'x': room_x,
                'y': room_y,
                'width': room_width,
                'height': room_height,
                'doors': doors,
                'section': (section_x, section_y)
            }
            
            # Cache the room
            self.room_cache[section_key] = room
            
            # Remove from processing set
            self.processing_sections.remove(section_key)
            
            return room
        
        # No room in this section
        self.room_cache[section_key] = None
        
        # Remove from processing set
        self.processing_sections.remove(section_key)
        
        return None
    
    def _schedule_hallway_generation(self, room, section_x, section_y):
        """
        Schedule hallway generation for a room to avoid recursion.
        This will be called after all rooms are generated.
        
        Args:
            room: Room data
            section_x, section_y: Section coordinates
        """
        # We'll handle this by generating hallways only when needed
        pass
    
    def _generate_doors(self, room_x, room_y, room_width, room_height, rng):
        """
        Generate doors for a room.
        
        Args:
            room_x, room_y: Room position
            room_width, room_height: Room dimensions
            rng: Random number generator
            
        Returns:
            list: List of door positions (x, y)
        """
        doors = []
        
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
            num_doors = rng.choices([0, 1, 2], weights=[0.3, 0.5, 0.2])[0]
            
            if num_doors == 0 or len(wall) < 3:
                continue
            
            # Calculate weights for door positions (higher weight for center positions)
            valid_positions = list(range(len(wall)))
            weights = []
            
            wall_center = len(wall) / 2
            for i in range(len(wall)):
                # Distance from center (0 to 1, where 0 is center)
                distance_from_center = abs(i - wall_center) / wall_center
                # Weight is higher for positions closer to center
                weight = 1.0 - (distance_from_center * 0.7)
                weights.append(weight)
            
            # Track door positions for this wall
            door_positions = []
            
            # Select door positions based on weights
            for _ in range(num_doors):
                if not valid_positions or sum(weights) == 0:
                    break
                    
                # Choose position based on weights
                chosen_idx_idx = rng.choices(range(len(valid_positions)), weights=weights)[0]
                chosen_idx = valid_positions[chosen_idx_idx]
                door_x, door_y = wall[chosen_idx]
                
                # Add door
                doors.append((door_x, door_y))
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
        
        return doors
    
    def _generate_hallways_for_room(self, room, section_x, section_y):
        """
        Generate hallways connecting this room to adjacent sections.
        
        Args:
            room: Room data
            section_x, section_y: Section coordinates
        """
        # Check adjacent sections
        adjacent_sections = [
            (section_x + 1, section_y),  # Right
            (section_x - 1, section_y),  # Left
            (section_x, section_y + 1),  # Down
            (section_x, section_y - 1)   # Up
        ]
        
        # Track which doors are connected
        connected_doors = set()
        
        for adj_x, adj_y in adjacent_sections:
            adj_key = (adj_x, adj_y)
            
            # Skip if we're already processing this section
            if adj_key in self.processing_sections:
                continue
                
            # Get the room in the adjacent section if it exists in cache
            if adj_key in self.room_cache:
                adj_room = self.room_cache[adj_key]
                if adj_room:
                    # Connect the rooms with hallways
                    hallway_doors = self._connect_rooms_with_hallway(room, adj_room)
                    if hallway_doors:
                        connected_doors.update(hallway_doors)
        
        # Remove doors that don't connect to anything
        room['doors'] = [door for door in room['doors'] if door in connected_doors]
    
    def _connect_rooms_with_hallway(self, room1, room2):
        """
        Connect two rooms with a hallway between their doors.
        
        Args:
            room1, room2: Room data
            
        Returns:
            set: Set of door positions that were connected
        """
        # Skip if already connected
        room_pair = tuple(sorted([(room1['x'], room1['y']), (room2['x'], room2['y'])]))
        if room_pair in self.hallway_cache:
            # Return the doors that are part of this hallway
            hallway = self.hallway_cache[room_pair]
            connected_doors = set()
            for door in room1['doors']:
                if door in hallway:
                    connected_doors.add(door)
            for door in room2['doors']:
                if door in hallway:
                    connected_doors.add(door)
            return connected_doors
        
        # Find the best door pair to connect
        best_door_pair = None
        best_distance = float('inf')
        
        for door1 in room1['doors']:
            for door2 in room2['doors']:
                dx = abs(door1[0] - door2[0])
                dy = abs(door1[1] - door2[1])
                distance = dx + dy
                
                if distance < best_distance:
                    best_distance = distance
                    best_door_pair = (door1, door2)
        
        if best_door_pair:
            # Create a hallway between these doors
            door1, door2 = best_door_pair
            
            # Get all rooms for pathfinding
            rooms = [room for room in self.room_cache.values() if room is not None]
            
            # Create a pathfinding grid
            hallway_path = self._find_path_avoiding_rooms(door1, door2, rooms)
            
            if hallway_path:
                # Cache the hallway
                self.hallway_cache[room_pair] = hallway_path
                return {door1, door2}
        
        return set()
    
    def _find_path_avoiding_rooms(self, start, end, rooms):
        """
        Find a path between two points that avoids going through rooms (except at doors).
        Uses A* pathfinding algorithm.
        
        Args:
            start: Starting point (x, y)
            end: Ending point (x, y)
            rooms: List of room data
            
        Returns:
            list: List of coordinates in the path
        """
        # Create a set of all wall and floor positions to avoid
        blocked_positions = set()
        door_positions = set()
        wall_positions = set()
        
        for room in rooms:
            # Add all wall positions
            for x in range(room['x'], room['x'] + room['width']):
                for y in range(room['y'], room['y'] + room['height']):
                    # Skip doors
                    if (x, y) in room['doors']:
                        door_positions.add((x, y))
                        continue
                    
                    # Check if it's a wall
                    is_wall = (
                        x == room['x'] or 
                        x == room['x'] + room['width'] - 1 or
                        y == room['y'] or 
                        y == room['y'] + room['height'] - 1
                    )
                    
                    if is_wall:
                        wall_positions.add((x, y))
                    
                    # Add to blocked positions (both walls and floors)
                    blocked_positions.add((x, y))
        
        # Remove start and end from blocked positions
        blocked_positions.discard(start)
        blocked_positions.discard(end)
        
        # A* pathfinding
        open_set = [(self._heuristic(start, end), 0, start, [])]  # (f, g, pos, path)
        closed_set = set()
        
        while open_set:
            # Get the node with the lowest f score
            open_set.sort()
            _, g, current, path = open_set.pop(0)
            
            # Check if we've reached the goal
            if current == end:
                return path + [current]
            
            # Skip if we've already processed this node
            if current in closed_set:
                continue
            
            # Add to closed set
            closed_set.add(current)
            
            # Generate neighbors
            x, y = current
            neighbors = [(x+1, y), (x-1, y), (x, y+1), (x, y-1)]
            
            for neighbor in neighbors:
                # Skip if blocked (except for doors)
                if neighbor in blocked_positions and neighbor not in door_positions:
                    continue
                
                # Skip if already processed
                if neighbor in closed_set:
                    continue
                
                # Calculate scores
                new_g = g + 1
                new_f = new_g + self._heuristic(neighbor, end)
                
                # Add to open set
                open_set.append((new_f, new_g, neighbor, path + [current]))
        
        # If no path is found, try a simpler approach
        return self._create_simple_hallway(start, end, blocked_positions, door_positions, wall_positions)
    
    def _create_simple_hallway(self, start, end, blocked_positions, door_positions, wall_positions):
        """
        Create a simple L-shaped hallway, avoiding blocked positions.
        
        Args:
            start: Starting point (x, y)
            end: Ending point (x, y)
            blocked_positions: Set of positions to avoid
            door_positions: Set of door positions (can pass through)
            wall_positions: Set of wall positions (never pass through)
            
        Returns:
            list: List of coordinates in the hallway path
        """
        path = []
        current_x, current_y = start
        target_x, target_y = end
        
        # Decide whether to go horizontal or vertical first
        hallway_rng = self._get_path_rng(start, end)
        go_horizontal_first = hallway_rng.choice([True, False])
        
        if go_horizontal_first:
            # Try to go horizontal first
            while current_x != target_x:
                next_x = current_x + (1 if current_x < target_x else -1)
                next_pos = (next_x, current_y)
                
                # If blocked and not a door, try going vertical instead
                if next_pos in blocked_positions and next_pos not in door_positions:
                    break
                
                # Never go through walls (except doors)
                if next_pos in wall_positions and next_pos not in door_positions:
                    break
                
                current_x = next_x
                path.append((current_x, current_y))
            
            # Then go vertical
            while current_y != target_y:
                next_y = current_y + (1 if current_y < target_y else -1)
                next_pos = (current_x, next_y)
                
                # If blocked and not a door, we can't proceed
                if next_pos in blocked_positions and next_pos not in door_positions:
                    # Try a different approach
                    return self._create_zigzag_hallway(start, end, blocked_positions, door_positions, wall_positions)
                
                # Never go through walls (except doors)
                if next_pos in wall_positions and next_pos not in door_positions:
                    return self._create_zigzag_hallway(start, end, blocked_positions, door_positions, wall_positions)
                
                current_y = next_y
                path.append((current_x, current_y))
            
            # Finally, go horizontal to reach the target
            while current_x != target_x:
                next_x = current_x + (1 if current_x < target_x else -1)
                next_pos = (next_x, current_y)
                
                # If blocked and not a door, we can't proceed
                if next_pos in blocked_positions and next_pos not in door_positions:
                    # Try a different approach
                    return self._create_zigzag_hallway(start, end, blocked_positions, door_positions, wall_positions)
                
                # Never go through walls (except doors)
                if next_pos in wall_positions and next_pos not in door_positions:
                    return self._create_zigzag_hallway(start, end, blocked_positions, door_positions, wall_positions)
                
                current_x = next_x
                path.append((current_x, current_y))
        else:
            # Try to go vertical first
            while current_y != target_y:
                next_y = current_y + (1 if current_y < target_y else -1)
                next_pos = (current_x, next_y)
                
                # If blocked and not a door, try going horizontal instead
                if next_pos in blocked_positions and next_pos not in door_positions:
                    break
                
                # Never go through walls (except doors)
                if next_pos in wall_positions and next_pos not in door_positions:
                    break
                
                current_y = next_y
                path.append((current_x, current_y))
            
            # Then go horizontal
            while current_x != target_x:
                next_x = current_x + (1 if current_x < target_x else -1)
                next_pos = (next_x, current_y)
                
                # If blocked and not a door, we can't proceed
                if next_pos in blocked_positions and next_pos not in door_positions:
                    # Try a different approach
                    return self._create_zigzag_hallway(start, end, blocked_positions, door_positions, wall_positions)
                
                # Never go through walls (except doors)
                if next_pos in wall_positions and next_pos not in door_positions:
                    return self._create_zigzag_hallway(start, end, blocked_positions, door_positions, wall_positions)
                
                current_x = next_x
                path.append((current_x, current_y))
            
            # Finally, go vertical to reach the target
            while current_y != target_y:
                next_y = current_y + (1 if current_y < target_y else -1)
                next_pos = (current_x, next_y)
                
                # If blocked and not a door, we can't proceed
                if next_pos in blocked_positions and next_pos not in door_positions:
                    # Try a different approach
                    return self._create_zigzag_hallway(start, end, blocked_positions, door_positions, wall_positions)
                
                # Never go through walls (except doors)
                if next_pos in wall_positions and next_pos not in door_positions:
                    return self._create_zigzag_hallway(start, end, blocked_positions, door_positions, wall_positions)
                
                current_y = next_y
                path.append((current_x, current_y))
        
        return path
    
    def _create_zigzag_hallway(self, start, end, blocked_positions, door_positions, wall_positions):
        """
        Create a zigzag hallway with multiple turns to avoid obstacles.
        
        Args:
            start: Starting point (x, y)
            end: Ending point (x, y)
            blocked_positions: Set of positions to avoid
            door_positions: Set of door positions (can pass through)
            wall_positions: Set of wall positions (never pass through)
            
        Returns:
            list: List of coordinates in the hallway path
        """
        # Use BFS to find a path
        queue = deque([(start, [])])
        visited = {start}
        
        while queue:
            current, path = queue.popleft()
            
            # Check if we've reached the goal
            if current == end:
                return path + [current]
            
            # Generate neighbors
            x, y = current
            neighbors = [(x+1, y), (x-1, y), (x, y+1), (x, y-1)]
            
            for neighbor in neighbors:
                # Skip if blocked (except for doors)
                if neighbor in blocked_positions and neighbor not in door_positions:
                    continue
                
                # Never go through walls (except doors)
                if neighbor in wall_positions and neighbor not in door_positions:
                    continue
                
                # Skip if already visited
                if neighbor in visited:
                    continue
                
                visited.add(neighbor)
                queue.append((neighbor, path + [current]))
        
        # If no path is found, return an empty path
        return []
    
    def _heuristic(self, a, b):
        """Manhattan distance heuristic for A* pathfinding"""
        return abs(a[0] - b[0]) + abs(a[1] - b[1])
    
    def _is_in_room(self, x, y, room):
        """Check if coordinates are within a room"""
        return (room['x'] <= x < room['x'] + room['width'] and 
                room['y'] <= y < room['y'] + room['height'])
    
    def _get_room_tile(self, x, y, room):
        """Get the tile type for a position within a room"""
        # Check if it's a door
        if (x, y) in room['doors']:
            return self.DOOR
        
        # Check if it's a corner
        if x == room['x'] and y == room['y']:
            return self.CORNER_TL
        elif x == room['x'] + room['width'] - 1 and y == room['y']:
            return self.CORNER_TR
        elif x == room['x'] and y == room['y'] + room['height'] - 1:
            return self.CORNER_BL
        elif x == room['x'] + room['width'] - 1 and y == room['y'] + room['height'] - 1:
            return self.CORNER_BR
        
        # Check if it's a wall
        if x == room['x'] or x == room['x'] + room['width'] - 1:
            return self.WALL_V
        if y == room['y'] or y == room['y'] + room['height'] - 1:
            return self.WALL_H
        
        # Otherwise it's a floor
        return self.FLOOR
    
    def _get_hallway_tile(self, x, y):
        """Check if coordinates are in a hallway"""
        for hallway_path in self.hallway_cache.values():
            if (x, y) in hallway_path:
                return self.HALLWAY
        return None
    
    def _get_section_rng(self, section_x, section_y):
        """Get a deterministic RNG for a section based on the seed"""
        section_seed = f"{self.seed}_{section_x}_{section_y}"
        hash_val = int(hashlib.md5(section_seed.encode()).hexdigest(), 16)
        return random.Random(hash_val)
    
    def _get_path_rng(self, start, end):
        """Get a deterministic RNG for a path based on endpoints"""
        path_seed = f"{self.seed}_{start[0]}_{start[1]}_{end[0]}_{end[1]}"
        hash_val = int(hashlib.md5(path_seed.encode()).hexdigest(), 16)
        return random.Random(hash_val)
    
    def generate_map_section(self, center_x, center_y, width=40, height=20):
        """
        Generate a section of the map as a string.
        
        Args:
            center_x, center_y: Center coordinates
            width, height: Dimensions of the map section
            
        Returns:
            str: String representation of the map section
        """
        start_x = center_x - width // 2
        start_y = center_y - height // 2
        
        # First, generate all rooms in the visible area
        for y in range(start_y, start_y + height):
            for x in range(start_x, start_x + width):
                section_x = x // self.CHUNK_SIZE
                section_y = y // self.CHUNK_SIZE
                self._get_or_generate_room(section_x, section_y)
        
        # Then generate hallways between all rooms
        for section_key, room in self.room_cache.items():
            if room:  # Skip empty sections
                section_x, section_y = section_key
                self._generate_hallways_for_room(room, section_x, section_y)
        
        # Clean up any doors that don't connect to hallways
        self._remove_unconnected_doors()
        
        # Generate the map grid
        map_grid = [[self.get_tile(start_x + x, start_y + y) for x in range(width)] 
                    for y in range(height)]
        
        # Convert to string
        return '\n'.join([''.join(row) for row in map_grid])
    
    def _remove_unconnected_doors(self):
        """Remove doors that don't connect to any hallways"""
        # Collect all doors that are part of hallways
        connected_doors = set()
        for hallway in self.hallway_cache.values():
            for pos in hallway:
                connected_doors.add(pos)
        
        # Remove unconnected doors from each room
        for room in self.room_cache.values():
            if room:  # Skip empty sections
                room['doors'] = [door for door in room['doors'] if door in connected_doors]


map_file_name = "../RogueLib/resources/map.txt"

def save_map_to_file(map_str, filename=map_file_name):
    """Save the generated map to a file"""
    with open(filename, 'w') as f:
        f.write(map_str)

if __name__ == "__main__":
    random_seed = random.randint(0, 1000000)
    infinite_map = InfiniteRogueMap(seed=random_seed)
    
    map_section = infinite_map.generate_map_section(0, 0, width=200, height=200)
    
    print(f"Using random seed: {random_seed}")
    print(map_section)
    save_map_to_file(map_section)
    print(f"Map saved to {map_file_name}")

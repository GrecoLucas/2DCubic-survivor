import matplotlib.pyplot as plt
from matplotlib.patches import Rectangle, FancyArrowPatch
import os
import math
import random

import json

def load_area_names(areas_dir):
    names = {}
    if not os.path.exists(areas_dir):
        return names
        
    for filename in os.listdir(areas_dir):
        if filename.endswith(".json"):
            area_id = os.path.splitext(filename)[0]
            try:
                with open(os.path.join(areas_dir, filename), 'r', encoding='utf-8') as f:
                    data = json.load(f)
                    if "name" in data:
                        names[area_id] = data["name"]
            except Exception as e:
                print(f"Warning: Could not read {filename}: {e}")
    return names

def load_map(path):
    # Resolve absolute path if relative
    if not os.path.isabs(path):
        # Assuming path is relative to this script
        base_dir = os.path.dirname(os.path.abspath(__file__))
        path = os.path.join(base_dir, path)

    areas = {}
    if not os.path.exists(path):
        print(f"Error: File not found at {path}")
        return {}

    with open(path, "r", encoding="utf-8") as f:
        for line in f:
            line = line.strip()
            if not line or line.startswith("#"):
                continue

            parts = line.split()
            if not parts:
                continue
                
            area = parts[0]
            connections = {}

            for conn in parts[1:]:
                if ":" in conn:
                    direction, target = conn.split(":")
                    connections[direction] = target
            
            areas[area] = connections
    return areas

DIR_OFFSET = {
    "U": (0, 2.0),
    "D": (0, -2.0),
    "L": (-2.0, 0),
    "R": (2.0, 0)
}

def solve_layout(areas, iterations=2000, learning_rate=0.05):
    """
    Solves for node positions using a spring-like relaxation method.
    Tries to satisfy the directional constraints (U=+y, R=+x, etc).
    """
    # Initialize randomly to avoid stacking
    # Use a fixed seed for reproducibility if desired, or random
    random.seed(42) 
    coords = {area: [random.uniform(-0.1, 0.1), random.uniform(-0.1, 0.1)] for area in areas}
    
    # Anchor the first area to (0,0)
    if areas:
        start_node = next(iter(areas))
        coords[start_node] = [0.0, 0.0]
    else:
        return {}

    # Build constraints list
    constraints = []
    for u, conns in areas.items():
        for d, v in conns.items():
            if v in areas:
                dx, dy = DIR_OFFSET.get(d, (0,0))
                constraints.append((u, v, dx, dy))

    # Relaxation loop
    for _ in range(iterations):
        moves = {area: [0.0, 0.0, 0] for area in areas}
        
        for u, v, dx, dy in constraints:
            ux, uy = coords[u]
            vx, vy = coords[v]
            
            # Constraint: v should be at u + (dx, dy)
            # Error: (vx - ux) - dx
            
            ex = (vx - ux) - dx
            ey = (vy - uy) - dy
            
            # Pull u towards v - delta
            moves[u][0] += ex
            moves[u][1] += ey
            moves[u][2] += 1
            
            # Pull v towards u + delta
            moves[v][0] -= ex
            moves[v][1] -= ey
            moves[v][2] += 1
            
        # Apply moves
        max_shift = 0
        for area in areas:
            if area == start_node: continue # Keep anchor fixed
            
            if moves[area][2] > 0:
                # Average the forces
                fx = moves[area][0] / moves[area][2]
                fy = moves[area][1] / moves[area][2]
                
                coords[area][0] += fx * learning_rate
                coords[area][1] += fy * learning_rate
                
                max_shift = max(max_shift, abs(fx), abs(fy))
        
        if max_shift < 0.0001:
            break
            
    return coords

def render_map(areas, area_names=None, output_path="world_map.png"):
    if area_names is None:
        area_names = {}
        
    coords = solve_layout(areas)
    
    if not coords:
        print("No areas to render.")
        return

    # Determine bounds
    xs = [c[0] for c in coords.values()]
    ys = [c[1] for c in coords.values()]
    min_x, max_x = min(xs), max(xs)
    min_y, max_y = min(ys), max(ys)
    
    # Add padding
    pad_x = 1.5
    pad_y = 1.5
    
    width = max((max_x - min_x) + 2*pad_x, 8)
    height = max((max_y - min_y) + 2*pad_y, 6)
    
    fig, ax = plt.subplots(figsize=(width, height))
    
    # Styling
    bg_color = '#1e1e1e'
    node_color = '#4a90e2'
    text_color = 'white'
    arrow_color = '#aaaaaa'
    
    ax.set_facecolor(bg_color)
    fig.patch.set_facecolor(bg_color)

    # Draw connections
    for area, conns in areas.items():
        x1, y1 = coords[area]
        for d, target in conns.items():
            if target not in coords:
                continue
            
            x2, y2 = coords[target]
            
            # Node dimensions
            rect_w, rect_h = 0.6, 0.4
            
            # Calculate start and end points based on direction
            # Default to center if unknown
            start_pos = (x1, y1)
            end_pos = (x2, y2)
            
            if d == "U":
                start_pos = (x1, y1 + rect_h/2)
                end_pos = (x2, y2 - rect_h/2)
            elif d == "D":
                start_pos = (x1, y1 - rect_h/2)
                end_pos = (x2, y2 + rect_h/2)
            elif d == "L":
                start_pos = (x1 - rect_w/2, y1)
                end_pos = (x2 + rect_w/2, y2)
            elif d == "R":
                start_pos = (x1 + rect_w/2, y1)
                end_pos = (x2 - rect_w/2, y2)
            
            # Check for bidirectional connection to curve arrows
            is_bidirectional = False
            if target in areas:
                for d2, t2 in areas[target].items():
                    if t2 == area:
                        is_bidirectional = True
                        break
            
            # Curve if bidirectional
            connection_style = 'arc3,rad=0.0'
            if is_bidirectional:
                # Adjust curvature for edge-to-edge connections
                # Since points are already offset, we might need less curvature or keep it
                connection_style = 'arc3,rad=0.2'
            
            arrow = FancyArrowPatch(start_pos, end_pos, 
                                    arrowstyle='-|>', 
                                    mutation_scale=15, 
                                    color=arrow_color,
                                    connectionstyle=connection_style,
                                    shrinkA=0, shrinkB=0,
                                    alpha=0.8)
            ax.add_patch(arrow)

    # Draw nodes
    for area, (x, y) in coords.items():
        # Draw rounded rectangle (simulated with Rectangle for now)
        rect_w, rect_h = 1, 0.4
        rect = Rectangle((x - rect_w/2, y - rect_h/2), rect_w, rect_h, 
                         facecolor=node_color, edgecolor='white', lw=2, zorder=10)
        ax.add_patch(rect)
        
        # Label text
        label = area
        if area in area_names:
            label = f"{area}\n{area_names[area]}"
            
        ax.text(x, y, label, ha='center', va='center', 
                color=text_color, fontweight='bold', zorder=11, fontsize=9)

    ax.set_xlim(min_x - pad_x, max_x + pad_x)
    ax.set_ylim(min_y - pad_y, max_y + pad_y)
    ax.set_aspect('equal')
    ax.axis('off')
    
    plt.tight_layout()
    try:
        plt.savefig(output_path, dpi=150, facecolor=bg_color)
        print(f"Map generated successfully: {os.path.abspath(output_path)}")
    except Exception as e:
        print(f"Error saving map: {e}")
    finally:
        plt.close()

if __name__ == "__main__":
    # Path to maps.txt relative to this script
    base_dir = os.path.dirname(__file__)
    map_path = os.path.join(base_dir, "..", "maps.txt")
    areas_dir = os.path.join(base_dir, "..", "areas")
    
    areas = load_map(map_path)
    area_names = load_area_names(areas_dir)
    
    render_map(areas, area_names, "world_map.png")

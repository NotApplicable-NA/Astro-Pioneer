from PIL import Image, ImageDraw

def create_sci_fi_slot():
    # Dimensions
    size = (128, 128)
    image = Image.new("RGBA", size, (0, 0, 0, 0))
    draw = ImageDraw.Draw(image)

    # Settings
    margin = 4
    corner_radius = 20
    
    # Colors (from GDD)
    # Lab White: #F0F4F8
    # Industrial Grey (Border): #78909C
    # Holographic Blue: #00F3FF (with alpha)
    
    color_rim_main = (240, 244, 248, 255) # F0F4F8
    color_rim_border = (120, 144, 156, 255) # 78909C (approx)
    color_hologram = (0, 243, 255, 50) # Low opacity for glass
    color_hologram_glow = (0, 243, 255, 100) # Slightly stronger glow at edges

    # 1. Draw Outer Border (Darker Industrial Grey)
    rect_box = [margin, margin, size[0]-margin, size[1]-margin]
    draw.rounded_rectangle(rect_box, radius=corner_radius, fill=None, outline=color_rim_border, width=6)

    # 2. Draw Main Rim (Lab White)
    # Shrink slightly to be inside the border
    rect_rim = [margin+2, margin+2, size[0]-margin-2, size[1]-margin-2]
    draw.rounded_rectangle(rect_rim, radius=corner_radius-2, fill=None, outline=color_rim_main, width=8)

    # 3. Draw Inner Glass (Holographic Blue)
    # Inside the rim
    rect_glass = [margin+10, margin+10, size[0]-margin-10, size[1]-margin-10]
    draw.rounded_rectangle(rect_glass, radius=corner_radius-5, fill=color_hologram, outline=color_hologram_glow, width=2)

    # 4. Add "Tech Detail" (Simple lines on the rim)
    # Top Left
    draw.line([(margin+20, margin+6), (margin+50, margin+6)], fill=color_rim_border, width=2)
    # Bottom Right
    draw.line([(size[0]-margin-50, size[1]-margin-6), (size[0]-margin-20, size[1]-margin-6)], fill=color_rim_border, width=2)

    # Save
    output_path = "Assets/Art/UI/UI_InventorySlot.png"
    image.save(output_path)
    print(f"Generated {output_path}")

if __name__ == "__main__":
    create_sci_fi_slot()

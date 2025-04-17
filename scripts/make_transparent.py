from PIL import Image

# Load the image
img = Image.open('../images/Codepage-437.png')

# Convert to RGBA if it isn't already
img = img.convert('RGBA')

# Get the data
data = img.getdata()

# Create new data with black pixels made transparent
new_data = []
for item in data:
    # If it's black (RGB all 0)
    if item[0] == 0 and item[1] == 0 and item[2] == 0:
        # Make it transparent
        new_data.append((0, 0, 0, 0))
    else:
        # Keep original color and alpha
        new_data.append(item)

# Update the image with the new data
img.putdata(new_data)

# Save the modified image
img.save('../images/Codepage-437-transparent.png', 'PNG')
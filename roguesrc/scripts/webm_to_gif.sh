#!/usr/bin/env bash

# Usage: ./webm_to_gif.sh input.webm output.gif

set -e

if [ $# -ne 2 ]; then
    echo "Usage: $0 input.webm output.gif"
    exit 1
fi

INPUT="$1"
OUTPUT="$2"
TMP_PALETTE="palette.png"

# Generate a palette for better colors
ffmpeg -y -i "$INPUT" -vf "fps=15,scale=480:-1:flags=lanczos,palettegen" "$TMP_PALETTE"

# Create the gif using the palette
ffmpeg -i "$INPUT" -i "$TMP_PALETTE" -filter_complex "fps=15,scale=480:-1:flags=lanczos[x];[x][1:v]paletteuse" "$OUTPUT"

# Clean up
rm "$TMP_PALETTE"

echo "GIF saved to $OUTPUT"
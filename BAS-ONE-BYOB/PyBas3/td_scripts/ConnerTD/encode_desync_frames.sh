#!/usr/bin/env bash
# Encode desync image sequence to video. Run after recording in TouchDesigner.
# Frames: artifacts/desync_frames/frame.0.NNNN.png (TD image sequence) or frame_NNNN.png

set -e
FRAMES_DIR="${1:-/Users/CONWARD/dev/bas/artifacts/desync_frames}"
OUTPUT="${2:-/Users/CONWARD/dev/bas/artifacts/desync_output.mp4}"
FPS=30

if [[ ! -d "$FRAMES_DIR" ]]; then
  echo "Frames dir not found: $FRAMES_DIR"
  exit 1
fi

# Prefer TD pattern frame.0.0000.png; then frame_0000.png or frame_0001.png (ffmpeg extract)
if [[ -f "$FRAMES_DIR/frame.0.0000.png" ]]; then
  PATTERN="$FRAMES_DIR/frame.0.%04d.png"
  EXTRA=""
elif [[ -f "$FRAMES_DIR/frame_0000.png" ]]; then
  PATTERN="$FRAMES_DIR/frame_%04d.png"
  EXTRA=""
elif [[ -f "$FRAMES_DIR/frame_0001.png" ]]; then
  PATTERN="$FRAMES_DIR/frame_%04d.png"
  EXTRA="-start_number 1"
else
  echo "No frame sequence found in $FRAMES_DIR (look for frame.0.0000.png or frame_0000.png or frame_0001.png)"
  exit 1
fi

COUNT=$(ls "$FRAMES_DIR"/frame*.png 2>/dev/null | wc -l)
echo "Encoding $COUNT frames at ${FPS}fps -> $OUTPUT"
ffmpeg -y $EXTRA -framerate "$FPS" -i "$PATTERN" -c:v libx264 -pix_fmt yuv420p -profile:v main -movflags +faststart -crf 20 "$OUTPUT"
echo "Done: $OUTPUT"

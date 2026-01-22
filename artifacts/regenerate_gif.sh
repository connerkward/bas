#!/bin/bash
# Regenerate GIF from basvid_v1.mov

cd "$(dirname "$0")" || exit

ffmpeg -i basvid_v1.mov \
  -vf "fps=15,scale=1080:1920:force_original_aspect_ratio=decrease,split[s0][s1];[s0]palettegen[p];[s1][p]paletteuse" \
  -t 10 \
  basvid_v1.gif \
  -y

echo "GIF regenerated. To upload to GitHub:"
echo "  git add artifacts/basvid_v1.gif"
echo "  git commit -m 'Update GIF'"
echo "  git push"

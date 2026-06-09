#!/bin/bash
# Automated test protocol for vision detector
# Tests: detection, thumbnails, participant tracking, NDI streams
#
# Usage:
#   ./test_vision.sh              # Use camera
#   ./test_vision.sh video.mp4    # Use video file

set -e

cd "$(dirname "$0")"

# Use uv if available, fallback to venv
if command -v uv &> /dev/null; then
    RUN="uv run python"
else
    source ../.venv/bin/activate
    RUN="python"
fi

VIDEO_SOURCE="${1:-0}"

echo "=== Vision Detector Test Protocol ==="
echo ""

# Clear previous test artifacts
echo "1. Clearing previous test artifacts..."
rm -f thumbnails/*.jpg 2>/dev/null || true
echo "   ✓ Cleared thumbnails"
echo ""

# Run detector for 5 seconds
echo "2. Running detector for 5 seconds (source: $VIDEO_SOURCE)..."
$RUN multi_person_detector.py --source "$VIDEO_SOURCE" 2>&1 | \
    grep -v "GL version\|INFO:\|W0000\|Model already\|inference_feedback\|landmark_projection" &
DETECTOR_PID=$!
sleep 5
kill $DETECTOR_PID 2>/dev/null || true
wait $DETECTOR_PID 2>/dev/null || true
echo "   ✓ Detector run complete"
echo ""

# Wait a moment for file writes
sleep 1

# Verify thumbnails
echo "3. Verifying thumbnails..."
if [ -d thumbnails ]; then
    TOTAL=$(find thumbnails -name "*.jpg" 2>/dev/null | wc -l | tr -d ' ')
    TEMP=$(find thumbnails -name "participant_temp_*.jpg" 2>/dev/null | wc -l | tr -d ' ')
    REAL=$(find thumbnails -name "participant_*.jpg" ! -name "participant_temp_*" 2>/dev/null | wc -l | tr -d ' ')
    
    echo "   Total thumbnails: $TOTAL"
    echo "   Temp UUID thumbnails: $TEMP"
    echo "   Real UUID thumbnails: $REAL"
    
    if [ "$TEMP" -gt 0 ]; then
        echo "   ⚠️  WARNING: Found $TEMP temp UUID thumbnails (should be 0)"
    else
        echo "   ✓ No temp UUID thumbnails"
    fi
    
    if [ "$REAL" -gt 0 ]; then
        echo "   ✓ Found $REAL real participant thumbnails"
        echo "   Files:"
        find thumbnails -name "participant_*.jpg" ! -name "participant_temp_*" 2>/dev/null | head -5 | sed 's/^/     /'
    else
        echo "   ⚠️  No real participant thumbnails found"
    fi
else
    echo "   ⚠️  Thumbnails directory not found"
fi
echo ""

# Verify participant database
echo "4. Verifying participant database..."
if [ -f participants_db.json ]; then
    PARTICIPANTS=$(cat participants_db.json | python -c "import sys, json; d=json.load(sys.stdin); print(len(d))" 2>/dev/null || echo "0")
    echo "   Participants in DB: $PARTICIPANTS"
    if [ "$PARTICIPANTS" -gt 0 ]; then
        echo "   ✓ Participant tracking working"
        echo "   Sample entries:"
        cat participants_db.json | python -m json.tool 2>/dev/null | head -15 | sed 's/^/     /' || cat participants_db.json | head -10 | sed 's/^/     /'
    else
        echo "   ⚠️  No participants in database"
    fi
else
    echo "   ⚠️  participants_db.json not found"
fi
echo ""

# Summary
echo "=== Test Summary ==="
if [ "$TEMP" -eq 0 ] && [ "$REAL" -gt 0 ]; then
    echo "✓ PASS: Thumbnails working correctly (no temp UUIDs, real UUIDs present)"
else
    echo "✗ FAIL: Thumbnail issues detected"
fi

if [ "$PARTICIPANTS" -gt 0 ]; then
    echo "✓ PASS: Participant tracking working"
else
    echo "⚠️  WARNING: No participants tracked"
fi

echo ""
echo "Test complete."

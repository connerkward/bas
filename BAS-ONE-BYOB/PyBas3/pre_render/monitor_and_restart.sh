#!/bin/bash
# Monitor pre-render process and restart if it crashes before completion

OUTPUT_DIR="outputs/2025-02-05_runside_megaslow_compressed_720p_blend_output"
LOG_FILE="outputs/2025-02-05_run.log"
VIDEO_FILE="$OUTPUT_DIR/videos/pose_skeleton.mp4"
MONITOR_LOG="outputs/monitor_$(date +%Y-%m-%d_%H-%M-%S).log"

echo "[$(date '+%Y-%m-%d %H:%M:%S')] Starting monitor script" | tee -a "$MONITOR_LOG"

while true; do
    # Check if the target file exists (indicates completion)
    if [ -f "$VIDEO_FILE" ]; then
        echo "[$(date '+%Y-%m-%d %H:%M:%S')] SUCCESS: pose_skeleton.mp4 found! Process completed successfully." | tee -a "$MONITOR_LOG"

        # Check if DONE message is in log
        if grep -q "\[DONE\]" "$LOG_FILE"; then
            echo "[$(date '+%Y-%m-%d %H:%M:%S')] Process fully completed. Exiting monitor." | tee -a "$MONITOR_LOG"
            exit 0
        fi
    fi

    # Check if process is running
    if pgrep -f "depth_blend_video.py.*runside-megaslow-compressed" > /dev/null; then
        echo "[$(date '+%Y-%m-%d %H:%M:%S')] Process is running..." | tee -a "$MONITOR_LOG"
        sleep 10
    else
        # Process not running - check if it completed or crashed
        if [ -f "$VIDEO_FILE" ] && grep -q "\[DONE\]" "$LOG_FILE"; then
            echo "[$(date '+%Y-%m-%d %H:%M:%S')] Process completed successfully!" | tee -a "$MONITOR_LOG"
            exit 0
        else
            echo "[$(date '+%Y-%m-%d %H:%M:%S')] Process crashed or stopped! Restarting..." | tee -a "$MONITOR_LOG"

            # Restart the process using uv run
            cd "$(dirname "$0")"
            uv run depth_blend_video.py ../input_videos/runside-megaslow-compressed.mp4 \
                --effects chroma_dithered pose_skeleton composite_skeleton_dithered \
                --target-fps 30 \
                --output-dir "$OUTPUT_DIR" >> "$LOG_FILE" 2>&1 &

            NEW_PID=$!
            echo "[$(date '+%Y-%m-%d %H:%M:%S')] Restarted with PID: $NEW_PID" | tee -a "$MONITOR_LOG"
            sleep 5
        fi
    fi
done

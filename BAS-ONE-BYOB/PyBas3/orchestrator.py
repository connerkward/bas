#!/usr/bin/env python3
"""
PyBas3 Orchestrator - Launches Vision + Scoring processes.

Usage:
    python orchestrator.py              # Vision + Scoring only
    python orchestrator.py --dashboard  # + live score display
    python orchestrator.py --persist    # Keep participants across restarts
"""

import argparse
import signal
import subprocess
import sys
import time
from pathlib import Path

ROOT = Path(__file__).parent
VISION_SCRIPT = ROOT / "mediapipe" / "multi_person_detector.py"
SCORING_SCRIPT = ROOT / "scoring" / "pose_scorer.py"
DASHBOARD_SCRIPT = ROOT / "mediapipe" / "live_dashboard.py"


class Orchestrator:
    def __init__(self):
        self.processes: dict[str, subprocess.Popen] = {}
        self._shutdown = False
    
    def start(self, name: str, script: Path, args: list[str] = None):
        """Start a subprocess."""
        cmd = [sys.executable, str(script)] + (args or [])
        print(f"[orchestrator] Starting {name}: {' '.join(cmd)}")
        proc = subprocess.Popen(cmd, cwd=ROOT)
        self.processes[name] = proc
        return proc
    
    def stop_all(self):
        """Stop all subprocesses."""
        self._shutdown = True
        for name, proc in self.processes.items():
            if proc.poll() is None:
                print(f"[orchestrator] Stopping {name}...")
                proc.terminate()
        
        # Wait for graceful shutdown
        for name, proc in self.processes.items():
            try:
                proc.wait(timeout=3)
            except subprocess.TimeoutExpired:
                print(f"[orchestrator] Force killing {name}")
                proc.kill()
        
        self.processes.clear()
    
    def wait(self):
        """Wait for processes, restart if they crash."""
        while not self._shutdown:
            for name, proc in list(self.processes.items()):
                ret = proc.poll()
                if ret is not None and not self._shutdown:
                    print(f"[orchestrator] {name} exited with code {ret}")
            time.sleep(1)


def main():
    parser = argparse.ArgumentParser(description="PyBas3 Orchestrator")
    parser.add_argument("--dashboard", action="store_true", 
                        help="Open live score dashboard")
    parser.add_argument("--persist", action="store_true",
                        help="Keep participants across restarts")
    parser.add_argument("--vision-only", action="store_true",
                        help="Only start Vision (skip Scoring)")
    args = parser.parse_args()
    
    orch = Orchestrator()
    
    # Signal handler for Ctrl+C
    def handle_signal(sig, frame):
        print("\n[orchestrator] Shutting down...")
        orch.stop_all()
        sys.exit(0)
    
    signal.signal(signal.SIGINT, handle_signal)
    signal.signal(signal.SIGTERM, handle_signal)
    
    # Start processes
    vision_args = ["--persist"] if args.persist else []
    orch.start("vision", VISION_SCRIPT, vision_args)
    
    # Small delay to let Vision create shared memory
    time.sleep(1)
    
    if not args.vision_only:
        orch.start("scoring", SCORING_SCRIPT)
    
    if args.dashboard:
        orch.start("dashboard", DASHBOARD_SCRIPT)
    
    print("[orchestrator] All processes started. Ctrl+C to stop.")
    
    try:
        orch.wait()
    except KeyboardInterrupt:
        pass
    finally:
        orch.stop_all()


if __name__ == "__main__":
    main()

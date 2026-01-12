#!/bin/bash
cd "$(dirname "$0")"
source ../.venv/bin/activate
python multi_person_detector.py 2>&1 | grep -v "GL version\|INFO:\|W0000\|Model already\|inference_feedback\|landmark_projection"

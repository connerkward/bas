# Internally used, don't mind this.
KILL_THREADS = False

# Toggle this in order to view how your WebCam is being interpreted (reduces performance).
DEBUG = True 

# Change UDP connection settings (must match Unity side)
HOST = '127.0.0.1'
PORT = 52733

CAM_INDEX = 0 # OpenCV2 webcam index, change for using another (ex: external) webcam.

# Settings do not universally apply, not all WebCams support all frame rates and resolutions
USE_CUSTOM_CAM_SETTINGS = False
FPS = 60
WIDTH = 320
HEIGHT = 240

MODEL_COMPLEXITY = 1 #[0, 1] higher numbers are more precise but also cost more performance
MIN_DETECTION_CONFIDENCE = 0.5 #[0.0, 1.0]
MIN_TRACKING_CONFIDENCE = 0.5 #[0.0, 1.0]
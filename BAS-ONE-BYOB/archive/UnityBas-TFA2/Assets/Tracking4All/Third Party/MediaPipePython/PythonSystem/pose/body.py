# MediaPipe Body
import mediapipe as mp
from mediapipe.tasks import python
from mediapipe.tasks.python import vision
from common.clientUDP import ClientUDP
import pose.global_vars as global_vars
from common.captureThread import CaptureThread

import threading
import time
import cv2

# the body thread actually does the 
# processing of the captured images, and communication with unity
class BodyThread(threading.Thread):
    data = ""
    dirty = True
    pipe = None
    timeSinceCheckedConnection = 0
    timeSincePostStatistics = 0

    def run(self):
        mp_drawing = mp.solutions.drawing_utils
        mp_pose = mp.solutions.pose

        self.setup_comms()
        
        capture = CaptureThread(global_vars)
        capture.start()

        with mp_pose.Pose(min_detection_confidence=global_vars.MIN_DETECTION_CONFIDENCE, min_tracking_confidence=global_vars.MIN_TRACKING_CONFIDENCE, model_complexity = global_vars.MODEL_COMPLEXITY,static_image_mode = False,enable_segmentation = False) as pose: 
            
            while not global_vars.KILL_THREADS and capture.isRunning==False:
                print("Waiting for camera and capture thread.")
                time.sleep(0.5)
            print("Beginning capture")
                
            while not global_vars.KILL_THREADS and capture.cap.isOpened():
                ti = time.time()

                # Fetch stuff from the capture thread
                ret = capture.ret
                image = capture.frame
                                
                # Image transformations and stuff
                image = cv2.flip(image, 1)
                image.flags.writeable = global_vars.DEBUG
                
                # Detections
                image = cv2.cvtColor(image, cv2.COLOR_BGR2RGB)
                results = pose.process(image)
                image = cv2.cvtColor(image, cv2.COLOR_RGB2BGR)
                tf = time.time()
                
                # Rendering results
                if global_vars.DEBUG:
                    if time.time()-self.timeSincePostStatistics>=1:
                        print("Theoretical Maximum FPS: %f"%(1/(tf-ti)))
                        self.timeSincePostStatistics = time.time()
                        
                    if results.pose_landmarks:
                        mp_drawing.draw_landmarks(image, results.pose_landmarks, mp_pose.POSE_CONNECTIONS, 
                                                mp_drawing.DrawingSpec(color=(255, 100, 0), thickness=2, circle_radius=4),
                                                mp_drawing.DrawingSpec(color=(255, 255, 255), thickness=2, circle_radius=2),
                                                )
                    cv2.imshow('Body Tracking', image)
                    cv2.waitKey(3)

                # Set up data for relay
                self.data = ""
                i = 0
                if results.pose_world_landmarks:
                    hand_world_landmarks = results.pose_world_landmarks
                    for i in range(0,33):
                        self.data += "{}|{}|{}|{}\n".format(i,hand_world_landmarks.landmark[i].x,hand_world_landmarks.landmark[i].y,hand_world_landmarks.landmark[i].z)

                self.send_data(self.data)
                    
        self.pipe.close()
        capture.cap.release()
        cv2.destroyAllWindows()
        pass

    def setup_comms(self):
        self.client = ClientUDP(global_vars.HOST,global_vars.PORT)
        self.client.start()

    def send_data(self,message):
        self.client.sendMessage(message)
        pass
                        
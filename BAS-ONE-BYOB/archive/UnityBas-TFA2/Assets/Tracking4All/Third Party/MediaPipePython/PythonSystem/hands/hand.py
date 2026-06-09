# MediaPipe Body
import mediapipe as mp
from mediapipe.tasks import python
from mediapipe.tasks.python import vision
from common.clientUDP import ClientUDP
import hands.global_vars as global_vars
from common.captureThread import CaptureThread

import threading
import time
import cv2

# the body thread actually does the 
# processing of the captured images, and communication with unity
class HandThread(threading.Thread):
    data = ""
    dirty = True
    pipe = None
    timeSinceCheckedConnection = 0
    timeSincePostStatistics = 0

    def run(self):
        mp_drawing = mp.solutions.drawing_utils
        mp_hands = mp.solutions.hands

        self.setup_comms()
        
        capture = CaptureThread(global_vars)
        capture.start()

        with mp_hands.Hands(min_detection_confidence=global_vars.MIN_DETECTION_CONFIDENCE, min_tracking_confidence=global_vars.MIN_TRACKING_CONFIDENCE, model_complexity = global_vars.MODEL_COMPLEXITY) as hands: 
            
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
                
                # Detections
                image = cv2.cvtColor(image, cv2.COLOR_BGR2RGB)
                results = hands.process(image)
                image = cv2.cvtColor(image, cv2.COLOR_RGB2BGR)
                
                image.flags.writeable = global_vars.DEBUG
                tf = time.time()
                
                # Rendering results
                if global_vars.DEBUG:
                    if time.time()-self.timeSincePostStatistics>=1:
                        print("Theoretical Maximum FPS: %f"%(1/(tf-ti)))
                        self.timeSincePostStatistics = time.time()
                        
                    if results.multi_hand_landmarks:
                        for num, hand in enumerate(results.multi_hand_landmarks):
                            mp_drawing.draw_landmarks(image, hand, mp_hands.HAND_CONNECTIONS, 
                                                    mp_drawing.DrawingSpec(color=(121, 22, 76), thickness=2, circle_radius=4),
                                                    mp_drawing.DrawingSpec(color=(250, 44, 250), thickness=2, circle_radius=2)
                                                    )
                                                 
                    cv2.imshow('Hand Tracking', image)
                    cv2.waitKey(3)

                # Set up data for relay
                self.data = ""
                i = 0
                if results.multi_hand_world_landmarks:
                    for j in range(len(results.multi_handedness)):
                        hand_world_landmarks = results.multi_hand_world_landmarks[j]
                        self.data +=str(results.multi_handedness[j].classification[0].label)+"\n";
                        for i in range(0,21):
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
                        
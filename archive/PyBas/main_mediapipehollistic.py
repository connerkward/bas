import cv2
import mediapipe as mp
import json
import pprint
import asyncio
import multiprocessing
import time

# from IO.socketio_adapter import start_socketio_server as NetworkLoop
from IO.udp_adapter import start_udp_server as NetworkLoop


# Initialize MediaPipe Holistic model
mp_holistic = mp.solutions.holistic
holistic = mp_holistic.Holistic()

# Pose and Hand landmarks mappings
POSE_LANDMARKS = mp_holistic.PoseLandmark
HAND_LANDMARKS = mp_holistic.HandLandmark


def extract_keypoints(results, process_pose=True, process_face=True, process_left_hand=True, process_right_hand=True):
    keypoints = {}
    if process_pose and results.pose_landmarks:
        for i, landmark in enumerate(results.pose_landmarks.landmark):
            keypoints[POSE_LANDMARKS(i).name] = {
                "x": landmark.x,
                "y": landmark.y,
                "z": landmark.z,
                "visibility": landmark.visibility,
            }
    if process_face and results.face_landmarks:
        keypoints["face"] = [
            {"x": landmark.x, "y": landmark.y, "z": landmark.z}
            for landmark in results.face_landmarks.landmark
        ]
    if process_left_hand and results.left_hand_landmarks:
        for i, landmark in enumerate(results.left_hand_landmarks.landmark):
            keypoints[HAND_LANDMARKS(i).name] = {
                "x": landmark.x,
                "y": landmark.y,
                "z": landmark.z,
            }
    if process_right_hand and results.right_hand_landmarks:
        for i, landmark in enumerate(results.right_hand_landmarks.landmark):
            keypoints[HAND_LANDMARKS(i).name] = {
                "x": landmark.x,
                "y": landmark.y,
                "z": landmark.z,
            }
    return keypoints

def visualize_and_send(frame, results, network_queue, process_pose=True, process_face=True, process_left_hand=True, process_right_hand=True):
    if process_pose and results.pose_landmarks:
        mp.solutions.drawing_utils.draw_landmarks(
            frame, results.pose_landmarks, mp_holistic.POSE_CONNECTIONS
        )
    if process_face and results.face_landmarks:
        mp.solutions.drawing_utils.draw_landmarks(
            frame, results.face_landmarks, mp_holistic.FACEMESH_TESSELATION
        )
    if process_left_hand and results.left_hand_landmarks:
        mp.solutions.drawing_utils.draw_landmarks(
            frame, results.left_hand_landmarks, mp_holistic.HAND_CONNECTIONS
        )
    if process_right_hand and results.right_hand_landmarks:
        mp.solutions.drawing_utils.draw_landmarks(
            frame, results.right_hand_landmarks, mp_holistic.HAND_CONNECTIONS
        )

    keypoints = extract_keypoints(results, process_pose, process_face, process_left_hand, process_right_hand)
    keypoints_json = json.dumps({"keypoints": keypoints})
    if keypoints:
        # print(keypoints)
        # print(keypoints["NOSE"])
        network_queue.put(keypoints_json)
    return frame

def main(network_queue, process_pose=True, process_face=True, process_left_hand=True, process_right_hand=True):
    cap = cv2.VideoCapture(0)

    while cap.isOpened():
        ret, frame = cap.read()
        if not ret:
            print("frames not found")
            break

        rgb_frame = cv2.cvtColor(frame, cv2.COLOR_BGR2RGB)
        results = holistic.process(rgb_frame)
        frame_with_landmarks = visualize_and_send(
            frame, results, network_queue, 
            process_pose, process_face, process_left_hand, process_right_hand
        )

        cv2.imshow("output", frame_with_landmarks)

        if cv2.waitKey(1) & 0xFF == ord("q"):
            break

    cap.release()
    cv2.destroyAllWindows()

if __name__ == "__main__":
    process_pose = True
    process_face = False
    process_left_hand = False
    process_right_hand = False

    mp_stop_event = multiprocessing.Event()
    network_queue = multiprocessing.Queue()
    network_thread = multiprocessing.Process(target=NetworkLoop, 
                                            kwargs={"mp_udp_queue":network_queue, "mp_stop_event":mp_stop_event},
                                            daemon=True)
    network_thread.start()
    # time.sleep(5) # for socketio uvicorn startup
    
    main(network_queue, process_pose, process_face, process_left_hand, process_right_hand)

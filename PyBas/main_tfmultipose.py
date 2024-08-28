import tensorflow as tf
import tensorflow_hub as hub
import numpy as np
import cv2
import json
import pprint
import asyncio

# import IO.socketio_server as Server


# Load the MoveNet multipose model from TensorFlow Hub
movenet = hub.load("https://tfhub.dev/google/movenet/multipose/lightning/1")

keypoint_names = [
    "nose",
    "left_eye",
    "right_eye",
    "left_ear",
    "right_ear",
    "left_shoulder",
    "right_shoulder",
    "left_elbow",
    "right_elbow",
    "left_wrist",
    "right_wrist",
    "left_hip",
    "right_hip",
    "left_knee",
    "right_knee",
    "left_ankle",
    "right_ankle",
]

connections = [
    (0, 1),
    (0, 2),
    (1, 3),
    (2, 4),
    (0, 5),
    (0, 6),
    (5, 7),
    (7, 9),
    (6, 8),
    (8, 10),
    (5, 6),
    (5, 11),
    (6, 12),
    (11, 12),
    (11, 13),
    (13, 15),
    (12, 14),
    (14, 16),
]


def detect_pose_multipose(frame):
    image_rgb = cv2.cvtColor(frame, cv2.COLOR_BGR2RGB)
    image_resized = tf.image.resize_with_pad(
        tf.expand_dims(image_rgb, axis=0), 256, 256
    )
    image_np = image_resized.numpy().astype(np.int32)
    outputs = movenet.signatures["serving_default"](tf.constant(image_np))
    keypoints_with_scores = outputs["output_0"].numpy()
    return keypoints_with_scores


def visualize_pose_multipose(
    frame, keypoints_with_scores, send_function, confidence_threshold=0.2
):
    keypoints_with_scores = np.array(keypoints_with_scores)
    keypoints_to_send = []

    for person in keypoints_with_scores[0]:
        keypoints = person[:51].reshape((17, 3))  # Extract and reshape keypoints
        person_keypoints = {}

        for i, kp in enumerate(keypoints):
            if (
                kp[2] > confidence_threshold
            ):  # Only draw keypoints with a confidence score above the threshold
                x = int(kp[1] * frame.shape[1])
                y = int(kp[0] * frame.shape[0])
                cv2.circle(frame, (x, y), 6, (255, 0, 0), -1)
                person_keypoints[keypoint_names[i]] = {
                    "x": float(kp[1]),
                    "y": float(kp[0]),
                    "confidence": float(kp[2]),
                }

        keypoints_to_send.append(person_keypoints)

        for connection in connections:
            start = keypoints[connection[0]]
            end = keypoints[connection[1]]
            if (
                start[2] > confidence_threshold and end[2] > confidence_threshold
            ):  # Only draw connections with confidence scores above the threshold
                start_point = (
                    int(start[1] * frame.shape[1]),
                    int(start[0] * frame.shape[0]),
                )
                end_point = (int(end[1] * frame.shape[1]), int(end[0] * frame.shape[0]))
                cv2.line(frame, start_point, end_point, (0, 0, 255), 2)

    # Convert keypoints to JSON and send using the provided send function
    keypoints_json = json.dumps({"keypoints": keypoints_to_send})
    pprint.pprint(keypoints_json)
    send_function(keypoints_json)

    return frame


def main(send_function):
    cap = cv2.VideoCapture(0)
    confidence_threshold = 0.5  # Set the desired confidence threshold

    while cap.isOpened():
        ret, frame = cap.read()
        if not ret:
            break

        keypoints_with_scores = detect_pose_multipose(frame)
        frame_with_poses = visualize_pose_multipose(
            frame, keypoints_with_scores, send_function, confidence_threshold
        )

        cv2.imshow("Pose Detection", frame_with_poses)

        if cv2.waitKey(1) & 0xFF == ord("q"):
            break

    cap.release()
    cv2.destroyAllWindows()


if __name__ == "__main__":
    # server = Server()
    # asyncio.run(main(server.send_message))
    def empty(dingo):
        pass
    asyncio.run(main(empty))
    # main(empty)

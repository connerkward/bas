using System;
using System.Collections.Generic;
using UnityEngine;

namespace Tracking4All
{
    /// <summary>
    /// An assortment of helper functions.
    /// </summary>
    public static class Helpers
    {
        /// <summary>
        /// Flips the right and left landmark for the input.
        /// </summary>
        /// <param name="landmark"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static PoseLandmarks Flip(this PoseLandmarks landmark)
        {
            switch (landmark)
            {
                case PoseLandmarks.NOSE:
                    return PoseLandmarks.NOSE;

                case PoseLandmarks.LEFT_EYE_INNER:
                    return PoseLandmarks.RIGHT_EYE_INNER;
                case PoseLandmarks.RIGHT_EYE_INNER:
                    return PoseLandmarks.LEFT_EYE_INNER;

                case PoseLandmarks.LEFT_EYE:
                    return PoseLandmarks.RIGHT_EYE;
                case PoseLandmarks.RIGHT_EYE:
                    return PoseLandmarks.LEFT_EYE;

                case PoseLandmarks.LEFT_EYE_OUTER:
                    return PoseLandmarks.RIGHT_EYE_OUTER;
                case PoseLandmarks.RIGHT_EYE_OUTER:
                    return PoseLandmarks.LEFT_EYE_OUTER;

                case PoseLandmarks.LEFT_EAR:
                    return PoseLandmarks.RIGHT_EAR;
                case PoseLandmarks.RIGHT_EAR:
                    return PoseLandmarks.LEFT_EAR;

                case PoseLandmarks.MOUTH_LEFT:
                    return PoseLandmarks.MOUTH_RIGHT;
                case PoseLandmarks.MOUTH_RIGHT:
                    return PoseLandmarks.MOUTH_LEFT;

                case PoseLandmarks.LEFT_SHOULDER:
                    return PoseLandmarks.RIGHT_SHOULDER;
                case PoseLandmarks.RIGHT_SHOULDER:
                    return PoseLandmarks.LEFT_SHOULDER;

                case PoseLandmarks.LEFT_ELBOW:
                    return PoseLandmarks.RIGHT_ELBOW;
                case PoseLandmarks.RIGHT_ELBOW:
                    return PoseLandmarks.LEFT_ELBOW;

                case PoseLandmarks.LEFT_WRIST:
                    return PoseLandmarks.RIGHT_WRIST;
                case PoseLandmarks.RIGHT_WRIST:
                    return PoseLandmarks.LEFT_WRIST;

                case PoseLandmarks.LEFT_PINKY:
                    return PoseLandmarks.RIGHT_PINKY;
                case PoseLandmarks.RIGHT_PINKY:
                    return PoseLandmarks.LEFT_PINKY;

                case PoseLandmarks.LEFT_INDEX:
                    return PoseLandmarks.RIGHT_INDEX;
                case PoseLandmarks.RIGHT_INDEX:
                    return PoseLandmarks.LEFT_INDEX;

                case PoseLandmarks.LEFT_THUMB:
                    return PoseLandmarks.RIGHT_THUMB;
                case PoseLandmarks.RIGHT_THUMB:
                    return PoseLandmarks.LEFT_THUMB;

                case PoseLandmarks.LEFT_HIP:
                    return PoseLandmarks.RIGHT_HIP;
                case PoseLandmarks.RIGHT_HIP:
                    return PoseLandmarks.LEFT_HIP;

                case PoseLandmarks.LEFT_KNEE:
                    return PoseLandmarks.RIGHT_KNEE;
                case PoseLandmarks.RIGHT_KNEE:
                    return PoseLandmarks.LEFT_KNEE;

                case PoseLandmarks.LEFT_ANKLE:
                    return PoseLandmarks.RIGHT_ANKLE;
                case PoseLandmarks.RIGHT_ANKLE:
                    return PoseLandmarks.LEFT_ANKLE;

                case PoseLandmarks.LEFT_HEEL:
                    return PoseLandmarks.RIGHT_HEEL;
                case PoseLandmarks.RIGHT_HEEL:
                    return PoseLandmarks.LEFT_HEEL;

                case PoseLandmarks.LEFT_FOOT:
                    return PoseLandmarks.RIGHT_FOOT;
                case PoseLandmarks.RIGHT_FOOT:
                    return PoseLandmarks.LEFT_FOOT;

                default:
                    throw new ArgumentOutOfRangeException(nameof(landmark), landmark, null);
            }
        }
        /// <summary>
        /// Flips the right and left handedness for the input.
        /// </summary>
        /// <param name="handedness"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static Handedness Flip(this Handedness handedness)
        {
            switch (handedness)
            {
                case Handedness.LEFT:
                    return Handedness.RIGHT;
                case Handedness.RIGHT:
                    return Handedness.LEFT;

                default:
                    throw new ArgumentOutOfRangeException(nameof(handedness), handedness, null);
            }
        }
        public static PoseJoints Flip(this PoseJoints joint)
        {
            switch (joint)
            {
                case PoseJoints.Hips:
                    return PoseJoints.Hips;
                case PoseJoints.Spine:
                    return PoseJoints.Spine;
                case PoseJoints.Chest:
                    return PoseJoints.Chest;
                case PoseJoints.Neck:
                    return PoseJoints.Neck;
                case PoseJoints.Head:
                    return PoseJoints.Head;

                case PoseJoints.RightShoulder:
                    return PoseJoints.LeftShoulder;
                case PoseJoints.RightUpperArm:
                    return PoseJoints.LeftUpperArm;
                case PoseJoints.RightLowerArm:
                    return PoseJoints.LeftLowerArm;
                case PoseJoints.RightHand:
                    return PoseJoints.LeftHand;

                case PoseJoints.LeftShoulder:
                    return PoseJoints.RightShoulder;
                case PoseJoints.LeftUpperArm:
                    return PoseJoints.RightUpperArm;
                case PoseJoints.LeftLowerArm:
                    return PoseJoints.RightLowerArm;
                case PoseJoints.LeftHand:
                    return PoseJoints.RightHand;

                case PoseJoints.RightUpperLeg:
                    return PoseJoints.LeftUpperLeg;
                case PoseJoints.RightLowerLeg:
                    return PoseJoints.LeftLowerLeg;
                case PoseJoints.RightFoot:
                    return PoseJoints.LeftFoot;

                case PoseJoints.LeftUpperLeg:
                    return PoseJoints.RightUpperLeg;
                case PoseJoints.LeftLowerLeg:
                    return PoseJoints.RightLowerLeg;
                case PoseJoints.LeftFoot:
                    return PoseJoints.RightFoot;

                default:
                    throw new ArgumentException("Unknown joint provided");
            }
        }

        /// <summary>
        /// Check if the interface provider's underlying interface is null.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="i"></param>
        /// <returns></returns>
        public static bool IsNull<T>(this InterfaceProvider<T> i)
            where T : class
        {
            return i.Null;
        }
        public static T Nullable<T>(this InterfaceProvider<T> i)
            where T : class
        {
            return i.Null ? null : i.Provider;
        }

        public static int GetLength(System.Type t)
        {
            return System.Enum.GetValues(t).Length;
        }
        public static string[] GetNames(System.Type t)
        {
            return System.Enum.GetNames(t);
        }
        public static float GetTime()
        {
            return Tracking4All.GetElapsedTime();
        }

        public static float DeltaTime(float last)
        {
            return GetTime() - last;
        }

        public static Landmark Average(params Landmark[] landmarks)
        {
            Landmark middle = default;
            for (int i = 0; i < landmarks.Length; ++i)
            {
                middle.Position += landmarks[i].Position;
                middle.Visibility += landmarks[i].Visibility;
            }

            middle.Position /= landmarks.Length;
            middle.Visibility /= landmarks.Length;

            return middle;
        }
        public static Vector3 Average(params Vector3[] landmarks)
        {
            Vector3 middle = default;
            for (int i = 0; i < landmarks.Length; ++i)
            {
                middle += landmarks[i];
            }

            middle /= landmarks.Length;

            return middle;
        }

        public static NormalizedLandmark Average(params NormalizedLandmark[] landmarks)
        {
            NormalizedLandmark middle = default;
            for (int i = 0; i < landmarks.Length; ++i)
            {
                middle.Position += landmarks[i].Position;
                middle.Visibility += landmarks[i].Visibility;
            }

            middle.Position /= landmarks.Length;
            middle.Visibility /= landmarks.Length;

            return middle;
        }

        public static Vector3 CalculateNormal(Vector3 p1, Vector3 p2, Vector3 p3)
        {
            Vector3 u = p2 - p1;
            Vector3 v = p3 - p1;
            Vector3 n = new Vector3((u.y * v.z - u.z * v.y), (u.z * v.x - u.x * v.z), (u.x * v.y - u.y * v.x));
            float nl = Mathf.Sqrt(n[0] * n[0] + n[1] * n[1] + n[2] * n[2]);
            return new Vector3(n[0] / nl, n[1] / nl, n[2] / nl);
        }

        public static T GetInterface<T>(this GameObject go, bool includingInactive = false) where T : class
        {
            if (go == null) return null;

            Component[] components = go.GetComponents(typeof(Component));
            foreach (var c in components)
            {
                if (c is T && (includingInactive || c.gameObject.activeInHierarchy))
                {
                    return c as T;
                }
            }

            return null;
        }

        /// <summary>
        /// Returns all children of a transform, including itself (at index 0).
        /// </summary>
        /// <param name="parent"></param>
        /// <returns></returns>
        public static Transform[] GetAllChildrenAndSelf(this Transform parent)
        {
            return parent.GetComponentsInChildren<Transform>();
        }
        /// <summary>
        /// Returns all children of a transform, excluding itself. Little more expensive do not call per frame or some crap.
        /// </summary>
        /// <param name="parent"></param>
        /// <returns></returns>
        public static Transform[] GetAllChildrenAndNotSelf(this Transform parent)
        {
            List<Transform> transforms = new List<Transform>(parent.GetComponentsInChildren<Transform>());
            transforms.Remove(parent);
            return transforms.ToArray();
        }
    }
}
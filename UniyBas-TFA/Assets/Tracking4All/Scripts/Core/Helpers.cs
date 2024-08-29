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
// MPUHelpers
// (C) 2024 G8gaming Ltd.
using System.Collections.Generic;
using UnityEngine;

namespace Tracking4All
{
    public static class MPUHelpers
    {
        /// <summary>
        /// Fetch the currently loaded camera options.
        /// </summary>
        /// <returns></returns>
        public static List<string> GetCameraOptions()
        {
            List<string> options = new();
            var cameras = WebCamTexture.devices;
            for (int i = 0; i < cameras.Length; ++i)
            {
                options.Add(cameras[i].name);
            }

            return options;
        }

        public static string GetCameraName(int index)
        {
            if (!HasCameraOptions())
            {
                Logger.LogError("There is no cameras currently loaded in the api…");
                return "UNKNOWN";
            }

            List<string> options = GetCameraOptions();
            if (index < 0 || index >= options.Count)
            {
                Logger.LogError("Camera Index " + index + " was out of range.");
                return "UNKNOWN";
            }

            return options[index];
        }

        public static bool HasCameraOptions()
        {
            return WebCamTexture.devices.Length > 0;
        }
    }
}
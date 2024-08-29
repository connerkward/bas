using UnityEngine;

namespace Tracking4All
{
    /// <summary>
    /// Tracking4All logger.
    /// </summary>
    public static class Logger
    {
        private static readonly string NAME = "Tracking4All";

        public static void LogError(object o, object context = null)
        {
            if (context==null)
            {
                Debug.LogError(NAME + ": " + o);
            }
            else
            {
                Debug.LogError(NAME + " ["+context+ "]: " + o);
            }
        }

        public static void LogWarning(object o, object context = null)
        {
            if (context == null)
            {
                Debug.LogWarning(NAME + ": " + o);
            }
            else
            {
                Debug.LogWarning(NAME + " [" + context + "]: " + o);
            }
        }

        public static void LogInfo(object o, object context = null)
        {
            if (context == null)
            {
                Debug.Log(NAME + ": " + o);
            }
            else
            {
                Debug.Log(NAME + " [" + context + "]: " + o);
            }
        }
    }
}
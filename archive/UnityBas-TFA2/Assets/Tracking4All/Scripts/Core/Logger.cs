using UnityEngine;

namespace Tracking4All
{
    /// <summary>
    /// Tracking4All logger.
    /// </summary>
    public static class Logger
    {
        private const string NAME = "Tracking4All";

        private static bool COLOR_TEXT = true;
        private const string ERROR_COLOR = "#a5cee6";//"#cc7680";
        private const string WARNING_COLOR = "#a5cee6";//"#ccb876";
        private const string NORMAL_COLOR = "#a5cee6";

        private static string PrefixEffects(string input, string color)
        {
#if UNITY_EDITOR
            if (COLOR_TEXT)
            {
                return "<color=" + color + ">" + input + "</color>";
            }
#endif

            return input;
        }

        public static void LogError(object o, object context = null)
        {
            if (context == null)
            {
                Debug.LogError(PrefixEffects(NAME + ": " + o, ERROR_COLOR));
            }
            else
            {
                Debug.LogError(PrefixEffects(NAME + " [" + context + "]: " + o, ERROR_COLOR));
            }
        }

        public static void LogWarning(object o, object context = null)
        {
            if (context == null)
            {
                Debug.LogWarning(PrefixEffects(NAME + ": " + o, WARNING_COLOR));
            }
            else
            {
                Debug.LogWarning(PrefixEffects(NAME + " [" + context + "]: " + o, WARNING_COLOR));
            }
        }

        public static void LogInfo(object o, object context = null)
        {
            if (context == null)
            {
                Debug.Log(PrefixEffects(NAME + ": " + o, NORMAL_COLOR));
            }
            else
            {
                Debug.Log(PrefixEffects(NAME + " [" + context + "]: " + o, NORMAL_COLOR));
            }
        }
    }
}
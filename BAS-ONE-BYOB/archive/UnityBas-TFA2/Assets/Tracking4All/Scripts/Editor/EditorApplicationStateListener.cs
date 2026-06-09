// EditorApplicationStateListener
// (C) 2024 G8gaming Ltd.
using UnityEditor;

namespace Tracking4All
{
    [InitializeOnLoadAttribute]
    public static class EditorApplicationStateListener
    {
        static EditorApplicationStateListener()
        {
            EditorApplication.playModeStateChanged += LogPlayModeState;
        }

        private static void LogPlayModeState(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingPlayMode)
            {
                // NOTE:
                // By default settings are cleared between playmode runs.
                // This is to avoid misleading developers on what the default settings are.
                // For builds or if the below line is commented out, settings will be saved between runs
                // BUT ONLY on this local machine.
                if (true)
                {
                    SettingsManager.PersistanceHandler.ClearAndSave();
                    Logger.LogInfo("Settings cleared since this is the editor...", "Editor Application");
                }
                else
                {
#pragma warning disable CS0162 // Unreachable code detected
                    Logger.LogError("Settings are being saved during editor. FYI this can cause inaccurate default settings!", "Editor Application");
#pragma warning restore CS0162 // Unreachable code detected
                }
            }
        }
    }
}
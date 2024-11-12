// EditorSettingsDebuggingWindow
// (C) 2024 G8gaming Ltd.
using UnityEditor;
using UnityEngine;

namespace Tracking4All
{
    public class EditorSettingsDebuggingWindow : EditorWindow
    {
        [MenuItem("Tracking4All/Settings Debugging")]
        public static void ShowExample()
        {
            EditorSettingsDebuggingWindow wnd = GetWindow<EditorSettingsDebuggingWindow>();
            wnd.titleContent = new GUIContent("Settings Debugging");
        }

        Vector2 scroll;

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Available Settings (runtime only):", EditorStyles.boldLabel);
            SettingsManager.PersistanceHandler.VERBOSE =
                EditorGUILayout.Toggle("Verbose Mode: ", SettingsManager.PersistanceHandler.VERBOSE);

            if (SettingsManager.PersistanceHandler.EditorOnly_RawSaveData != null && EditorApplication.isPlaying)
            {
                int Count = SettingsManager.PersistanceHandler.EditorOnly_RawSaveData.Count;
                scroll = EditorGUILayout.BeginScrollView(scroll);
                string display = "";
                for (int i = 0; i < Count; ++i)
                {
                    string s = SettingsManager.PersistanceHandler.EditorOnly_RawSaveData.GetName(i) +
                        " = " + SettingsManager.PersistanceHandler.EditorOnly_RawSaveData.GetValue(i);
                    EditorGUILayout.LabelField(s, EditorStyles.label);
                    display += s + '\n';
                }
                EditorGUILayout.EndScrollView();
            }

            GUILayout.Space(10);
            GUILayout.Label("Has any settings stored? \n" + SettingsManager.PersistanceHandler.IsPrefsEmpty().ToString() + "\n",
                EditorStyles.boldLabel);
            if(GUILayout.Button("Copy State"))
            {
                GUIUtility.systemCopyBuffer = SettingsManager.PersistanceHandler.EditorOnly_RawState;
                Logger.LogInfo("Copied");
            }
            if (GUILayout.Button("Reset Saved Settings"))
            {
                if (SettingsManager.Instance != null)
                {
                    SettingsManager.Instance.DeleteSettings();
                }
                else
                {
                    SettingsManager.PersistanceHandler.ClearAndSave();
                }
                Logger.LogInfo("Reset all settings successfully.");
            }
            GUILayout.TextArea("NOTE: above doesn't clear the runtime settings, it just clears the save data (so if you reload the scene the settings will be default again).",
                EditorStyles.wordWrappedLabel);

            EditorUtility.SetDirty(this);
        }

        private void OnInspectorUpdate()
        {
            Repaint();
        }
    }
}
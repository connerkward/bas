// SettingsMenu
// (C) 2024 G8gaming Ltd.
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Tracking4All
{
    /// <summary>
    /// Manages settings and populates appropriate debug menu window.
    /// </summary>
    [DefaultExecutionOrder(-200)]
    public class SettingsManager : MonoBehaviour
    {
        public static SettingsManager Instance;

        [SerializeField] private bool persistAnySettings = true; // if false, settings will never persist.
        [SerializeField] private GameObject settingsMenuContent;
        [SerializeField] private DropdownSettingsUI dropdownPrefab;
        [SerializeField] private TextInputSettingsUI floatPrefab;
        [SerializeField] private TextInputSettingsUI intPrefab;
        [SerializeField] private ToggleSettingsUI boolPrefab;

        private static Dictionary<string, SettingsUIElement> settings = new();

        public delegate void AnySettingChanged(string setting);
        /// <summary>
        /// Called when any setting is changed through ui.
        /// <para>Listen to this for when any setting might be updated.</para>
        /// </summary>
        public static event AnySettingChanged OnAnySettingChanged;

        private void Awake()
        {
            if (Instance != null)
            {
                Logger.LogError("Only 1 settings menu should ever exist at a time!", gameObject.name);
                Destroy(gameObject);
                return;
            }
            Instance = this;

            // Remove next update
            if (persistAnySettings)
            {
                Logger.LogWarning("Note: settings will persist between scenes and on builds will persist indefinitely.",
                    "Settings Manager - PersistAnySettings");
            }

            ReloadSettings();
        }

        /// <summary>
        /// Refresh all settings/sync.
        /// </summary>
        public void RefreshDisplay()
        {
            ReloadSettings();

            // NOTE: currently only respects visibility.
            foreach (var s in settings)
            {
                Refresh(s.Value);
            }
        }
        private void Refresh(SettingsUIElement e)
        {
            if (!e.SettingVisible) e.gameObject.SetActive(false);
            else e.gameObject.SetActive(true);
        }

        /// <summary>
        /// Loads settings from persistant handler.
        /// </summary>
        public void ReloadSettings()
        {
            if (!persistAnySettings) return;

            PersistanceHandler.Load(ref settings);
        }
        /// <summary>
        /// Saves the current settings to the handler.
        /// </summary>
        public void SaveSettings()
        {
            if (!persistAnySettings) return;

            PersistanceHandler.Save(settings);
        }
        /// <summary>
        /// Reset the persistant settings and immediately reload.
        /// </summary>
        public void DeleteSettings()
        {
            PersistanceHandler.ClearAndSave();
            RefreshDisplay();
        }

        public void AddSetting<T>(EnumSetting<T> setting)
            where T : System.Enum
        {
            DropdownSettingsUI ui = (DropdownSettingsUI)Instantiate(dropdownPrefab);
            if (TryAdd(ui, setting))
            {
                ui.Hook(setting, OnAnySettingChanged, new List<string>(Helpers.GetNames(typeof(T))), System.Convert.ToInt32(setting.Value));
                Refresh(ui);
            }
        }
        public void AddSetting(FloatSetting setting)
        {
            TextInputSettingsUI ui = (TextInputSettingsUI)Instantiate(floatPrefab);
            if (TryAdd(ui, setting))
            {
                ui.Hook(setting, OnAnySettingChanged, setting.Value);
                Refresh(ui);
            }
        }
        public void AddSetting(IntSetting setting)
        {
            TextInputSettingsUI ui = (TextInputSettingsUI)Instantiate(intPrefab);
            if (TryAdd(ui, setting))
            {
                ui.Hook(setting, OnAnySettingChanged, setting.Value);
                Refresh(ui);
            }

        }
        public void AddSetting(BoolSetting setting)
        {
            ToggleSettingsUI ui = (ToggleSettingsUI)Instantiate(boolPrefab);
            if (TryAdd(ui, setting))
            {
                ui.Hook(setting, OnAnySettingChanged, setting.Value);
                Refresh(ui);
            }
        }
        public void AddSetting(IntByStringSetting setting, List<string> options)
        {
            DropdownSettingsUI ui = (DropdownSettingsUI)Instantiate(dropdownPrefab);
            setting.UpdateOptions(options);
            if (TryAdd(ui, setting))
            {
                ui.Hook(setting, OnAnySettingChanged, options, setting.Value);
                Refresh(ui);
            }
        }
        public void AddSetting(IRegisterableSettings registerableSettings)
        {
            registerableSettings.RegisterMenuSettings();
        }
        private bool TryAdd(SettingsUIElement e, RuntimeSetting s)
        {
            if (string.IsNullOrWhiteSpace(s.Name))
            {
                Logger.LogError("Tried to add a setting '" + e + "'. But is has an empty name! Please set a name (in the inspector most likely)!");
                return false;
            }
            if (settings.ContainsKey(s.Name))
            {
                Logger.LogError("Tried to add a duplicate setting '" + e.Name + "'. All settings must have a unique name.");
                return false;
            }

            settings.Add(s.Name, e);


            if (persistAnySettings)
            {
                // Update value to save data.
                string v = PersistanceHandler.Lookup(s.Name);
                if (v != null) s.OnSettingUIChanged(v);
            }

            return true;
        }

        private SettingsUIElement Instantiate(SettingsUIElement uiPrefab)
        {
            SettingsUIElement s = Instantiate(uiPrefab, settingsMenuContent.transform, true);
            s.gameObject.transform.localScale = Vector3.one;
            return s;
        }

        public void RemoveSetting(string name)
        {
            if (!settings.ContainsKey(name)) return;
            settings[name].SetValue(settings[name].DefaultSettingValueObject);
            if (settings[name] != null) Destroy(settings[name].gameObject);
            settings.Remove(name);
        }
        public void RemoveSetting(RuntimeSetting setting)
        {
            RemoveSetting(setting.Name);
        }
        public void RemoveSetting(IRegisterableSettings registerableSettings)
        {
            registerableSettings.DeregisterMenuSettings();
        }

        /// <summary>
        /// Get the current settings state (list of all values).
        /// </summary>
        /// <returns></returns>
        public List<object> GetCompleteSettingsState()
        {
            List<object> o = new List<object>();
            foreach (var s in settings)
            {
                o.Add(s.Value.SettingValueObject);
            }

            return o;
        }
        /// <summary>
        /// Check if the current setting state differs from an input old state.
        /// </summary>
        /// <param name="oldState">The old state cached when starting to monitor changes.</param>
        /// <returns></returns>
        public bool IsSettingStateDirty(List<object> oldState)
        {
            var current = GetCompleteSettingsState();
            if (oldState.Count != current.Count)
            {
                Logger.LogWarning("Settings state had different lengths, this should never happen. Assuming dirty state.", gameObject.name);
                return true;
            }

            bool dirty = false;
            for (int i = 0; i < oldState.Count; ++i)
            {
                if (!current[i].Equals(oldState[i]))
                {
                    dirty = true;
                    break;
                }
            }

            return dirty;
        }


        /// <summary>
        /// Returns true if the setting with the input name exists.
        /// </summary>
        /// <param name="settingName"></param>
        /// <returns></returns>
        public static bool HasSetting(string settingName)
        {
            return settings.ContainsKey(settingName);
        }

        public static float GetFloat(string settingName, float defaultValue)
        {
            return GetValue(settingName, defaultValue);
        }
        public static int GetInt(string settingName, int defaultValue)
        {
            return GetValue(settingName, defaultValue);
        }
        public static bool GetBool(string settingName, bool defaultValue)
        {
            return GetValue(settingName, defaultValue);
        }
        /// <summary>
        /// Returns the setting value given the input name.
        /// <para>Returns defaultValue if the setting does not exist or is null.</para>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="settingName"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static T GetValue<T>(string settingName, T defaultValue)
        {
            if (HasSetting(settingName))
            {
                try
                {
                    object o = settings[settingName].SettingValueObject;
                    return o == null ? defaultValue : (T)o;
                }
                catch (System.InvalidCastException ex)
                {
                    Debug.LogError(settingName + " is not of type " + typeof(T).Name + "\n" + ex.Message);
                }
            }

            return defaultValue;
        }

        public static void SetInt(string settingName, int value)
        {
            SetValue(settingName, value);
        }
        public static void SetFloat(string settingName, float value)
        {
            SetValue(settingName, value);
        }
        public static void SetBool(string settingName, bool value)
        {
            SetValue(settingName, value);
        }
        public static void SetValue(string settingName, object value)
        {
            if (HasSetting(settingName))
            {
                settings[settingName].SetValue(value);
            }
        }

        public static class PersistanceHandler
        {
            public static bool VERBOSE = false;

            private const string PREFS_KEY = "TRACKING4ALL_SETTINGS_DATA";
            private const string LOG_ID = "T4A Settings Manager";
            public static bool IsPrefsEmpty()
            {
                return PlayerPrefs.GetString(PREFS_KEY) != "";
            }

#if UNITY_EDITOR
            public static SettingsSaveData EditorOnly_RawSaveData => saveData;
            public static string EditorOnly_RawState => PlayerPrefs.GetString(PREFS_KEY);
#endif

            private static SettingsSaveData saveData;
            private static SettingsSaveData SaveData
            {
                get
                {
                    if (saveData == null) saveData = new SettingsSaveData();
                    return saveData;
                }
            }

            public static void Save(Dictionary<string, SettingsUIElement> data)
            {
                SaveData.Save(data);
                if (VERBOSE) Logger.LogInfo("Saved data", LOG_ID);
            }
            public static void Load(ref Dictionary<string, SettingsUIElement> into)
            {
                for (int i = 0; i < SaveData.Count; ++i)
                {
                    if (into.ContainsKey(SaveData.GetName(i)))
                    {
                        into[SaveData.GetName(i)].SetValue(SaveData.GetValue(i));
                        if (VERBOSE) Logger.LogInfo("Load into " + SaveData.GetName(i) + " value " + SaveData.GetValue(i), LOG_ID);
                    }
                }
            }
            public static void ClearAndSave()
            {
                SaveData.ClearAndSave();
                if (VERBOSE) Logger.LogInfo("Cleared all save data.", LOG_ID);
            }
            public static string Lookup(string name)
            {
                return SaveData.Lookup(name);
            }

            [Serializable]
            public class SettingsSaveData
            {
                [SerializeField] protected List<string> names = new List<string>();
                [SerializeField] protected List<string> values = new List<string>();

                private Dictionary<string, string> nameValuePairs = new Dictionary<string, string>();

                public int Count => values.Count;
                public string GetName(int i)
                {
                    return names[i];
                }
                public string GetValue(int i)
                {
                    return values[i];
                }

                /// <summary>
                /// Returns the value or null if it dne.
                /// </summary>
                /// <param name="name"></param>
                /// <returns></returns>
                public string Lookup(string name)
                {
                    if (nameValuePairs.ContainsKey(name)) return nameValuePairs[name];
                    return null;
                }

                public SettingsSaveData()
                {
                    Load();
                }

                public void ClearAndSave()
                {
                    PlayerPrefs.SetString(PREFS_KEY, "");
                    PlayerPrefs.Save();
                }
                public void Save(Dictionary<string, SettingsUIElement> newData)
                {
                    int i = 0;
                    foreach (var pair in newData)
                    {
                        bool set = false;

                        // Update pre-existing setting
                        if (nameValuePairs.ContainsKey(pair.Key))
                        {
                            for (int j = 0; j < names.Count; ++j)
                            {
                                if (names[j].Equals(pair.Key))
                                {
                                    values[i] = pair.Value.SettingValueObject.ToString();
                                    set = true;
                                    break;
                                }
                            }
                        }

                        // Add new entry
                        if (!set)
                        {
                            names.Add(pair.Key);
                            values.Add(pair.Value.SettingValueObject.ToString());
                            nameValuePairs.Add(pair.Key, pair.Value.SettingValueObject.ToString());
                        }

                        ++i;
                    }

                    PlayerPrefs.SetString(PREFS_KEY, ToJson());
                    PlayerPrefs.Save();
                }
                public void Load()
                {
                    string data = PlayerPrefs.GetString(PREFS_KEY, "");
                    JsonUtility.FromJsonOverwrite(data, this);

                    // Maintain dictionary for fast access
                    nameValuePairs = new Dictionary<string, string>();
                    for (int i = 0; i < names.Count; ++i)
                    {
                        nameValuePairs.Add(names[i], values[i]);
                    }
                }

                protected string ToJson()
                {
                    return JsonUtility.ToJson(this);
                }
            }
        }

    }
}
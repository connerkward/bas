// SettingsMenu
// (C) 2024 G8gaming Ltd.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Tracking4All
{
    /// <summary>
    /// Manages and handles displaying settings in the menu.
    /// </summary>
    [DefaultExecutionOrder(-10)]
    public class SettingsManager : MonoBehaviour
    {
        public static SettingsManager Instance;

        [SerializeField] GameObject settingsMenuContent;
        [SerializeField] DropdownSettingsUI dropdownPrefab;
        [SerializeField] TextInputSettingsUI floatPrefab;
        [SerializeField] TextInputSettingsUI intPrefab;
        [SerializeField] ToggleSettingsUI boolPrefab;

        private Dictionary<string,SettingsUIElement> settings = new();

        private void Awake()
        {
            if(Instance != null)
            {
                Logger.LogError("Only 1 settings menu should ever exist at a time!");
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        /// <summary>
        /// Refresh all settings/sync.
        /// </summary>
        public void RefreshDisplay()
        {
            // NOTE: currently only respects visibility.
            foreach(var s in settings)
            {
                Refresh(s.Value);
            }
        }
        private void Refresh(SettingsUIElement e)
        {
            if (!e.SettingVisible) e.gameObject.SetActive(false);
            else e.gameObject.SetActive(true);
        }

        public void AddSetting<T>(EnumSetting<T> setting)
            where T : System.Enum
        {
            DropdownSettingsUI ui = (DropdownSettingsUI)Instantiate(dropdownPrefab);
            ui.Hook(setting, new List<string>(Helpers.GetNames(typeof(T))), System.Convert.ToInt32(setting.Value));
            Add(ui);
        }
        public void AddSetting(FloatSetting setting)
        {
            TextInputSettingsUI ui = (TextInputSettingsUI)Instantiate(floatPrefab);
            ui.Hook(setting, setting.Value);
            Add(ui);
        }
        public void AddSetting(IntSetting setting)
        {
            TextInputSettingsUI ui = (TextInputSettingsUI)Instantiate(intPrefab);
            ui.Hook(setting, setting.Value);
            Add(ui);
        }
        public void AddSetting(BoolSetting setting)
        {
            ToggleSettingsUI ui = (ToggleSettingsUI)Instantiate(boolPrefab);
            ui.Hook(setting, setting.Value);
            Add(ui);
        }

        /// <summary>
        /// Get the current settings state.
        /// </summary>
        /// <returns></returns>
        public List<object> GetSettingsState()
        {
            List<object> o = new List<object>();
            foreach(var s in settings)
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
            var current = GetSettingsState();
            if(oldState.Count != current.Count)
            {
                Logger.LogWarning("Settings state had different lengths, this should never happen. Assuming dirty state.", gameObject.name);
                return true;
            }

            bool dirty = false;
            for(int i = 0; i < oldState.Count; ++i)
            {
                if (!current[i].Equals(oldState[i]))
                {
                    dirty = true;
                    break;
                }
            }

            return dirty;
        }

        private void Add(SettingsUIElement e)
        {
            if (string.IsNullOrWhiteSpace(e.Name))
            {
                Logger.LogError("Tried to add a setting '" + e + "'. But is has an empty name! Please set a name!");
                return;
            }
            if (settings.ContainsKey(e.Name))
            {
                Logger.LogError("Tried to add a duplicate setting '"+e.Name+"'. All settings must have a unique name.");
                return;
            }
            settings.Add(e.Name, e);

            Refresh(e);
        }
        private SettingsUIElement Instantiate(SettingsUIElement uiPrefab)
        {
            return Instantiate(uiPrefab, settingsMenuContent.transform, true);
        }
        public void RemoveSetting(string name)
        {
            if (!settings.ContainsKey(name)) return;
            if(settings[name] != null) Destroy(settings[name].gameObject);
            settings.Remove(name);
        }
        public void RemoveSetting(RuntimeSetting setting)
        {
            RemoveSetting(setting.Name);
        }
    }


}
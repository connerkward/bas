// DropdownSettings
// (C) 2024 G8gaming Ltd.
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Tracking4All
{
    public class DropdownSettingsUI : SettingsUIElement
    {
        [SerializeField] protected Dropdown dropdown;

        public void Hook(RuntimeSetting setting, SettingsManager.AnySettingChanged changed, List<string> options, int defaultValue)
        {
            base.Hook(setting, changed);
            dropdown.ClearOptions();
            dropdown.AddOptions(options);
            dropdown.value = defaultValue;

            dropdown.onValueChanged.AddListener(Changed);
        }

        protected void Changed(int value)
        {
            setting.OnSettingUIChanged(dropdown.options[value].text);
            settingChangedEvent?.Invoke(Name);
        }
    }
}
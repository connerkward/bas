// DropdownSettings
// (C) 2024 G8gaming Ltd.
using UnityEngine;
using UnityEngine.UI;

namespace Tracking4All
{
    public class ToggleSettingsUI : SettingsUIElement
    {
        [SerializeField] protected Toggle toggle;

        public void Hook(RuntimeSetting setting, SettingsManager.AnySettingChanged changed, bool defaultValue)
        {
            base.Hook(setting, changed);
            toggle.isOn = defaultValue;

            toggle.onValueChanged.AddListener(Changed);
        }

        protected void Changed(bool value)
        {
            setting.OnSettingUIChanged(value);
            settingChangedEvent?.Invoke(Name);
        }
    }
}
// FloatSettingsUI
// (C) 2024 G8gaming Ltd.
using UnityEngine;
using UnityEngine.UI;

namespace Tracking4All
{
    public class TextInputSettingsUI : SettingsUIElement
    {
        [SerializeField] private InputField inputField;

        public void Hook(RuntimeSetting setting, SettingsManager.AnySettingChanged changed, float defaultValue)
        {
            base.Hook(setting, changed);
            inputField.text = defaultValue.ToString();

            inputField.onValueChanged.AddListener(Changed);
        }

        protected void Changed(string value)
        {
            setting.OnSettingUIChanged(inputField.text);
            settingChangedEvent?.Invoke(Name);
        }
    }
}
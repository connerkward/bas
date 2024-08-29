// FloatSettingsUI
// (C) 2024 G8gaming Ltd.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Tracking4All
{
    public class TextInputSettingsUI : SettingsUIElement
    {
        [SerializeField] private InputField inputField;

        public void Hook(RuntimeSetting setting, float defaultValue)
        {
            base.Hook(setting);
            inputField.text = defaultValue.ToString();

            inputField.onValueChanged.AddListener(Changed);
        }

        protected void Changed(string value)
        {
            setting.OnChanged(inputField.text);
        }
    }
}
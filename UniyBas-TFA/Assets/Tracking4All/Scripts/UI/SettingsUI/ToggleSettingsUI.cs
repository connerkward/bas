// DropdownSettings
// (C) 2024 G8gaming Ltd.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Tracking4All
{
    public class ToggleSettingsUI : SettingsUIElement
    {
        [SerializeField] protected Toggle toggle;

        public void Hook(RuntimeSetting setting, bool defaultValue)
        {
            base.Hook(setting);
            toggle.isOn = defaultValue;

            toggle.onValueChanged.AddListener(Changed);
        }

        protected void Changed(bool value)
        {
            setting.OnChanged(value);
        }
    }
}
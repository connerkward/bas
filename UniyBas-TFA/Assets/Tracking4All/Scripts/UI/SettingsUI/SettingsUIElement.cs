// SettingsUIElement
// (C) 2024 G8gaming Ltd.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Tracking4All
{
    /// <summary>
    /// Base class for a UI element that works as a setting.
    /// </summary>
    public abstract class SettingsUIElement : MonoBehaviour
    {
        [SerializeField] protected Text nameText;

        public string Name => nameText.text;
        public object SettingValueObject => setting.GetValueObject();
        public bool SettingVisible => setting.Visible;

        protected RuntimeSetting setting;

        public void Hook(RuntimeSetting setting)
        {
            nameText.text = setting.Name;
            this.setting = setting;
        }
    }
}
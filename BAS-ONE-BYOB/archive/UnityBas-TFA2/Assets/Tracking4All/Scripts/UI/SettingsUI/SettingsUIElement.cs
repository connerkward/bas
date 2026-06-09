// SettingsUIElement
// (C) 2024 G8gaming Ltd.
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

        public string Name => setting.Name;
        public object SettingValueObject => setting.GetValue();
        public bool SettingVisible => setting.Visible;

        protected RuntimeSetting setting;
        protected SettingsManager.AnySettingChanged settingChangedEvent;

        public object DefaultSettingValueObject { get; protected set; }

        public void Hook(RuntimeSetting setting, SettingsManager.AnySettingChanged onChange)
        {
            nameText.text = setting.Name;
            this.setting = setting;
            this.settingChangedEvent = onChange;
            DefaultSettingValueObject = setting.GetValue();
        }

        /// <summary>
        /// Tries to set the underlying setting value with the object.
        /// <para>Will silently fail if the value is a mismatch.</para>
        /// </summary>
        /// <param name="value">Often can be the string representing the value OR the value itself.</param>
        public void SetValue(object value)
        {
            if (value.ToString().Equals(this.setting.GetValue().ToString())) return;

            setting.OnSettingUIChanged(value);
        }
    }
}
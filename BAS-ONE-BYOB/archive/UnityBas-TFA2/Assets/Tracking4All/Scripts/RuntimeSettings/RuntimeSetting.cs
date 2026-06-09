// RuntimeSetting
// (C) 2024 G8gaming Ltd.
using UnityEngine;

namespace Tracking4All
{
    /// <summary>
    /// Base class for a serializable setting that works with the menu ui.
    /// </summary>
    [System.Serializable]
    public abstract class RuntimeSetting
    {
        [SerializeField] private string name;
        [SerializeField] private bool visible = true;

        public delegate void OnValueChangedInMenu(object newValue);
        /// <summary>
        /// Invoked when the menu value changes (newValue will only have valid new changes).
        /// </summary>
        public event OnValueChangedInMenu ValueChangedInMenu;

        /// <summary>
        /// The unique name of this setting.
        /// </summary>
        public string Name => name;
        /// <summary>
        /// If true the setting should be visible in the menu/to the user.
        /// </summary>
        public bool Visible => visible;

        protected abstract bool TryParse(object input);
        /// <summary>
        /// Called when the setting changes through the UI, can also be called with an input to make the setting parse.
        /// </summary>
        /// <param name="value"></param>
        public virtual void OnSettingUIChanged(object value)
        {
            if (TryParse(value))
            {
                ValueChangedInMenu?.Invoke(GetValue());
            }
        }

        /// <summary>
        /// Get the underlying setting value.
        /// <para>You can directly cast this to the setting type (ex: float).</para>
        /// </summary>
        /// <returns></returns>
        public abstract object GetValue();
    }
}
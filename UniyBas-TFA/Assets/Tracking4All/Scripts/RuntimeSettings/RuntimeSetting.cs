// RuntimeSetting
// (C) 2024 G8gaming Ltd.
using System.Collections;
using System.Collections.Generic;
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
        public event OnValueChangedInMenu ValueChangedInMenu;

        /// <summary>
        /// The unique name of this setting.
        /// </summary>
        public string Name => name;
        /// <summary>
        /// If true the setting should be visible in the menu/to the user.
        /// </summary>
        public bool Visible => visible;

        public virtual void OnChanged(object value)
        {
            ValueChangedInMenu?.Invoke(value);
        }
        public abstract object GetValueObject();
    }
}
// EnumSetting
// (C) 2024 G8gaming Ltd.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tracking4All
{
    [System.Serializable]
    public class EnumSetting<T> : RuntimeSetting, IRuntimeSetting<T>
    where T : System.Enum
    {
        [SerializeField] protected T value;

        public T Value => value;

        public EnumSetting(T value)
        {
            this.value = value;
        }

        public override void OnChanged(object value)
        {
            base.OnChanged(value);
            // expects string
            this.value = (T)System.Enum.Parse(typeof(T), value.ToString());
        }

        public override object GetValueObject()
        {
            return value;
        }

        public void Set(T t)
        {
            value = t;
        }
    }
}
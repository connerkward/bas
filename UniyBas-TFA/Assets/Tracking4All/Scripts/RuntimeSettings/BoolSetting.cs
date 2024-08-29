// BoolSetting
// (C) 2024 G8gaming Ltd.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tracking4All
{
    [System.Serializable]
    public class BoolSetting : RuntimeSetting, IRuntimeSetting<bool>
    {
        [SerializeField] protected bool value;
        public bool Value => value;

        public override object GetValueObject()
        {
            return value;
        }

        public override void OnChanged(object value)
        {
            base.OnChanged(value);
            this.value = bool.Parse(value.ToString());
        }

        public void Set(bool t)
        {
            value = t;
        }
    }
}
// IntSetting
// (C) 2024 G8gaming Ltd.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tracking4All
{
    [System.Serializable]
    public class IntSetting : RuntimeSetting, IRuntimeSetting<int>
    {
        [SerializeField] protected int value;
        public int Value => value;

        public override object GetValueObject()
        {
            return value;
        }

        public override void OnChanged(object value)
        {
            base.OnChanged(value);
            this.value = int.Parse(value.ToString());
        }

        public void Set(int t)
        {
            value = t;
        }
    }
}
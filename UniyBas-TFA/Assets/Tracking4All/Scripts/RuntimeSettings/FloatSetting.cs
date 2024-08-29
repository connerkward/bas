// FloatSetting
// (C) 2024 G8gaming Ltd.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tracking4All
{
    [System.Serializable]
    public class FloatSetting : RuntimeSetting, IRuntimeSetting<float>
    {
        [SerializeField] protected float value;
        public float Value => value;

        public override object GetValueObject()
        {
            return value;
        }

        public override void OnChanged(object value)
        {
            base.OnChanged(value);
            this.value = float.Parse(value.ToString());
        }

        public void Set(float t)
        {
            value = t;
        }
    }
}
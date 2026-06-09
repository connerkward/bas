// FloatSetting
// (C) 2024 G8gaming Ltd.
using UnityEngine;

namespace Tracking4All
{
    [System.Serializable]
    public class FloatSetting : RuntimeSetting, IRuntimeSetting<float>
    {
        [SerializeField] protected float value;
        public float Value => value;

        public override object GetValue()
        {
            return value;
        }

        public void Set(float t)
        {
            value = t;
        }

        protected override bool TryParse(object input)
        {
            return float.TryParse(input.ToString(), out value);
        }
    }
}
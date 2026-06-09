// BoolSetting
// (C) 2024 G8gaming Ltd.
using UnityEngine;

namespace Tracking4All
{
    [System.Serializable]
    public class BoolSetting : RuntimeSetting, IRuntimeSetting<bool>
    {
        [SerializeField] protected bool value;

        public bool Value => value;

        public override object GetValue()
        {
            return value;
        }

        public void Set(bool t)
        {
            value = t;
        }

        protected override bool TryParse(object input)
        {
            return bool.TryParse(input.ToString(), out value);
        }
    }
}
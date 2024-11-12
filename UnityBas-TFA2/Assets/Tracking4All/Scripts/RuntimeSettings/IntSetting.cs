// IntSetting
// (C) 2024 G8gaming Ltd.
using UnityEngine;

namespace Tracking4All
{
    [System.Serializable]
    public class IntSetting : RuntimeSetting, IRuntimeSetting<int>
    {
        [SerializeField] protected int value;
        public int Value => value;

        public override object GetValue()
        {
            return value;
        }

        public void Set(int t)
        {
            value = t;
        }

        protected override bool TryParse(object input)
        {
            return int.TryParse(input.ToString(), out value);
        }
    }
}
// EnumSetting
// (C) 2024 G8gaming Ltd.
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

        public override object GetValue()
        {
            return value;
        }

        public void Set(T t)
        {
            value = t;
        }

        protected override bool TryParse(object input)
        {
            bool result = System.Enum.TryParse(typeof(T), input.ToString(), false, out input);
            if (result) this.value = (T)input;
            return result;
        }
    }
}
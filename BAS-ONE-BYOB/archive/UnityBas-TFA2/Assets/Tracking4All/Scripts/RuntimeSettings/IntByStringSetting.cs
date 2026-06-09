// IntEnumSetting
// (C) 2024 G8gaming Ltd.
using System.Collections.Generic;
using UnityEngine;

namespace Tracking4All
{
    [System.Serializable]
    /// <summary>
    /// An int option, exposed as a collection of string options to the menu.
    /// </summary>
    public class IntByStringSetting : RuntimeSetting, IRuntimeSetting<int>
    {
        [SerializeField] private int value;

        private List<string> options = new();
        public void UpdateOptions(List<string> options)
        {
            this.options = options;
        }

        public int Value => value;
        public string GetValueString()
        {
            if (Value >= options.Count || value < 0)
            {
                Logger.LogError("A runtime value for a string selector setting was out of range! " + Value);
                return "ERROR";
            }

            return options[Value];
        }

        public IntByStringSetting(int value, List<string> options)
        {
            UpdateOptions(options);
            this.value = value;
        }

        /// <summary>
        /// Underlying value is an integer.
        /// </summary>
        /// <returns></returns>
        public override object GetValue()
        {
            return value;
        }

        public void Set(int i)
        {
            this.value = i;
        }

        protected override bool TryParse(object input)
        {
            // receives string or int
            if (int.TryParse(input.ToString(), out this.value))
            {
                return true;
            }

            for (int i = 0; i < options.Count; ++i)
            {
                if (options[i] == (string)input)
                {
                    this.value = i;
                    return true;
                }
            }

            return false;
        }
    }
}
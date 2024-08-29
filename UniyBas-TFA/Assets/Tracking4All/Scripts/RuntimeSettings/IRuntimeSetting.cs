// IRuntimeSetting
// (C) 2024 G8gaming Ltd.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tracking4All
{
    /// <summary>
    /// Base implementation for a run time setting of a specific type.
    /// </summary>
    /// <typeparam name="TYPE"></typeparam>
    public interface IRuntimeSetting<TYPE>
    {
        public TYPE Value { get; }

        public void Set(TYPE t);
    }
}
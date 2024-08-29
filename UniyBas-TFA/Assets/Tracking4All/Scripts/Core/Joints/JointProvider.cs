// JointProvider
// (C) 2024 G8gaming Ltd.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tracking4All
{
    /// <summary>
    /// Allows you to assign and use a joint provider interface in the unity inspector. 
    /// </summary>
    /// <typeparam name="INDEXER"></typeparam>
    [System.Serializable]
    public class JointProvider<INDEXER> : InterfaceProvider<IJointProvider<INDEXER>>, IJointProvider<INDEXER>
        where INDEXER : System.Enum
    {
        public int DataCount => Provider.DataCount;

        public float TimeSinceLastUpdate => Provider.TimeSinceLastUpdate;
        public bool IsAlive =>Tracking4All.Instance.IsProviderLive(TimeSinceLastUpdate);

        public event IProvider<INDEXER, PuppetJoint>.GroupUpdated OnJointsUpdated
        {
            add { Provider.OnJointsUpdated += value; }

            remove { Provider.OnJointsUpdated -= value; }
        }
        PuppetJoint IProvider<INDEXER, PuppetJoint>.Get(int group, INDEXER index)
        {
            return Provider.Get(group, index);
        }

        PuppetJoint IProvider<INDEXER, PuppetJoint>.Get(int group, int index)
        {
            return Provider.Get(group, index);
        }
    }
}
// JointProvider
// (C) 2024 G8gaming Ltd.
namespace Tracking4All
{
    /// <summary>
    /// Allows you to assign and use a joint provider interface in the unity inspector. 
    /// </summary>
    /// <typeparam name="INDEXER"></typeparam>
    [System.Serializable]
    public class PuppetJointProvider<INDEXER, JOINT_DATA> : InterfaceProvider<IJointProvider<INDEXER, JOINT_DATA>>, IJointProvider<INDEXER, JOINT_DATA>
        where INDEXER : System.Enum
        where JOINT_DATA : PuppetJoint, new()
    {
        public int DataCount => Provider.DataCount;

        public float TimeSinceLastUpdate => Provider.TimeSinceLastUpdate;
        public float LastUpdateTime => Provider.LastUpdateTime;
        public bool IsAlive => Tracking4All.Instance.IsProviderLive(TimeSinceLastUpdate);


        public event IProvider<INDEXER, JOINT_DATA>.GroupUpdated OnJointsUpdated
        {
            add { Provider.OnJointsUpdated += value; }

            remove { Provider.OnJointsUpdated -= value; }
        }

        void IProvider<INDEXER, JOINT_DATA>.DisposeProviderData(int group)
        {
            Provider.DisposeProviderData(group);
        }

        public JOINT_DATA Get(int group, INDEXER index)
        {
            return Provider.Get(group, index);
        }

        public JOINT_DATA Get(int group, int index)
        {
            return Provider.Get(group, index);
        }

        public JOINT_DATA GetAbsoluteJoint(int group, INDEXER indexer)
        {
            return Provider.GetAbsoluteJoint(group, indexer);
        }
    }
}
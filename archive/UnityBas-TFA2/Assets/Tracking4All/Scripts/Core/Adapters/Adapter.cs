using System.Collections;
using UnityEngine;

namespace Tracking4All
{
    /// <summary>
    /// Act as an IProvider<TO, TO_DATA> given FROM_DATA. i.e. parse data and store it for access.
    /// </summary>
    /// <typeparam name="FROM_DATA"></typeparam>
    /// <typeparam name="TO"></typeparam>
    /// <typeparam name="TO_DATA"></typeparam>
    public abstract class Adapter<FROM_DATA, TO, TO_DATA> : IProvider<TO, TO_DATA>
        where TO : System.Enum
        where TO_DATA : IDisposableData, new()
    {
        private TableGroup<TO_DATA> data;

        protected IAdapterSettings adapterSettings;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="groupSize">The number of groups (1 is a good default).</param>
        public Adapter(IAdapterSettings settings, int groupSize = 1)
        {
            data = new TableGroup<TO_DATA>(groupSize, Helpers.GetLength(typeof(TO)));
            adapterSettings = settings;

            if (!Tracking4All.Instance)
            {
                Logger.LogError("Tracking4All must be initialized before adapters are constructed.");
            }
            else
            {
                Tracking4All.Instance.StartCoroutine(AliveWatcher());
            }
        }

        public int DataCount => data.ElementCount;

        protected FROM_DATA WorkingData { get; private set; }

        /// <summary>
        /// The group currently being updated.
        /// </summary>
        protected int WorkingGroup { get; private set; }

        public float LastUpdateTime => lastUpdateTime;
        protected float lastUpdateTime = UnityEngine.Mathf.NegativeInfinity;

        /// <summary>
        /// Called when we want to update the adapter.
        /// </summary>
        /// <param name="group"></param>
        /// <param name="from"></param>
        public virtual void Update(int group, FROM_DATA from)
        {
            WorkingGroup = group;
            WorkingData = from;

            Convert();

            lastUpdateTime = Helpers.GetTime();
        }
        private IEnumerator AliveWatcher()
        {
            while (this != null)
            {
                yield return new WaitUntil(() => ((IProvider<TO, TO_DATA>)this).IsAlive == true);
                // provider is alive

                yield return new WaitUntil(() => ((IProvider<TO, TO_DATA>)this).IsAlive == false);
                // provider is dead

                // dispose all groups
                for (int i = 0; i < data.GroupSize; ++i)
                {
                    DisposeProviderData(i);
                    // InvokeUpdateMessage(i);
                    InvokeStoppedMessage(i);
                }
            }
        }

        /// <summary>
        /// Define how to convert using adapter internal state ex: use Set() and Get() methods.
        /// </summary>
        /// <param name="group"></param>
        /// <param name="from"></param>
        protected abstract void Convert();

        /// <summary>
        /// Can be used to convert where the difference in the input and output is just the identifier name.
        /// </summary>
        protected void PassthroughConversion()
        {
            for (int i = 0; i < DataCount; ++i)
            {
                Set(i, Get(i));
            }
        }

        protected void Set(TO index, TO_DATA value)
        {
            data.Set(WorkingGroup, System.Convert.ToInt32(index), value);
        }

        protected void Set(int index, TO_DATA value)
        {
            data.Set(WorkingGroup, index, value);
        }

        protected abstract void InvokeUpdateMessage(int group);
        protected abstract void InvokeStoppedMessage(int group);

        protected abstract TO_DATA Get(int i);

        TO_DATA IProvider<TO, TO_DATA>.Get(int group, TO index)
        {
            return data.Get(group, System.Convert.ToInt32(index));
        }

        TO_DATA IProvider<TO, TO_DATA>.Get(int group, int index)
        {
            return data.Get(group, index);
        }

        public void DisposeProviderData(int group)
        {
            TO_DATA current = default;
            for (int i = 0; i < DataCount; ++i)
            {
                current = Get(i);
                current.Dispose();

                Set(i, current);
            }
        }
    }
}
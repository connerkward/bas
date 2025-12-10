using UnityEngine;

namespace Tracking4All
{
    /// <summary>
    /// Base class for ModifierStack.
    /// </summary>
    /// <typeparam name="DATA_TYPE">The data type which can be modified</typeparam>
    [System.Serializable]
    public abstract class ModifierStack<DATA_TYPE> : MonoBehaviour, IModifier<DATA_TYPE>
    {
        [SerializeField] protected bool enable = true;
        [SerializeField] protected InterfaceProvider<IModifier<DATA_TYPE>>[] modifiers;

        public bool Enabled => enable;

        public void PreCalculate(float deltaTime, int dataCount)
        {
            if (!Enabled) return;

            for (int i = 0; i < modifiers.Length; ++i)
            {
                modifiers[i].Provider.PreCalculate(deltaTime, dataCount);
            }
        }

        public void Modify(int dataIndex, ref DATA_TYPE current, ref DATA_TYPE target, ref bool stayAlive, float deltaTime)
        {
            if (!Enabled) return;

            for (int i = 0; i < modifiers.Length; ++i)
            {
                modifiers[i].Provider.Modify(dataIndex, ref current, ref target, ref stayAlive, deltaTime);
            }
        }

        public void PostCalculate(float deltaTime)
        {
            if (!Enabled) return;

            for (int i = 0; i < modifiers.Length; ++i)
            {
                modifiers[i].Provider.PostCalculate(deltaTime);
            }
        }
    }
}
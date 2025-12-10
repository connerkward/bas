using UnityEngine;

namespace Tracking4All
{
    public abstract class Modifier : MonoBehaviour
    {
        [SerializeField] protected bool enable = true;

        public virtual bool Enabled => enable;

        public virtual void PreCalculate(float deltaTime, int dataCount)
        {

        }
        public virtual void PostCalculate(float deltaTime)
        {

        }
    }
}
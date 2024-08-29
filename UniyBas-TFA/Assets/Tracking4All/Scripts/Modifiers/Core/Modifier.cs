using UnityEngine;

namespace Tracking4All
{
    public abstract class Modifier: MonoBehaviour
    {
        [SerializeField] protected bool enable = true;

        public virtual bool Enabled => enable;

        public virtual void PreUpdate(float deltaTime)
        {

        }
    }
}
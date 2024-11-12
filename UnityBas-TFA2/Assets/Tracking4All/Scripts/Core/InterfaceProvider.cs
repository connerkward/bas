using UnityEngine;

namespace Tracking4All
{
    /// <summary>
    /// Wrapper to get interfaces from Unity GameObjects.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [System.Serializable]
    public class InterfaceProvider<T>
        where T : class
    {
        [SerializeField] protected GameObject provider;
        private T _provider = null;

        /// <summary>
        /// The gameobject the provider sources from.
        /// <para>Might be null/unrelated if the provider was manually explicitly set.</para>
        /// </summary>
        public GameObject Source => provider;
        /// <summary>
        /// Returns true if this provider has a valid interface reference.
        /// </summary>
        public bool HasInterface
        {
            get
            {
                if (_provider == null)
                {
                    _provider = Helpers.GetInterface<T>(provider, true);
                }
                return _provider != null;
            }
        }
        /// <summary>
        /// Check if the internal interface is null or not.
        /// </summary>
        public bool Null => !HasInterface;

        /// <summary>
        /// Get the interface T and use it (will verbosely fail if the provider is not configured correctly).
        /// <para>If you need to silently check if it's null, use Null field or HasInterface field.</para>
        /// </summary>
        public T Provider
        {
            get
            {
                if (_provider == null)
                {
                    TrySetInterface();
                }

                return _provider;
            }
        }

        private void TrySetInterface()
        {
            _provider = Helpers.GetInterface<T>(provider, true);
            if (_provider == null)
            {
                if (provider != null)
                {
                    Logger.LogError("Failed to get " + typeof(T).ToString() + " on " + provider.gameObject.name +
                                    ". The provider gameobject was null or does not impliment this interface...");
                }
                else
                {
                    Logger.LogError("Failed to get " + typeof(T).ToString() + ". The provider gameobject was null or does not impliment this interface...");
                }
            }
        }

        /// <summary>
        /// Override the currently assigned provider game object.
        /// </summary>
        /// <param name="newProvider">The gameobject to provide from.</param>
        public void Set(GameObject newProvider)
        {
            provider = newProvider;
            _provider = null;
            TrySetInterface();
        }
        /// <summary>
        /// Explicitly set the provider interface. 
        /// </summary>
        /// <param name="newInterface">The interface to use.</param>
        /// <param name="source">The source gameobject (can be null).</param>
        public void Set(T newInterface, GameObject source = null)
        {
            if (source != null) provider = source;
            _provider = newInterface;
        }
        /// <summary>
        /// Resets the interface forcing the provider to re-fetch it if possible when next required.
        /// </summary>
        public void Reset()
        {
            _provider = null;
        }

#if UNITY_EDITOR
        /// <summary>
        /// EDITOR ONLY used to determine whether or not an interface is valid.
        /// </summary>
        public bool EditorOnly_HasInterfaceRaw => _provider != null;
#endif
    }
}
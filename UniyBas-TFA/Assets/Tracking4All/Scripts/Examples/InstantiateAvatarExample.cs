// InstantiateAvatarExample
// (C) 2024 G8gaming Ltd.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tracking4All
{
    public class InstantiateAvatarExample : MonoBehaviour
    {
        [SerializeField] Puppet puppet;
        [SerializeField] GameObject instantiateThis;

        private void Start()
        {
            // Instantiate prefab instance.
            GameObject g = Instantiate(instantiateThis);

            // Get or add a new avatar component.
            Avatar a = g.GetComponent<Avatar>();
            if (a == null)
            {
                Debug.LogError("The prefab should have an avatar component!");
                return;
            }

            // Supply the puppet to the avatar here (instantiators responsibility).
            // Alternatively if 'useAnyPuppet' is enabled on the Avatar you can skip this step.
            a.ResetAvatar(puppet);
        }
    }
}
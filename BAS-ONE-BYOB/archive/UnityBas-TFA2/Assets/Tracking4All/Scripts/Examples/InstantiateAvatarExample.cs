// InstantiateAvatarExample
// (C) 2024 G8gaming Ltd.
using System.Collections;
using UnityEngine;

namespace Tracking4All
{
    public class InstantiateAvatarExample : MonoBehaviour
    {
        [SerializeField] PosePuppet puppet;
        [SerializeField] HandPuppet right, left;
        [SerializeField] GameObject instantiateThis; // NOTE: dont make this an already moving avatar :)

        private void Start()
        {
            if (r)
            {
                // Instantiate prefab instance.
                GameObject g = Instantiate(instantiateThis);

                // Get or add a new avatar component.
                Avatar a = g.GetComponent<Avatar>();
                if (a == null)
                {
                    a = g.AddComponent<Avatar>();
                    return;
                }

                // Supply the puppet to the avatar here (instantiators responsibility).
                // Alternatively if 'useAnyPuppet' is enabled on the Avatar you can skip this step.
                a.ResetAvatar(puppet, right, left);
            }

            if (!r) StartCoroutine(wait());
        }

        bool r;
        private IEnumerator wait()
        {
            r = true;
            yield return new WaitForSeconds(2f);
            Start();
        }
    }
}
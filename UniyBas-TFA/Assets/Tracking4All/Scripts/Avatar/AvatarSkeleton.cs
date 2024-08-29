// AvatarAnimator
// (C) 2024 G8gaming Ltd.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tracking4All
{
    /// <summary>
    /// Interface for Avatar to deal with the animator and expose bones.
    /// </summary>
    [RequireComponent(typeof(Animator)), DefaultExecutionOrder(-1)]
    public class AvatarSkeleton : MonoBehaviour
    {
        [SerializeField] private Animator animator;

        private readonly Dictionary<HumanBodyBones, AvatarSkeletonBone> data = new();

        private void OnValidate()
        {
            if(animator==null) animator = GetComponent<Animator>();
            else
            {
                if (animator.avatar == null)
                {
                    Logger.LogError("The Animator that the AvatarSkeleton is using must have a humanoid avatar assigned.", gameObject.name);
                }
            }
        }
        private void Awake()
        {
            ResetSkeleton();
        }
        public void ResetSkeleton()
        {
            if (animator == null) animator = GetComponent<Animator>();

            data.Clear();

            // Create the avatar animator bones.
            HumanBodyBones[] bones = (HumanBodyBones[])System.Enum.GetValues(typeof(HumanBodyBones));
            foreach (var b in bones)
            {
                if (b == HumanBodyBones.LastBone) continue;
                TryAddBone(b);
            }
        }

        private void TryAddBone(HumanBodyBones bone)
        {
            Transform boneTransform = animator.GetBoneTransform(bone);
            if (boneTransform)
            {
                data.Add(bone, new AvatarSkeletonBone(boneTransform));
            }
        }

        /// <summary>
        /// Get the animator avatar bone.
        /// </summary>
        /// <param name="bone"></param>
        /// <returns>The avatar animator bone, null if does not exist.</returns>
        public AvatarSkeletonBone GetBone(HumanBodyBones bone)
        {
            if (!data.ContainsKey(bone)) return null;
            return data[bone];
        }
    }
    
    public class AvatarSkeletonBone
    {
        private Transform bone;
        private Quaternion bindingRotation;

        public Transform BoneTransform => bone;
        public Quaternion BindingRotation => bindingRotation;

        public AvatarSkeletonBone(Transform bone)
        {
            this.bone = bone;
            bindingRotation = bone.rotation;
        }
    }
}
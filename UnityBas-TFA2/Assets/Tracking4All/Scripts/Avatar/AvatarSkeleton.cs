// AvatarAnimator
// (C) 2024 G8gaming Ltd.
using System.Collections.Generic;
using UnityEngine;

namespace Tracking4All
{
    /// <summary>
    /// Drives avatar skeleton.
    /// </summary>
    public interface ISkeletonJointDriver
    {
        /// <summary>
        /// Called when the driver is initialized/created.
        /// </summary>
        public void Initialize(Transform jointAbsoluteParent, HumanBodyBones bone);

        /// <summary>
        /// Update/step the driver.
        /// </summary>
        public void Update(AvatarSkeleton skeleton);

        /// <summary>
        /// Called when the driver is removed from the avatar.
        /// </summary>
        public void Dispose();
    }

    /// <summary>
    /// Interface for Avatar to deal with the animator and expose bones.
    /// </summary>
    [RequireComponent(typeof(Animator)), DefaultExecutionOrder(-5)] // NOTE: this must run before anything related to avatars.
    public class AvatarSkeleton : MonoBehaviour
    {
        // Handles animating the Avatar at a low level.
        [SerializeField] private Animator animator;

        private readonly Dictionary<HumanBodyBones, AvatarSkeletonBone> bones = new();
        // Each human body bone should have only 1 corresponding driver assigned.
        private readonly Dictionary<HumanBodyBones, ISkeletonJointDriver> activeDrivers = new();

        private void OnValidate()
        {
            if (animator == null) animator = GetComponent<Animator>();
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
            BindSkeleton();
        }
        /// <summary>
        /// Updates skeleton bones to treat the current pose as the binding pose for the skeleton.
        /// </summary>
        private void BindSkeleton()
        {
            if (animator == null) animator = GetComponent<Animator>();

            this.bones.Clear();

            // Create the avatar animator bones.
            HumanBodyBones[] bones = (HumanBodyBones[])System.Enum.GetValues(typeof(HumanBodyBones));
            foreach (var bone in bones)
            {
                if (bone == HumanBodyBones.LastBone) continue;

                // Construct bone reference (1 reference for every bone supported by the animator) 
                Transform boneTransform = animator.GetBoneTransform(bone);
                if (boneTransform)
                {
                    this.bones.Add(bone, new AvatarSkeletonBone(boneTransform));
                }
            }
        }

        /// <summary>
        /// Driver requests update at its own tick rate, permit or disallow it.
        /// </summary>
        public void RequestUpdate(HumanBodyBones bone, ISkeletonJointDriver i)
        {
            if (activeDrivers.ContainsKey(bone) && activeDrivers[bone].Equals(i))
            {
                activeDrivers[bone].Update(this);
            }
        }

        /// <summary>
        /// Set the driver at the bone to the inputted driver.
        /// </summary>
        /// <param name="bone"></param>
        /// <param name="driver"></param>
        public void SetDriver(HumanBodyBones bone, ISkeletonJointDriver driver)
        {
            if (activeDrivers.ContainsKey(bone))
            {
                Logger.LogError("Duplicate drivers on avatar (" + bone + "). Only one driver per joint is supported.", gameObject.name);
                return;
            }
            if (!bones.ContainsKey(bone))
            {
                Logger.LogWarning("The avatar does not support driver " + bone + ", is the avatar correctly configued or missing bones?", gameObject.name);
                return;
            }

            _RemoveDriver(bone);
            activeDrivers.Add(bone, driver);
        }
        /// <summary>
        /// Remove the driver at the bone as the inputted driver.
        /// </summary>
        /// <param name="bone"></param>
        /// <param name="driver"></param>
        public void RemoveDriver(HumanBodyBones bone, ISkeletonJointDriver driver)
        {
            if (activeDrivers.ContainsKey(bone) && activeDrivers[bone].Equals(driver))
            {
                _RemoveDriver(bone);
            }
        }
        private void _RemoveDriver(HumanBodyBones bone)
        {
            if (!activeDrivers.ContainsKey(bone)) return;

            activeDrivers[bone].Dispose();
            activeDrivers.Remove(bone);
        }

        /// <summary>
        /// Reset the entire skeleton to the binding pose.
        /// </summary>
        public void ResetPose()
        {
            foreach (var bone in bones)
            {
                bone.Value.Reset();
            }
        }


        // Exposed get/set state of bones.
        public bool HasBone(HumanBodyBones bone)
        {
            return bones.ContainsKey(bone);
        }
        public Quaternion GetBindingRotation(HumanBodyBones bone)
        {
            if (!bones.ContainsKey(bone))
            {
                Logger.LogError("Bone with identifier " + bone + " does not exist is that avatar correctly configured?", gameObject.name);
                return Quaternion.identity;
            }

            return bones[bone].BindingRotation;
        }
        public void SetRotation(HumanBodyBones bone, Quaternion rotation)
        {
            if (!bones.ContainsKey(bone))
            {
                Logger.LogError("Bone with identifier " + bone + " does not exist is that avatar correctly configured?", gameObject.name);
                return;
            }

            bones[bone].BoneTransform.rotation = rotation;
        }
        public Transform GetTransform(HumanBodyBones bone)
        {
            if (!HasBone(bone)) return null;

            return bones[bone].BoneTransform;
        }


        // Internal to only avatar skeleton, do not leak references out since they can be destroyed via bindskeleton.
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

            /// <summary>
            /// Reset the bone to the binding rotation.
            /// </summary>
            public void Reset()
            {
                bone.transform.rotation = BindingRotation;
            }
        }
    }

}
// Avatar
// (C) 2024 G8gaming Ltd.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tracking4All
{
    [RequireComponent(typeof(AvatarSkeleton))]
    /// <summary>
    /// The Avatar receives data from a joint provider to drive the motion of a humanoid using it's animator.
    /// </summary>
    public class Avatar : MonoBehaviour
    {
        [SerializeField] protected JointProvider<PoseJoints> posePuppet;
        [SerializeField] private bool useAnyPuppet = false;
        [SerializeField] protected AvatarSkeleton avatarSkeleton;

        private readonly Dictionary<HumanBodyBones, DriverPair> drivers = new();
        private void AddDriver(PoseJoints joint, HumanBodyBones bone)
        {
            PuppetJoint j = Get(joint);
            if (j == null)
            {
                Logger.LogWarning("Joint provider was unable to provide for " + joint + " joint, is your 3d model correctly configured?",gameObject.name);
                return;
            }
            AvatarSkeletonBone t = avatarSkeleton.GetBone(bone);
            if (t == null)
            {
                Logger.LogWarning("Avatar Animator is missing "+bone + " bone, is your 3d model correctly configured?",gameObject.name);
                return;
            }

            drivers.Add(bone, new DriverPair(j, t));
        }

        private void OnValidate()
        {
            if (avatarSkeleton == null) avatarSkeleton = GetComponent<AvatarSkeleton>();
        }
        private void OnEnable()
        {
            ResetAvatar();
        }
        private void OnDisable()
        {
            ReleaseAvatar();
        }

        /// <summary>
        /// Resets and rebuilds data to use the avatar. Useful if you switch out the provider at runtime.
        /// </summary>
        /// <param name="provider"></param>
        public void ResetAvatar(IJointProvider<PoseJoints> provider = null)
        {
            ReleaseAvatar();

            // Try to get the avatar skeleton if its not assigned.
            if (avatarSkeleton == null)
            {
                avatarSkeleton = GetComponent<AvatarSkeleton>();
            }

            // Set the jointProvider to the method input or the global joint provider otherwise.
            if (provider != null)
            {
                posePuppet.Set(provider, posePuppet.Source);
            }
            // In the worst-case try to bind to any puppet.
            if (posePuppet.Null && useAnyPuppet)
            {
                Puppet found = Puppet.GetAnyPuppet();
                if (found == null)
                {
                    Logger.LogWarning("Failed to find any puppet to automatically bind to.", gameObject.name);
                    return;
                }
                posePuppet.Set((IJointProvider<PoseJoints>)found);
            }

            if (!posePuppet.HasInterface)
            {
                Logger.LogWarning("The joint provider could not be found. " +
                    "Either provide one through this method or assign in the inspector.");
                return;
            }
            if (avatarSkeleton == null)
            {
                Logger.LogWarning("The avatar animator was not found. Assign one through the inspector.");
                return;
            }

            // NOTE: if you are getting null references from passing a provider in and 
            // your interface is coming from a InterfaceProvider, pass InterfaceProvider.Nullable() instead.
            posePuppet.OnJointsUpdated += PoseJointProvider_OnJointsUpdated;

            AddDriver(PoseJoints.Hips, HumanBodyBones.Hips);
            AddDriver(PoseJoints.Spine, HumanBodyBones.Spine);
            AddDriver(PoseJoints.Chest, HumanBodyBones.Chest);
            AddDriver(PoseJoints.Neck, HumanBodyBones.Neck);
            AddDriver(PoseJoints.Head, HumanBodyBones.Head);

            AddDriver(PoseJoints.RightShoulder, HumanBodyBones.RightShoulder);
            AddDriver(PoseJoints.RightUpperArm, HumanBodyBones.RightUpperArm);
            AddDriver(PoseJoints.RightLowerArm, HumanBodyBones.RightLowerArm);
            AddDriver(PoseJoints.RightHand, HumanBodyBones.RightHand);
            AddDriver(PoseJoints.RightUpperLeg, HumanBodyBones.RightUpperLeg);
            AddDriver(PoseJoints.RightLowerLeg, HumanBodyBones.RightLowerLeg);
            AddDriver(PoseJoints.RightFoot, HumanBodyBones.RightFoot);

            AddDriver(PoseJoints.LeftShoulder, HumanBodyBones.LeftShoulder);
            AddDriver(PoseJoints.LeftUpperArm, HumanBodyBones.LeftUpperArm);
            AddDriver(PoseJoints.LeftLowerArm, HumanBodyBones.LeftLowerArm);
            AddDriver(PoseJoints.LeftHand, HumanBodyBones.LeftHand);
            AddDriver(PoseJoints.LeftUpperLeg, HumanBodyBones.LeftUpperLeg);
            AddDriver(PoseJoints.LeftLowerLeg, HumanBodyBones.LeftLowerLeg);
            AddDriver(PoseJoints.LeftFoot, HumanBodyBones.LeftFoot);
        }
        private void ReleaseAvatar()
        {
            drivers.Clear();
            if (posePuppet.HasInterface)
            {
                posePuppet.OnJointsUpdated -= PoseJointProvider_OnJointsUpdated;
            }
        }

        private void PoseJointProvider_OnJointsUpdated(int group)
        {
            // Update the avatar drivers.
            foreach (var k in drivers)
            {
                k.Value.Update(transform);
            }
        }

        protected PuppetJoint Get(PoseJoints joint)
        {
            if (!posePuppet.HasInterface)
            {
                Logger.LogError("The joint provider on the avatar is null. Please assign it before getting data.");
                return null;
            }

            return posePuppet.Provider.Get(0, joint);
        }

        /// <summary>
        /// Just stores data and drives the animator bone given the puppet transform.
        /// </summary>
        protected class DriverPair
        {
            private PuppetJoint joint;
            private AvatarSkeletonBone bone;

            // Now hooks to references so that the puppet joint can reconstruct itself whenever.
            public Transform TrackTransform => joint.Transform;
            public Transform BoneTransform => bone.BoneTransform;
            private Quaternion BoneBindRot => bone.BindingRotation;
            private Quaternion JointBindRot => joint.BindingRotation;

            public bool WellConstructed => BoneTransform == null;

            public DriverPair(PuppetJoint trackPuppetJoint, AvatarSkeletonBone bone)
            {
                this.joint = trackPuppetJoint;
                this.bone = bone;
            }

            public void Update(Transform parent)
            {
                if (!joint.IsWellConstructed)
                {
                    Logger.LogWarning("Tried to update with a badly constructed joint. Make sure joint providers are running before dependencies.");
                        return;
                }

                Quaternion trackRot = Quaternion.LookRotation(
                    parent.localToWorldMatrix.MultiplyVector(TrackTransform.forward),
                    parent.localToWorldMatrix.MultiplyVector(TrackTransform.up));
                BoneTransform.rotation = (trackRot * Quaternion.Inverse(JointBindRot)) * BoneBindRot;
            }
        }
    }
}
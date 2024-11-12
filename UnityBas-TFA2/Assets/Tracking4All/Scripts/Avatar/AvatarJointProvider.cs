// AvatarJointSystem
// (C) 2024 G8gaming Ltd.
using System.Collections.Generic;
using UnityEngine;

namespace Tracking4All
{
    /// <summary>
    /// AvatarJointProvider is a wrapper for JointProvider with additional helpers to make it easy to use with Avatar.
    /// <para>In addition to handling some settings, also handles correctly generating lower-level drivers for the Avatar.</para>
    /// </summary>
    /// <typeparam name="JOINT_INDEXER"></typeparam>
    /// <typeparam name="JOINT_DATA"></typeparam>
    [System.Serializable]
    public abstract class AvatarJointProvider<JOINT_INDEXER, JOINT_DATA, DRIVER>
        where JOINT_INDEXER : System.Enum
        where JOINT_DATA : PuppetJoint, new()
        where DRIVER : ISkeletonJointDriver
    {
        [SerializeField] private PuppetJointProvider<JOINT_INDEXER, JOINT_DATA> puppetProvider;
        [SerializeField] private bool autoSeekPuppet = false;

        private AvatarSkeleton owningSkeleton;

        public bool HasProvider => puppetProvider.HasInterface;

        public AvatarJointProvider()
        {
            puppetProvider = new PuppetJointProvider<JOINT_INDEXER, JOINT_DATA>();
        }

        private readonly Dictionary<HumanBodyBones, DRIVER> drivers = new();
        private void AddDriver(JOINT_INDEXER joint, HumanBodyBones bone)
        {
            PuppetJoint j = GetJointForBinding(joint);

            if (j == null)
            {
                Logger.LogWarning("Joint provider was unable to provide for " + j.PuppetJointTransform.name + " joint, is your 3d model correctly configured?", puppetProvider.Source.name);
                return;
            }

            DRIVER driver = CreateDriver(j, bone);
            drivers.Add(bone, driver);
        }
        public DRIVER GetDriver(HumanBodyBones bone)
        {
            if (drivers.ContainsKey(bone) == false)
            {
                Logger.LogError("Missing driver");
                return default;
            }

            return drivers[bone];
        }
        protected abstract DRIVER CreateDriver(PuppetJoint joint, HumanBodyBones bone);
        private void CleanDrivers(AvatarSkeleton skeleton)
        {
            foreach (var k in drivers)
            {
                skeleton.RemoveDriver(k.Key, k.Value);
            }
            drivers.Clear();
        }

        /// <summary>
        /// Cleans drivers off of skeleton, rebuilds and rebinds them.
        /// </summary>
        /// <param name="skeleton"></param>
        /// <param name="jointProvider"></param>
        public void Reset(AvatarSkeleton skeleton, IJointProvider<JOINT_INDEXER, JOINT_DATA> jointProvider,
            Dictionary<JOINT_INDEXER, HumanBodyBones> humanMapping)
        {
            owningSkeleton = skeleton;

            CleanDrivers(skeleton);

            if (puppetProvider.HasInterface || TryConstructProvider(jointProvider, puppetProvider))
            {
                // NOTE: if you are getting null references from passing a provider in and 
                // your interface is coming from a serialized InterfaceProvider, pass InterfaceProvider.Nullable() instead.
                puppetProvider.OnJointsUpdated += PuppetProvider_OnJointsUpdated;

                foreach (var kp in humanMapping)
                {
                    AddDriver(kp.Key, kp.Value);
                }
            }
        }
        /// <summary>
        /// Cleans drivers off skeleton aand correctly dispose.
        /// </summary>
        /// <param name="avatarSkeleton"></param>
        public void Release(AvatarSkeleton avatarSkeleton)
        {
            CleanDrivers(avatarSkeleton);

            if (puppetProvider.HasInterface)
            {
                puppetProvider.OnJointsUpdated -= PuppetProvider_OnJointsUpdated;
            }
        }

        private void PuppetProvider_OnJointsUpdated(int group)
        {
            foreach (var k in drivers)
            {
                owningSkeleton.RequestUpdate(k.Key, k.Value);
            }
        }

        private bool TryConstructProvider(IJointProvider<JOINT_INDEXER, JOINT_DATA> jointProvider, PuppetJointProvider<JOINT_INDEXER, JOINT_DATA> puppet)
        {
            // Set the jointProvider to the method input or the global joint provider otherwise.
            if (jointProvider != null)
            {
                puppet.Set(jointProvider, puppet.Source);
            }
            // In the worst-case try to bind to any puppet.
            if (puppet.Null && autoSeekPuppet)
            {
                PuppetBase<JOINT_INDEXER, JOINT_DATA> found = PuppetBase<JOINT_INDEXER, JOINT_DATA>.GetPotentialPuppet();
                if (found == null)
                {
                    Logger.LogWarning("Trying to autoseek a puppet but failed to find any puppet to automatically bind to...");
                    return false;
                }
                puppet.Set((IJointProvider<JOINT_INDEXER, JOINT_DATA>)found);
            }

            if (!puppet.HasInterface)
            {
                /*Logger.LogWarning("The joint provider could not be found. " +
                    "Either provide one through this method or assign in the inspector.",gameObject.name);*/
                return false;
            }

            return true;
        }
        protected PuppetJoint GetJointForBinding(JOINT_INDEXER joint)
        {
            if (!puppetProvider.HasInterface)
            {
                Logger.LogError("The joint provider on the avatar is null. Please assign it before getting data.");
                return null;
            }

            return puppetProvider.Provider.GetAbsoluteJoint(0, joint);
        }

    }

    [System.Serializable]
    public class AvatarJointBoneProvider<JOINT_INDEXER, JOINT_DATA> : AvatarJointProvider<JOINT_INDEXER, JOINT_DATA, DirectJointBoneDriver>
        where JOINT_INDEXER : System.Enum
        where JOINT_DATA : PuppetJoint, new()
    {
        protected override DirectJointBoneDriver CreateDriver(PuppetJoint joint, HumanBodyBones bone)
        {
            return new DirectJointBoneDriver(joint);
        }
    }

}
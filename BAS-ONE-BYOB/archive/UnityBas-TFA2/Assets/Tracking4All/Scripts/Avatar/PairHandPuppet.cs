// PairHandPuppet
// (C) 2024 G8gaming Ltd.
using System;
using UnityEngine;

namespace Tracking4All
{
    /// <summary>
    /// Allows you to access 2 hand joint providers as one while respecting mirroring options.
    /// </summary>
    public class PairHandPuppet : MonoBehaviour,
        IHandJointProvider<HandJoints>
    {
        public AdapterSettingsProvider settings;
        public PuppetJointProvider<HandJoints, HandJoint> rightHand;
        public PuppetJointProvider<HandJoints, HandJoint> leftHand;

        public int DataCount => rightHand.DataCount;

        public float LastUpdateTime => lastUpdateTime;

        public event IProvider<HandJoints, HandJoint>.GroupUpdated OnJointsUpdated;

        private float lastUpdateTime;

        private void OnEnable()
        {
            if (rightHand.HasInterface)
            {
                rightHand.OnJointsUpdated += Hand_OnJointsUpdated;
            }

            if (leftHand.HasInterface)
            {
                leftHand.OnJointsUpdated += Hand_OnJointsUpdated;
            }
        }
        private void OnDisable()
        {
            if (rightHand.HasInterface)
            {
                rightHand.OnJointsUpdated -= Hand_OnJointsUpdated;
            }

            if (leftHand.HasInterface)
            {
                leftHand.OnJointsUpdated -= Hand_OnJointsUpdated;
            }
        }

        private void Hand_OnJointsUpdated(int group)
        {
            lastUpdateTime = Helpers.GetTime();

            OnJointsUpdated?.Invoke(group);
        }


        // Access must respect mirroring.

        public void DisposeProviderData(int group)
        {
            if (settings.Mirror)
            {
                group = (int)((Handedness)group).Flip();
            }

            switch ((Handedness)group)
            {
                case Handedness.LEFT:
                    leftHand.Provider.DisposeProviderData(group);
                    break;
                case Handedness.RIGHT:
                    rightHand.Provider.DisposeProviderData(group);
                    break;
            }
        }
        public HandJoint Get(int group, HandJoints index)
        {
            if (settings.Mirror)
            {
                group = (int)((Handedness)group).Flip();
            }

            switch ((Handedness)group)
            {
                case Handedness.LEFT:
                    return leftHand.Get(group, index);
                case Handedness.RIGHT:
                    return rightHand.Get(group, index);
            }

            throw new NotImplementedException();
        }
        public HandJoint Get(int group, int index)
        {
            if (settings.Mirror)
            {
                group = (int)((Handedness)group).Flip();
            }

            switch ((Handedness)group)
            {
                case Handedness.LEFT:
                    return leftHand.Get(group, index);
                case Handedness.RIGHT:
                    return rightHand.Get(group, index);
            }

            throw new NotImplementedException("hand pair puppet " + group);
        }

        public HandJoint GetAbsoluteJoint(int group, HandJoints index)
        {
            // ??? mirror, nah
            switch ((Handedness)group)
            {
                case Handedness.LEFT:
                    return leftHand.Get(group, index);
                case Handedness.RIGHT:
                    return rightHand.Get(group, index);
            }

            throw new NotImplementedException();
        }
    }
}
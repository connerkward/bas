// HandJoint
// (C) 2024 G8gaming Ltd.
using UnityEngine;

namespace Tracking4All
{
    /// <summary>
    /// Hand specific joint data
    /// </summary>
    [System.Serializable]
    public class HandJoint : PuppetJoint
    {
        // Pitch == 'curl'
        // Yaw == 'side to side'

        protected Handedness handedness;

        /// <summary>
        /// Is this joint for a right or left hand?
        /// </summary>
        public Handedness Handedness { get { return handedness; } }

        /// <summary>
        /// The 'zero' yaw for the joint.
        /// </summary>
        public float RestYaw { get; private set; }
        /// <summary>
        /// The 'zero' pitch for the joint.
        /// </summary>
        public float RestPitch { get; private set; }

        /// <summary>
        /// The yaw deviation from the resting yaw. You can use this to determine how much deflection there is from rest.
        /// </summary>
        public float YawDeviation { get; private set; }
        /// <summary>
        /// The pitch deviation from the resting pitch. You can use this to determine how much pitch there is from rest.
        /// </summary>
        public float PitchDeviation { get; private set; }

        public void Reconstruct(HumanBodyBones correspondence, Transform transform, Handedness handedness)
        {
            base.Reconstruct(correspondence, transform);

            this.handedness = handedness;

            RestYaw = transform.localEulerAngles.y;
            if (RestYaw >= 180f) RestYaw -= 360f;

            RestPitch = transform.localEulerAngles.x;
            if (RestPitch >= 180f) RestPitch -= 360f;
        }

        /// <summary>
        /// Set the rotation of this joint using absolute yaw and pitch.
        /// </summary>
        /// <param name="yaw"></param>
        /// <param name="pitch"></param>
        public void SetRotation(float yaw, float pitch)
        {
            PuppetJointTransform.localRotation = Quaternion.Euler(pitch, yaw, 0);
            YawDeviation = yaw - RestPitch;
            PitchDeviation = pitch - RestYaw;
        }
    }

}
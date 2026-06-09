// (Puppet) Joint
// (C) 2024 G8gaming Ltd.
using UnityEngine;

namespace Tracking4All
{
    /// <summary>
    /// The puppet joint contains data defining each joint for a puppet.
    /// <para>Basically a Joint.</para>
    /// </summary>
    [System.Serializable]
    public abstract class PuppetJoint
    {
        [SerializeField] protected HumanBodyBones correspondence;
        [SerializeField] protected Transform transform;

        /// <summary>
        /// The unity scene name of this joint.
        /// </summary>
        public string Name => transform.name;
        /// <summary>
        /// The human body bone equivalent for this joint.
        /// </summary>
        public HumanBodyBones Correspondence => correspondence;
        /// <summary>
        /// The actual transform of the joint on the puppet.
        /// </summary>
        public Transform PuppetJointTransform => transform;

        public Quaternion BindingRotation { get; protected set; }
        public Quaternion BindingLocalRotation { get; protected set; }

        public Vector3 BindingForward => transform.parent.TransformDirection(relativeForward);
        public Vector3 BindingRight => transform.parent.TransformDirection(relativeRight);
        public Vector3 BindingUp => transform.parent.TransformDirection(relativeUp);

        public bool IsWellConstructed { get; protected set; }

        private Vector3 relativeForward, relativeRight, relativeUp;

        public PuppetJoint() { }
        public PuppetJoint(HumanBodyBones correspondence, Transform transform)
        {
            Reconstruct(correspondence, transform);
        }

        /// <summary>
        /// Reconstruct the internal state of the puppet joint.
        /// </summary>
        /// <param name="correspondence"></param>
        /// <param name="transform"></param>
        public void Reconstruct(HumanBodyBones correspondence, Transform transform)
        {
            this.correspondence = correspondence;
            this.transform = transform;
            BindingRotation = transform.rotation;
            BindingLocalRotation = transform.localRotation;

            relativeForward = transform.parent.InverseTransformDirection(transform.forward);
            relativeRight = transform.parent.InverseTransformDirection(transform.right);
            relativeUp = transform.parent.InverseTransformDirection(transform.up);

            IsWellConstructed = true;
        }

        /// <summary>
        /// Reset the puppet joint to its binding rotation.
        /// </summary>
        public void Reset(Space space = Space.World)
        {
            switch (space)
            {
                case Space.World:
                    transform.rotation = BindingRotation;
                    break;

                case Space.Self:
                    transform.localRotation = BindingLocalRotation;
                    break;
            }
        }
    }
}
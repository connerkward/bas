// Joint
// (C) 2024 G8gaming Ltd.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tracking4All
{
    /// <summary>
    /// The puppet joint contains data defining each joint for a puppet.
    /// </summary>
    [System.Serializable]
    public class PuppetJoint
    {
        [SerializeField] protected HumanBodyBones correspondence;
        [SerializeField] protected Transform transform;

        public HumanBodyBones Correspondence => correspondence;
        public Transform Transform => transform;
        public Quaternion BindingRotation { get; protected set; }
        public bool IsWellConstructed { get; protected set; }

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
            IsWellConstructed = true;
        }

        /// <summary>
        /// Reset the puppet joint to its binding rotation.
        /// </summary>
        public void Reset()
        {
            transform.rotation = BindingRotation;
        }
    }
}
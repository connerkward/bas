// PuppetBodyPart
// (C) 2024 G8gaming Ltd.
using UnityEngine;

namespace Tracking4All
{
    /// <summary>
    /// Internal data structure used to store references to puppet body parts.
    /// </summary>
    [System.Serializable]
    public class PuppetBodyPart
    {
        [SerializeField] private string name;
        [SerializeField] private Transform part;
        [SerializeField] private HumanBodyBones correspondence;

        public string Name => name;
        public Transform BodyPart => part;
        public HumanBodyBones Correspondence => correspondence;

        public PuppetBodyPart(string name, Transform part, HumanBodyBones correspondence)
        {
            this.name = name;
            this.part = part;
            this.correspondence = correspondence;
        }

        public void SetCorrespondence(HumanBodyBones correspondence)
        {
            this.correspondence = correspondence;
        }
    }
}
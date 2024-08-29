using System.Collections.Generic;
using UnityEngine;

namespace Tracking4All
{
    public class HandLandmarkVisualizer : LandmarkVisualizer<HandLandmarks>
    {
        [SerializeField] protected Handedness handedness;

        private static readonly List<int[]> EDGE_DEFINITIONS = new List<int[]>()
        {
            new int[] { 0, 5, 9, 13, 17, 0 },
            new int[] { 0, 1, 2, 3, 4 },
            new int[] { 5, 6, 7, 8 },
            new int[] { 9, 10, 11, 12 },
            new int[] { 13, 14, 15, 16 },
            new int[] { 17, 18, 19, 20 }
        };

        public override List<int[]> EdgeDefinitions => EDGE_DEFINITIONS;

        private static readonly HashSet<int> IGNORE_INDICES = new HashSet<int>(
            new int[]
            {
                // 1,2,3,4,5,6
            });

        public override HashSet<int> IgnoreIndicies => IGNORE_INDICES;

        public override int Group => (int)handedness;
    }
}
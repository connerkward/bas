// EuroSmooth
// (C) 2024 G8gaming Ltd.
using System.Collections.Generic;
using UnityEngine;

namespace Tracking4All
{
    /// <summary>
    /// Excels at noise/jitter reduction but can result in loss of fine movement detail if set too agreesively.
    /// </summary>
    public class EuroSmooth : Modifier, ILandmarkModifier, INormalizedLandmarkModifier
    {
        // NOTE: create an instance PER PROVIDER, this modifier CANNOT be shared since it has internal state.

        // higher is smoother (ex: 600), lower is more responsive but also more noisy
        [SerializeField] protected float positionFrequency = 60;
        [SerializeField] protected float occlusionMeasuresFrequency = 15;

        private List<OneEuroFilterVector3> positionFilters = new List<OneEuroFilterVector3>();
        private List<OneEuroFilterFloat> presenceFilters = new List<OneEuroFilterFloat>();
        private List<OneEuroFilterFloat> visibilityFilters = new List<OneEuroFilterFloat>();

        private List<OneEuroFilterVector3> normalizedPositionFilters = new List<OneEuroFilterVector3>();
        private List<OneEuroFilterFloat> normalizedPresenceFilters = new List<OneEuroFilterFloat>();
        private List<OneEuroFilterFloat> normalizedVisibilityFilters = new List<OneEuroFilterFloat>();

        public override void PreCalculate(float deltaTime, int dataCount)
        {
            base.PreCalculate(deltaTime, dataCount);

            // Instantiate all filters
            for (int i = 0; i < dataCount; ++i)
            {
                positionFilters.Add(new OneEuroFilterVector3(positionFrequency));
                presenceFilters.Add(new OneEuroFilterFloat(occlusionMeasuresFrequency));
                visibilityFilters.Add(new OneEuroFilterFloat(occlusionMeasuresFrequency));

                normalizedPositionFilters.Add(new OneEuroFilterVector3(positionFrequency));
                normalizedPresenceFilters.Add(new OneEuroFilterFloat(occlusionMeasuresFrequency));
                normalizedVisibilityFilters.Add(new OneEuroFilterFloat(occlusionMeasuresFrequency));
            }
        }

        public void Modify(int dataIndex, ref Landmark current, ref Landmark target, ref bool stayAlive, float deltaTime)
        {
            current.Position = positionFilters[dataIndex].Filter(target.Position, deltaTime);
            current.Presence = presenceFilters[dataIndex].Filter(target.Presence, deltaTime);
            current.Visibility = visibilityFilters[dataIndex].Filter(target.Visibility, deltaTime);
        }

        public void Modify(int dataIndex, ref NormalizedLandmark current, ref NormalizedLandmark target, ref bool stayAlive, float deltaTime)
        {
            current.Position = normalizedPositionFilters[dataIndex].Filter(target.Position, deltaTime);
            current.Presence = normalizedPresenceFilters[dataIndex].Filter(target.Presence, deltaTime);
            current.Visibility = normalizedVisibilityFilters[dataIndex].Filter(target.Visibility, deltaTime);
        }
    }
}
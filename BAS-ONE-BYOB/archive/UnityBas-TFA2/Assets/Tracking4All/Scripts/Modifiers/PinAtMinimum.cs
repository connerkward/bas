using UnityEngine;

namespace Tracking4All
{
    /// <summary>
    /// Modifies the landmarks to pin at the landmark with the smallest y value.
    /// Ex: good for pinning pose to the ground by the feet/hands/etc.
    /// NOT recommended generally because it increases noise of the entire pose.
    /// </summary>
    public class PinAtMinimum : Modifier, ILandmarkModifier
    {
        [SerializeField] private LandmarkProvider<PoseLandmarks> provider;
        [SerializeField] private Transform optionalGroundTransform;
        [SerializeField] private bool shouldSmooth = false;
        [SerializeField] private float smooth = 10f;

        private float offset;

        public override void PreCalculate(float deltaTime, int dataCount)
        {
            base.PreCalculate(deltaTime, dataCount);

            if (!Enabled) return;

            float minY = Mathf.Infinity;
            for (int i = 0; i < provider.Provider.DataCount; ++i)
            {
                minY = Mathf.Min(minY, provider.Provider.Get(0, i).Position.y);
            }

            if (optionalGroundTransform)
            {
                float pinY = ((GetPoint(PoseLandmarks.LEFT_HIP) + GetPoint(PoseLandmarks.RIGHT_HIP)) / 2f).y;

                if (shouldSmooth)
                {
                    offset = Mathf.Lerp(offset, pinY - minY, deltaTime * smooth);
                }
                else
                {
                    offset = pinY - minY;
                }
            }
        }
        public void Modify(int dataIndex, ref Landmark current, ref Landmark target, ref bool stayAlive, float deltaTime)
        {
            if (!Enabled) return;

            target.Position.y += offset;

        }

        private Vector3 GetPoint(PoseLandmarks l)
        {
            return provider.Provider.Get(0, l).Position;
        }
    }
}

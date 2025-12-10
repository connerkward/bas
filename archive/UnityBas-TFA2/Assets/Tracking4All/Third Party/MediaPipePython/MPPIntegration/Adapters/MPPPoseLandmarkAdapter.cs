using Tracking4All;
using UnityEngine;

public class MPPPoseLandmarkAdapter : LandmarkAdapter<PythonServerSolution.StringData, MPPoseLandmarks>
{
    public MPPPoseLandmarkAdapter(IAdapterSettings settings, int groupSize) : base(settings, groupSize)
    {
    }

    protected override void Convert()
    {
        foreach (string l in WorkingData.lines)
        {
            string[] s = l.Split('|');
            if (s.Length < 4) continue;

            int landmark;
            if (!int.TryParse(s[0], out landmark)) continue;

            position = new Vector3(float.Parse(s[1]), float.Parse(s[2]), float.Parse(s[3]));

            Set(landmark, Get(landmark));
        }
    }

    float visibility = 1, presence = 1;
    Vector3 position;
    protected override Landmark Get(int i)
    {
        if (adapterSettings.PerspectiveFlip)
        {
            return new Landmark(
                // new Vector3(landmark.HasX ? landmark.X : 0, landmark.HasY ? -landmark.Y : 0, landmark.HasZ ? landmark.Z : 0),
                new Vector3(position.x, -position.y, position.z),
                visibility,
                presence
            );
        }
        else
        {
            return new Landmark(
                new Vector3(-position.x, -position.y, -position.z),
                visibility,
                presence
            );
        }
    }
}

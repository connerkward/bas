using Tracking4All;
using UnityEngine;

public class MPPHandLandmarkAdapter : LandmarkAdapter<PythonServerSolution.StringData, MPHandLandmarks>
{
    public MPPHandLandmarkAdapter(IAdapterSettings settings, int groupSize) : base(settings, groupSize)
    {
    }

    protected override void Convert()
    {
        foreach (string l in WorkingData.lines)
        {
            string[] s = l.Split('|');
            if (s.Length < 4) continue;

            int landmarkIndex;
            if (!int.TryParse(s[0], out landmarkIndex)) continue;

            position = new Vector3(float.Parse(s[1]), float.Parse(s[2]), float.Parse(s[3]));

            Set(landmarkIndex, Get(landmarkIndex));
        }
    }

    float visibility = 1, presence = 1;
    Vector3 position;
    protected override Landmark Get(int i)
    {
        if (adapterSettings.PerspectiveFlip)
        {
            return new Landmark(
                new Vector3(-position.x, -position.y, position.z),
                visibility,
                presence
            );
        }
        else
        {
            return new Landmark(
                new Vector3(position.x, -position.y, -position.z),
                visibility,
                presence
            );
        }
    }
}

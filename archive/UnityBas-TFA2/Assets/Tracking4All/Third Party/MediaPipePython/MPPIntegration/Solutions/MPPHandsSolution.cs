using Tracking4All;

public class MPPHandsSolution : PythonServerSolution,
    ILandmarkProvider<MPHandLandmarks>
{
    private string[] temp;
    private string tempS;
    private StringData data = new StringData();

    private MPPHandLandmarkAdapter landmarks;

    int IProvider<MPHandLandmarks, Landmark>.DataCount => landmarks.DataCount;

    public float LastUpdateTime => lastUpdateTime;

    private void Awake()
    {
        landmarks = new MPPHandLandmarkAdapter(this, Helpers.GetLength(typeof(Handedness)));
    }

    protected override void ParseData(string received)
    {
        base.ParseData(received);

        tempS = "";
        temp = received.Split('\n');
        Handedness scanning = Handedness.RIGHT;
        foreach (var s in temp)
        {
            if (string.IsNullOrWhiteSpace(s)) continue;

            if (s == "Right")
            {
                TryPushUpdate(scanning);
                scanning = Handedness.RIGHT;
                tempS = "";
            }
            else if (s == "Left")
            {
                TryPushUpdate(scanning);
                scanning = Handedness.LEFT;
                tempS = "";
            }
            else
            {
                tempS += s + "\n";
            }
        }
        TryPushUpdate(scanning);
    }
    private void TryPushUpdate(Handedness scanning)
    {
        if (tempS != "")
        {
            data.lines = tempS.Split('\n');
            landmarks.Update((int)scanning, data);
        }
    }



    // implement through
    public event IProvider<MPHandLandmarks, Landmark>.GroupUpdated OnLandmarksUpdated
    {
        add
        {
            ((ILandmarkProvider<MPHandLandmarks>)landmarks).OnLandmarksUpdated += value;
        }

        remove
        {
            ((ILandmarkProvider<MPHandLandmarks>)landmarks).OnLandmarksUpdated -= value;
        }
    }

    public event IProvider<MPHandLandmarks, Landmark>.GroupUpdated OnLandmarksStopped
    {
        add
        {
            ((ILandmarkProvider<MPHandLandmarks>)landmarks).OnLandmarksStopped += value;
        }

        remove
        {
            ((ILandmarkProvider<MPHandLandmarks>)landmarks).OnLandmarksStopped -= value;
        }
    }

    public Landmark Get(int group, MPHandLandmarks index)
    {
        return ((IProvider<MPHandLandmarks, Landmark>)landmarks).Get(group, index);
    }
    public Landmark Get(int group, int index)
    {
        return ((IProvider<MPHandLandmarks, Landmark>)landmarks).Get(group, index);
    }

    void IProvider<MPHandLandmarks, Landmark>.DisposeProviderData(int group)
    {
        landmarks.DisposeProviderData(group);
    }
}

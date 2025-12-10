using Tracking4All;

public class MPPPoseSolution : PythonServerSolution,
    ILandmarkProvider<MPPoseLandmarks>
{
    private StringData data = new StringData();

    private MPPPoseLandmarkAdapter landmarks;

    private void Awake()
    {
        landmarks = new MPPPoseLandmarkAdapter(this, 1);
    }

    protected override void ParseData(string received)
    {
        base.ParseData(received);
        data.lines = received.Split('\n');
        landmarks.Update(0, data);
    }



    // Implement through
    public int DataCount => ((IProvider<MPPoseLandmarks, Landmark>)landmarks).DataCount;

    public float LastUpdateTime => lastUpdateTime;

    public event IProvider<MPPoseLandmarks, Landmark>.GroupUpdated OnLandmarksUpdated
    {
        add
        {
            ((ILandmarkProvider<MPPoseLandmarks>)landmarks).OnLandmarksUpdated += value;
        }

        remove
        {
            ((ILandmarkProvider<MPPoseLandmarks>)landmarks).OnLandmarksUpdated -= value;
        }
    }

    public event IProvider<MPPoseLandmarks, Landmark>.GroupUpdated OnLandmarksStopped
    {
        add
        {
            ((ILandmarkProvider<MPPoseLandmarks>)landmarks).OnLandmarksStopped += value;
        }

        remove
        {
            ((ILandmarkProvider<MPPoseLandmarks>)landmarks).OnLandmarksStopped -= value;
        }
    }

    public Landmark Get(int group, MPPoseLandmarks index)
    {
        return ((IProvider<MPPoseLandmarks, Landmark>)landmarks).Get(group, index);
    }
    public Landmark Get(int group, int index)
    {
        return ((IProvider<MPPoseLandmarks, Landmark>)landmarks).Get(group, index);
    }

    void IProvider<MPPoseLandmarks, Landmark>.DisposeProviderData(int group)
    {
        landmarks.DisposeProviderData(group);
    }
}

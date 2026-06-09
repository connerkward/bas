using UnityEngine;

public class PoseExampleCamera : MonoBehaviour
{
    [SerializeField] private Transform track;

    private Vector3 ogPosition;
    private Vector3 ogTrackPosition;

    private void Start()
    {
        if (!track) return;
        ogPosition = transform.position;
        ogTrackPosition = track.position;
    }

    private void LateUpdate()
    {
        if (!track) return;
        transform.position = ogPosition + (track.position - ogTrackPosition);
    }
}

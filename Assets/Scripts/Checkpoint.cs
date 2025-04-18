using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    private CheckpointTracker checkpointTracker;

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<Car>(out Car car))
        {
            checkpointTracker.CarThroughCheckpoint(this, other.transform);
        }
    }

    public void SetTrackCheckpoint(CheckpointTracker checkpointTracker)
    {
        this.checkpointTracker = checkpointTracker;
    }
}

using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    private CheckpointTracker checkpointTracker;

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<PrometeoCarController>(out PrometeoCarController carController))
        {
            checkpointTracker.CarThroughCheckpoint(this, other.transform);
        }
    }

    public void SetTrackCheckpoint(CheckpointTracker checkpointTracker)
    {
        this.checkpointTracker = checkpointTracker;
    }
}

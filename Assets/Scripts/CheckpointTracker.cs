using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.EventSystems;


public class CheckpointTracker : MonoBehaviour
{
    public event EventHandler<CheckpointEventArgs> OnPlayerCorrectCheckpoint;
    public event EventHandler<CheckpointEventArgs> OnPlayerWrongCheckpoint;


    [SerializeField] private List<Transform> carTransformList;

    private List<Checkpoint> checkpointList;
    private List<int> nextCheckpointIndexList;

    private void Awake()
    {
        checkpointList = new List<Checkpoint>();

        foreach (Transform child in transform)
        {
            Checkpoint checkpoint = child.GetComponent<Checkpoint>();

            checkpoint.SetTrackCheckpoint(this);
            
            checkpointList.Add(checkpoint);
        }

        nextCheckpointIndexList = new List<int>();
        foreach (Transform carTransform in carTransformList)
        {
            nextCheckpointIndexList.Add(0);
        }
    }

    public void CarThroughCheckpoint(Checkpoint checkpoint, Transform carTransform)
    {
        int nextCheckpointIndex = nextCheckpointIndexList[carTransformList.IndexOf(carTransform)];
        if (checkpointList.IndexOf(checkpoint) == nextCheckpointIndex)
        {
            Debug.Log("Player passed through checkpoint: " + checkpoint.name);
            nextCheckpointIndexList[carTransformList.IndexOf(carTransform)] = (nextCheckpointIndex + 1) % checkpointList.Count;
            
            OnPlayerCorrectCheckpoint?.Invoke(this, new CheckpointEventArgs(carTransform));
        }
        else
        {
            Debug.LogError("Player passed through the wrong checkpoint!");
            OnPlayerWrongCheckpoint?.Invoke(this, new CheckpointEventArgs(carTransform));
        }
    }

    public Transform GetNextCheckpoint(Transform carTransform)
    {
        return checkpointList[nextCheckpointIndexList[carTransformList.IndexOf(carTransform)]].transform;
    }

    public void ResetCheckpoint(Transform carTransform)
    {
        int index = carTransformList.IndexOf(carTransform);
        if (index >= 0 && index < nextCheckpointIndexList.Count)
        {
            nextCheckpointIndexList[index] = 0;
        }
        else
        {
            Debug.LogError("Car transform not found in the list.");
        }
    }
}

public class CheckpointEventArgs : EventArgs
{
    public Transform CarTransform { get; }

    public CheckpointEventArgs(Transform carTransform)
    {
        CarTransform = carTransform;
    }
}
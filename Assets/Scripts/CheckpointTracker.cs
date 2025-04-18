using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.EventSystems;


public class CheckpointTracker : MonoBehaviour
{
    public event EventHandler OnPlayerCorrectCheckpoint;
    public event EventHandler OnPlayerWrongCheckpoint;

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
            // Agent add reward
            Debug.Log("Player passed through checkpoint: " + checkpoint.name);
            nextCheckpointIndexList[carTransformList.IndexOf(carTransform)] = (nextCheckpointIndex + 1) % checkpointList.Count;
            OnPlayerCorrectCheckpoint?.Invoke(this, EventArgs.Empty);
        }
        else
        {
            // Agent add penalty
            Debug.LogError("Player passed through the wrong checkpoint!");
            OnPlayerWrongCheckpoint?.Invoke(this, EventArgs.Empty);
        }
    }
}
using System;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class CarAgent : Agent
{
    [SerializeField] private CarController carController;
    [SerializeField] private CheckpointTracker checkpointTracker;

    private int stuckSteps = 0;
    private const int maxStuckSteps = 50;
    private float prevDistanceToCheckpoint = -1f;
    private int wallCollisionCount = 0;

    private Vector3 startingPos;

    void Start()
    {
        startingPos = transform.position;
        carController = GetComponent<CarController>();
        checkpointTracker.OnPlayerCorrectCheckpoint += CheckpointTracker_OnPlayerCorrectCheckpoint;
        checkpointTracker.OnPlayerWrongCheckpoint += CheckpointTracker_OnPlayerWrongCheckpoint;
    }

    private void CheckpointTracker_OnPlayerCorrectCheckpoint(object sender, CheckpointEventArgs e)
    {
        if (e.CarTransform != null)
        {
            AddReward(0.1f);
        }
    }

    private void CheckpointTracker_OnPlayerWrongCheckpoint(object sender, CheckpointEventArgs e)
    {
        if (e.CarTransform != null)
        {
            AddReward(-1.0f);
        }
    }

    public override void OnEpisodeBegin()
    {
        checkpointTracker.ResetCheckpoint(carController.transform);
        carController.StopCompletely();
        stuckSteps = 0;
        prevDistanceToCheckpoint = -1f;
        ResetPosition();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        Vector3 checkpointForward = checkpointTracker.GetNextCheckpoint(carController.transform).transform.forward;
        float directionDot = Vector3.Dot(carController.transform.forward, checkpointForward);
        sensor.AddObservation(directionDot);

        Vector3 directionToCheckpoint = checkpointTracker.GetNextCheckpoint(carController.transform).transform.position - transform.position;
        float angleToCheckpoint = Vector3.SignedAngle(transform.forward, directionToCheckpoint.normalized, Vector3.up);
        sensor.AddObservation(angleToCheckpoint / 180f);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        AddReward(-0.001f);

        float forwardAmount = 0f;
        float turnAmount = 0f;

        switch (actions.DiscreteActions[0])
        {
            case 0: forwardAmount = 0f; break;
            case 1: forwardAmount = +1f; break;
            case 2: forwardAmount = -1f; break;
        }

        switch (actions.DiscreteActions[1])
        {
            case 0: turnAmount = 0f; break;
            case 1: turnAmount = +1f; break;
            case 2: turnAmount = -1f; break;
        }

        Vector3 checkpointPos = checkpointTracker.GetNextCheckpoint(transform).transform.position;
        float distanceToCheckpoint = Vector3.Distance(transform.position, checkpointPos);
        if (prevDistanceToCheckpoint > 0f)
        {
            float progress = prevDistanceToCheckpoint - distanceToCheckpoint;
            AddReward(progress * 0.01f);
        }
        prevDistanceToCheckpoint = distanceToCheckpoint;

        // End episode if stuck
        if (carController.CurrentSpeed < 0.1f)
            stuckSteps++;
        else
            stuckSteps = 0;

        if (stuckSteps > maxStuckSteps)
        {
            AddReward(-1.0f);
            EndEpisode();
        }

        carController.SetInputs(forwardAmount, turnAmount);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActions = actionsOut.DiscreteActions;

        if (Input.GetKey(KeyCode.W))
            discreteActions[0] = 1; //forward
        else if (Input.GetKey(KeyCode.S))
            discreteActions[0] = 2; //reverse
        else
            discreteActions[0] = 0; //not moving

        if (Input.GetKey(KeyCode.D))
            discreteActions[1] = 1; //right
        else if (Input.GetKey(KeyCode.A))
            discreteActions[1] = 2; //left
        else
            discreteActions[1] = 0; //not turning
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Reset"))
        {
            AddReward(-0.5f * wallCollisionCount);
            wallCollisionCount++;
            EndEpisode();
        }
    }

    private void ResetPosition()
    {
        transform.position = startingPos;
        transform.forward = -Vector3.forward;
    }
}

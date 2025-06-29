using System;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class CarAgent : Agent
{
    [SerializeField] private CarController carController;
    [SerializeField] private CheckpointTracker checkpointTracker;

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
            AddReward(1.0f);
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
        ResetPosition();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        Vector3 checkpointForward = checkpointTracker.GetNextCheckpoint(carController.transform).transform.forward;
        float directionDot = Vector3.Dot(carController.transform.forward, checkpointForward);
        sensor.AddObservation(directionDot);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float forwardAmount = 0f;
        float turnAmount = 0f;

        switch (actions.DiscreteActions[0])
        {
            case 0: //not moving
                forwardAmount = 0f;
                break;
            case 1: //moving forward
                forwardAmount = +1f;
                break;
            case 2: //moving reverse
                forwardAmount = -1f;
                break;
        }

        switch (actions.DiscreteActions[1])
        {
            case 0: //not turning
                turnAmount = 0f;
                break;
            case 1: //turning right
                turnAmount = +1f;
                break;
            case 2: //turning left
                turnAmount = -1f;
                break;
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

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Walls"))
        {
            AddReward(-0.5f);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Reset"))
        {
            AddReward(-1.0f);
            EndEpisode();
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Walls"))
        {
            AddReward(-0.1f);
        }
    }

    private void ResetPosition()
    {
        transform.position = startingPos;
        transform.forward = -Vector3.forward;
    }
}

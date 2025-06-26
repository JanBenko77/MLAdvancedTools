using System;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class CarAgent : Agent
{
    [SerializeField] private PrometeoCarController carController;
    [SerializeField] private CheckpointTracker checkpointTracker;

    private Transform startingPos;

    void Start()
    {
        startingPos = transform;
        carController = GetComponent<PrometeoCarController>();
        checkpointTracker = FindAnyObjectByType<CheckpointTracker>();
        checkpointTracker.OnPlayerCorrectCheckpoint += CheckpointTracker_OnPlayerCorrectCheckpoint;
        checkpointTracker.OnPlayerWrongCheckpoint += CheckpointTracker_OnPlayerWrongCheckpoint;
    }

    void Update()
    {
        
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
        transform.position = startingPos.position;
        transform.forward = startingPos.forward;
        carController.ThrottleOff();
    }


    public override void CollectObservations(VectorSensor sensor)
    {
        Vector3 checkpointForward = checkpointTracker.GetNextCheckpoint(carController.transform).transform.forward;
        float directionDot = Vector3.Dot(carController.transform.forward, checkpointForward);
        sensor.AddObservation(directionDot);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        bool forward = false;
        bool back = false;
        bool turning = false;
        bool left = false;
        bool handBreak = false;

        switch (actions.DiscreteActions[0])
        {
            case 0: forward = false; break;
            case 1: forward = true; break;
        }

        switch (actions.DiscreteActions[1])
        {
            case 0: if (forward == false); break;
            case 1: if (forward == true) back = false; break;
            case 2: if (forward == true) back = true; break;
        }

        switch (actions.DiscreteActions[2])
        {
            case 0: turning = false; break;
            case 1: turning = true; break;
        }

        switch (actions.DiscreteActions[3])
        {
            case 0: if (turning == false); break;
            case 1: if (turning == true) left = false; break;
            case 2: if (turning == true) left = true; break;
        }

        switch (actions.DiscreteActions[4])
        {
            case 0: handBreak = false; break;
            case 1: handBreak = true; break;
        }

        carController.SetInputs(forward, back, turning, left, handBreak);
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
}

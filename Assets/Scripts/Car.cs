using NUnit.Framework;
using System;
using UnityEngine;
using System.Collections.Generic;

public class Car : MonoBehaviour
{
    public enum Axel
    {
        Front,
        Rear
    }

    [Serializable]
    public struct Wheel
    {
        public GameObject wheelModel;
        public WheelCollider wheelCollider;
        public Axel axel;
    }

    [SerializeField] private List<Wheel> wheels;
    [SerializeField] private Vector3 centerOfMassEffect;

    #region Movement
    [SerializeField] private float maxAcceleration = 30.0f;
    [SerializeField] private float breakAcceleration = 50.0f;
    private float moveInput;
    #endregion

    #region Turning
    [SerializeField] private float turnSensitivity = 1.0f;
    [SerializeField] private float maxSteeringAngle = 30.0f;
    private float turnInput;
    #endregion


    private Rigidbody carRb;

    private void Start()
    {
        carRb = GetComponent<Rigidbody>();
        carRb.centerOfMass = centerOfMassEffect;

        foreach (Wheel wheel in wheels)
        {
            wheel.wheelCollider.wheelDampingRate = 0.5f;
        }
    }

    private void Update()
    {
        GetInputs();
        AnimateWheels();
    }

    private void FixedUpdate()
    {
        SetInputs();
    }

    private void GetInputs()
    {
        moveInput = Input.GetAxis("Vertical");
        turnInput = Input.GetAxis("Horizontal");
    }

    private void SetInputs()
    {
        foreach (Wheel wheel in wheels)
        {
            // Steering — keep always active
            if (wheel.axel == Axel.Front)
            {
                var turnAngle = turnInput * turnSensitivity * maxSteeringAngle;
                wheel.wheelCollider.steerAngle = Mathf.Lerp(wheel.wheelCollider.steerAngle, turnAngle, 0.6f);
            }

            // Acceleration
            if (Mathf.Abs(moveInput) > 0.05f)
            {
                wheel.wheelCollider.motorTorque = moveInput * maxAcceleration;
                wheel.wheelCollider.brakeTorque = 0f;
            }
            else
            {
                wheel.wheelCollider.motorTorque = 0f;

                // Light brake when idle, but LESS on front wheels to avoid steering lock
                if (wheel.axel == Axel.Rear)
                {
                    wheel.wheelCollider.brakeTorque = breakAcceleration * 0.5f;
                }
                else
                {
                    wheel.wheelCollider.brakeTorque = breakAcceleration * 0.2f;
                }
            }

            // Manual brakes override
            if (Input.GetKey(KeyCode.Space))
            {
                wheel.wheelCollider.brakeTorque = breakAcceleration;
            }
        }
    }

    private void AnimateWheels()
    {
        //foreach (Wheel wheel in wheels)
        //{
        //    Quaternion rot;
        //    Vector3 pos;
        //    wheel.wheelCollider.GetWorldPose(out pos, out rot);
        //    wheel.wheelModel.transform.position = pos;
        //    wheel.wheelModel.transform.rotation = rot;
        //}
    }
}
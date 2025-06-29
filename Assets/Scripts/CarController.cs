using UnityEngine;

public class CarController : MonoBehaviour
{
    [Header("Car Settings")]
    [SerializeField] private float maxSpeed = 10f;
    [SerializeField] private float acceleration = 5f;
    [SerializeField] private float deceleration = 20f;
    [SerializeField] private float turnSpeed = 120f;

    private Rigidbody rb;
    private float currentSpeed = 0f;
    private float inputForward = 0f;
    private float inputTurn = 0f;

    public Vector3 GetVelocity() => rb.linearVelocity;
    public float MaxSpeed => maxSpeed;
    public float CurrentSpeed => currentSpeed;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.constraints = RigidbodyConstraints.FreezeRotationZ | RigidbodyConstraints.FreezeRotationX;
    }

    private void FixedUpdate()
    {
        ApplyMovement();
        ApplyTurning();
    }

    private void ApplyMovement()
    {
        float targetSpeed = inputForward * maxSpeed;

        if (Mathf.Abs(targetSpeed) > 0.01f)
        {
            currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, acceleration * Time.fixedDeltaTime);
        }
        else
        {
            currentSpeed = Mathf.MoveTowards(currentSpeed, 0f, deceleration * Time.fixedDeltaTime);
        }

        Vector3 velocity = transform.forward * currentSpeed;
        rb.linearVelocity = new Vector3(velocity.x, rb.linearVelocity.y, velocity.z);
    }

    private void ApplyTurning()
    {
        if (Mathf.Abs(rb.linearVelocity.magnitude) > 0.1f && Mathf.Abs(inputTurn) > 0.01f)
        {
            float turnAmount = inputTurn * turnSpeed * Time.fixedDeltaTime;
            Quaternion turnRotation = Quaternion.Euler(0f, turnAmount, 0f);
            rb.MoveRotation(rb.rotation * turnRotation);
        }
    }

    public void SetInputs(float forwardAmount, float turnAmount)
    {
        inputForward = Mathf.Clamp(forwardAmount, -1f, 1f);
        inputTurn = Mathf.Clamp(turnAmount, -1f, 1f);
    }

    public void StopCompletely()
    {
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        inputForward = 0f;
        inputTurn = 0f;
        currentSpeed = 0f;
    }
}
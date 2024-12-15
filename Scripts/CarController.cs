using UnityEngine;

public class CarController : MonoBehaviour
{
    [Header("Car Settings")]
    public float maxSpeed = 200f;
    public float motorForce = 1500f;
    public float brakeForce = 3000f;
    public float maxSteerAngle = 30f;

    [Header("Input Values")]
    public float horizontalInput = 0f;
    public float verticalInput = 0f;
    public bool isBraking = false;

    [Header("Wheel Colliders")]
    public WheelCollider frontLeftWheelCollider;
    public WheelCollider frontRightWheelCollider;
    public WheelCollider rearLeftWheelCollider;
    public WheelCollider rearRightWheelCollider;

    [Header("Wheel Transforms")]
    public Transform frontLeftWheelTransform;
    public Transform frontRightWheelTransform;
    public Transform rearLeftWheelTransform;
    public Transform rearRightWheelTransform;

    private Rigidbody rb;
    private AudioSource engineSound;
    private float currentSpeed;
    private bool isEngineOn = true;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        engineSound = GetComponent<AudioSource>();

        if (rb != null)
        {
            rb.mass = 1525f;
            rb.linearDamping = 0.01f;
            rb.angularDamping = 0.5f;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
        }
    }

    private void FixedUpdate()
    {
        if (!isEngineOn) return;

        // Handle steering
        float steerAngle = maxSteerAngle * horizontalInput;
        frontLeftWheelCollider.steerAngle = steerAngle;
        frontRightWheelCollider.steerAngle = steerAngle;

        // Get current wheel speeds
        float frontLeftSpeed = frontLeftWheelCollider.rpm;
        float frontRightSpeed = frontRightWheelCollider.rpm;
        float rearLeftSpeed = rearLeftWheelCollider.rpm;
        float rearRightSpeed = rearRightWheelCollider.rpm;
        float averageSpeed = (frontLeftSpeed + frontRightSpeed + rearLeftSpeed + rearRightSpeed) / 4f;

        // Handle motor and brakes
        if (isBraking)
        {
            // Apply full brakes when brake button is pressed
            frontLeftWheelCollider.brakeTorque = brakeForce;
            frontRightWheelCollider.brakeTorque = brakeForce;
            rearLeftWheelCollider.brakeTorque = brakeForce;
            rearRightWheelCollider.brakeTorque = brakeForce;

            frontLeftWheelCollider.motorTorque = 0;
            frontRightWheelCollider.motorTorque = 0;
            rearLeftWheelCollider.motorTorque = 0;
            rearRightWheelCollider.motorTorque = 0;
        }
        else if (verticalInput > 0)
        {
            // Forward movement
            float motorTorque = verticalInput * motorForce;
            frontLeftWheelCollider.motorTorque = motorTorque;
            frontRightWheelCollider.motorTorque = motorTorque;
            rearLeftWheelCollider.motorTorque = motorTorque;
            rearRightWheelCollider.motorTorque = motorTorque;

            frontLeftWheelCollider.brakeTorque = 0;
            frontRightWheelCollider.brakeTorque = 0;
            rearLeftWheelCollider.brakeTorque = 0;
            rearRightWheelCollider.brakeTorque = 0;
        }
        else if (verticalInput < 0)
        {
            // First apply brakes if moving forward
            if (averageSpeed > 1f)
            {
                frontLeftWheelCollider.brakeTorque = brakeForce;
                frontRightWheelCollider.brakeTorque = brakeForce;
                rearLeftWheelCollider.brakeTorque = brakeForce;
                rearRightWheelCollider.brakeTorque = brakeForce;
                
                frontLeftWheelCollider.motorTorque = 0;
                frontRightWheelCollider.motorTorque = 0;
                rearLeftWheelCollider.motorTorque = 0;
                rearRightWheelCollider.motorTorque = 0;
            }
            else
            {
                // Apply reverse thrust once nearly stopped
                float reverseTorque = verticalInput * motorForce * 0.7f; // Increased reverse power
                frontLeftWheelCollider.motorTorque = reverseTorque;
                frontRightWheelCollider.motorTorque = reverseTorque;
                rearLeftWheelCollider.motorTorque = reverseTorque;
                rearRightWheelCollider.motorTorque = reverseTorque;

                frontLeftWheelCollider.brakeTorque = 0;
                frontRightWheelCollider.brakeTorque = 0;
                rearLeftWheelCollider.brakeTorque = 0;
                rearRightWheelCollider.brakeTorque = 0;
            }
        }
        else
        {
            // No input, let the car coast with minimal braking
            frontLeftWheelCollider.motorTorque = 0;
            frontRightWheelCollider.motorTorque = 0;
            rearLeftWheelCollider.motorTorque = 0;
            rearRightWheelCollider.motorTorque = 0;

            float coastBrake = 100f; // Light braking when coasting
            frontLeftWheelCollider.brakeTorque = coastBrake;
            frontRightWheelCollider.brakeTorque = coastBrake;
            rearLeftWheelCollider.brakeTorque = coastBrake;
            rearRightWheelCollider.brakeTorque = coastBrake;
        }

        UpdateWheelPoses();
        UpdateSpeed();
    }

    private void Update()
    {
        // Get input
        horizontalInput = Input.GetAxis("Horizontal");
        verticalInput = Input.GetAxis("Vertical");
        
        // Check for reverse input (S key)
        bool isReversing = Input.GetKey(KeyCode.S);
        
        // Calculate motor force based on input and direction
        float currentMotorForce = isReversing ? -motorForce * 0.7f : motorForce;  // Half power in reverse
        
        // Apply motor force to wheels
        if (Mathf.Abs(verticalInput) > 0.1f)
        {
            frontLeftWheelCollider.motorTorque = verticalInput * currentMotorForce;
            frontRightWheelCollider.motorTorque = verticalInput * currentMotorForce;
            rearLeftWheelCollider.motorTorque = verticalInput * currentMotorForce;
            rearRightWheelCollider.motorTorque = verticalInput * currentMotorForce;
        }
        else
        {
            frontLeftWheelCollider.motorTorque = 0;
            frontRightWheelCollider.motorTorque = 0;
            rearLeftWheelCollider.motorTorque = 0;
            rearRightWheelCollider.motorTorque = 0;
        }

        // Apply steering
        float steerAngle = horizontalInput * maxSteerAngle;
        frontLeftWheelCollider.steerAngle = steerAngle;
        frontRightWheelCollider.steerAngle = steerAngle;

        // Update wheel visuals
        UpdateWheelPoses();
    }

    private void UpdateWheelPoses()
    {
        UpdateWheelPose(frontLeftWheelCollider, frontLeftWheelTransform);
        UpdateWheelPose(frontRightWheelCollider, frontRightWheelTransform);
        UpdateWheelPose(rearLeftWheelCollider, rearLeftWheelTransform);
        UpdateWheelPose(rearRightWheelCollider, rearRightWheelTransform);
    }

    private void UpdateWheelPose(WheelCollider collider, Transform transform)
    {
        if (collider == null || transform == null) return;

        Vector3 pos;
        Quaternion rot;
        collider.GetWorldPose(out pos, out rot);
        transform.position = pos;
        transform.rotation = rot;
    }

    private void UpdateSpeed()
    {
        if (rb != null)
        {
            currentSpeed = rb.linearVelocity.magnitude * 3.6f; // Convert to km/h
        }
    }

    public float GetCurrentSpeed()
    {
        return currentSpeed;
    }

    public void StopEngine()
    {
        isEngineOn = false;
        if (engineSound != null)
        {
            engineSound.Stop();
        }
    }

    public void MuteEngine(bool mute)
    {
        if (engineSound != null)
        {
            engineSound.mute = mute;
        }
    }
}

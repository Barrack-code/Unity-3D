using UnityEngine;

public class CarControllerBackup : MonoBehaviour
{
    [Header("Car Settings")]
    public float motorForce = 1500f;
    public float brakeForce = 3000f;
    public float maxSteerAngle = 30f;

    [Header("Wheel Colliders")]
    public WheelCollider frontLeftWheelCollider;
    public WheelCollider frontRightWheelCollider;
    public WheelCollider rearLeftWheelCollider;
    public WheelCollider rearRightWheelCollider;

    [Header("Wheel Transforms (Initial Local Positions)")]
    public Vector3 frontLeftWheelLocalPos;
    public Vector3 frontRightWheelLocalPos;
    public Vector3 rearLeftWheelLocalPos;
    public Vector3 rearRightWheelLocalPos;

    [Header("Input Keys")]
    public KeyCode forwardKey = KeyCode.W;
    public KeyCode backwardKey = KeyCode.S;
    public KeyCode leftKey = KeyCode.A;
    public KeyCode rightKey = KeyCode.D;
    public KeyCode brakeKey = KeyCode.Space;

    private float horizontalInput;
    private float verticalInput;
    private bool isBreaking;
    private float currentSteeringAngle;
    private Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();

        // Verify wheel colliders are assigned
        if (!VerifyWheelColliders())
        {
            Debug.LogError("Wheel colliders not assigned on " + gameObject.name);
            enabled = false;
            return;
        }

        // Configure the rigidbody
        if (rb != null)
        {
            rb.mass = 1000f;
            rb.linearDamping = 0.05f;
            rb.angularDamping = 0.05f;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

            // Lower center of mass for better stability
            rb.centerOfMass = new Vector3(0, -0.5f, 0);
        }
    }

    private bool VerifyWheelColliders()
    {
        return frontLeftWheelCollider != null &&
               frontRightWheelCollider != null &&
               rearLeftWheelCollider != null &&
               rearRightWheelCollider != null;
    }

    private void Update()
    {
        GetInput();
    }

    private void FixedUpdate()
    {
        HandleMotor();
        HandleSteering();
        UpdateWheelsRotation();
    }

    private void GetInput()
    {
        // Handle steering
        if (Input.GetKey(leftKey))
        {
            horizontalInput = -1f;
        }
        else if (Input.GetKey(rightKey))
        {
            horizontalInput = 1f;
        }
        else
        {
            horizontalInput = 0f;
        }

        // Handle acceleration/reverse
        if (Input.GetKey(forwardKey))
        {
            verticalInput = 1f;
        }
        else if (Input.GetKey(backwardKey))
        {
            verticalInput = -1f;
        }
        else
        {
            verticalInput = 0f;
        }

        // Handle braking
        isBreaking = Input.GetKey(brakeKey);
    }

    private void HandleMotor()
    {
        // Apply motor torque to front wheels
        float motorTorque = verticalInput * motorForce;
        frontLeftWheelCollider.motorTorque = motorTorque;
        frontRightWheelCollider.motorTorque = motorTorque;

        // Apply braking
        float brakeTorque = isBreaking ? brakeForce : 0f;
        ApplyBraking(brakeTorque);
    }

    private void ApplyBraking(float brakeTorque)
    {
        frontLeftWheelCollider.brakeTorque = brakeTorque;
        frontRightWheelCollider.brakeTorque = brakeTorque;
        rearLeftWheelCollider.brakeTorque = brakeTorque;
        rearRightWheelCollider.brakeTorque = brakeTorque;
    }

    private void HandleSteering()
    {
        currentSteeringAngle = maxSteerAngle * horizontalInput;
        frontLeftWheelCollider.steerAngle = currentSteeringAngle;
        frontRightWheelCollider.steerAngle = currentSteeringAngle;
    }

    private void UpdateWheelsRotation() // Update only rotation, not position
    {
        UpdateSingleWheelRotation(frontLeftWheelCollider);
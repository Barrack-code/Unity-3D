using UnityEngine;
using System.Collections;
using UnityEngine.UI;  
using TMPro;  

public class Backup : MonoBehaviour
{
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

    [Header("Car Settings")]
    public float motorForce = 1000f;
    public float brakeForce = 3000f;
    public float maxSteerAngle = 30f;
    public float maxSpeed = 200f;
    public float accelerationCurve = 1f;
    public float powerDistribution = 1f;
    public float engineBraking = 0.3f;
    public float maxSlipRatio = 0.3f;
    public float tractionControlStiffness = 1f;

    [Header("Audio Settings")]
    [SerializeField] private float rpmThreshold = 30f;

    [Header("Track Detection Settings")]
    [SerializeField] private LayerMask trackLayer;
    [SerializeField] private float maxDistanceFromTrack = 5f;
    [SerializeField] private float trackCheckInterval = 0.1f;

    [Header("Stability Settings")]
    [SerializeField] private float antiRollForce = 5000f;

    [Header("UI References")]
    [SerializeField] private TMP_Text resetText;  

    private float lastTrackCheck;
    private float lastResetTime;
    private bool isGrounded;
    private float currentSpeed;
    private Rigidbody rb;
    private AudioSource engineSound;
    private bool isEngineMuted = false;
    private bool isHighRPM = false;
    private bool isPlayer1;
    private bool isOnTrack = false;

    private RaceManager raceManager;
    private CheckpointManager checkpointManager;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError($"{gameObject.name}: No Rigidbody found!");
            return;
        }

        // Set the center of mass lower and slightly forward for better stability
        rb.centerOfMass = new Vector3(0, -0.4f, 0.2f);
        
        // Increase angular drag to resist flipping
        rb.angularDamping = 5f;
        
        // Find the RaceManager
        raceManager = Object.FindAnyObjectByType<RaceManager>();
        if (raceManager == null)
        {
            Debug.LogWarning("RaceManager not found in scene");
        }

        // Find the CheckpointManager
        checkpointManager = Object.FindAnyObjectByType<CheckpointManager>();
        if (checkpointManager == null)
        {
            Debug.LogWarning("CheckpointManager not found in scene");
        }

        // Get the audio source
        engineSound = GetComponent<AudioSource>();
        if (engineSound == null)
        {
            Debug.LogWarning("No AudioSource found on car!");
        }

        // Determine if this is player 1 or 2
        isPlayer1 = gameObject.name.Contains("Player1");

        // Set the reset text message based on player number
        if (resetText != null)
        {
            resetText.text = isPlayer1 ? "Press R to Reset" : "Press P to Reset";
            resetText.gameObject.SetActive(false);  
        }

        // Configure wheel friction
        ConfigureWheelFriction(frontLeftWheelCollider);
        ConfigureWheelFriction(frontRightWheelCollider);
        ConfigureWheelFriction(rearLeftWheelCollider);
        ConfigureWheelFriction(rearRightWheelCollider);
    }

    private void ConfigureWheelFriction(WheelCollider wheel)
    {
        WheelFrictionCurve fwdFriction = wheel.forwardFriction;
        fwdFriction.extremumSlip = 0.3f;     
        fwdFriction.extremumValue = 2f;     
        fwdFriction.asymptoteSlip = 0.8f;    
        fwdFriction.asymptoteValue = 1.6f;    
        fwdFriction.stiffness = 1.2f;        
        wheel.forwardFriction = fwdFriction;

        WheelFrictionCurve sideFriction = wheel.sidewaysFriction;
        sideFriction.extremumSlip = 0.25f;    
        sideFriction.extremumValue = 2.2f;    
        sideFriction.asymptoteSlip = 0.5f;    
        sideFriction.asymptoteValue = 2f;    
        sideFriction.stiffness = 1.4f;       
        wheel.sidewaysFriction = sideFriction;
    }

    private void FixedUpdate()
    {
        if (rb == null) return;

        // Don't process physics if race hasn't started
        if (!raceManager.HasRaceStarted)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            return;
        }

        // Apply downforce when grounded
        bool isGrounded = frontLeftWheelCollider.isGrounded || frontRightWheelCollider.isGrounded || 
                         rearLeftWheelCollider.isGrounded || rearRightWheelCollider.isGrounded;

        if (isGrounded)
        {
            // Apply downforce based on speed
            float speedFactor = rb.linearVelocity.magnitude / 10f;  
            Vector3 downforceVector = -transform.up * (100f * speedFactor);
            rb.AddForce(downforceVector);
        }
        else
        {
            // Apply extra gravity when in air
            rb.AddForce(Physics.gravity * (2f - 1));
            
            // Apply air resistance to prevent floating
            Vector3 airResistanceForce = -rb.linearVelocity * 0.1f;
            rb.AddForce(airResistanceForce);
        }

        currentSpeed = rb.linearVelocity.magnitude;
        HandleInputs();
        UpdateWheels();
        ApplyAntiRoll();
        StabilizeCar();

        // Apply anti-roll force
        Vector3 predictedUp = Quaternion.AngleAxis(
            rb.angularVelocity.magnitude * Mathf.Rad2Deg * 0.01f,
            rb.angularVelocity
        ) * transform.up;
        
        Vector3 torqueVector = Vector3.Cross(predictedUp, Vector3.up);
        rb.AddTorque(torqueVector * antiRollForce * rb.linearVelocity.magnitude);

        // Apply extra downforce at high speeds
        float downforce = rb.linearVelocity.sqrMagnitude * 0.1f;
        rb.AddForce(-transform.up * downforce);

        // Apply stability force to prevent excessive tilting
        if (Vector3.Dot(transform.up, Vector3.up) < 0.95f)
        {
            Vector3 stabilizationTorque = Vector3.Cross(transform.up, Vector3.up) * 2000f;
            rb.AddTorque(stabilizationTorque);
        }
    }

    private void HandleInputs()
    {
        float steering, acceleration;
        bool isReversing;

        if (isPlayer1)
        {
            // Player 1 - WASD controls
            steering = Input.GetKey(KeyCode.D) ? 1 : Input.GetKey(KeyCode.A) ? -1 : 0;
            acceleration = Input.GetKey(KeyCode.W) ? 1 : Input.GetKey(KeyCode.S) ? -1 : 0;
            isReversing = Input.GetKey(KeyCode.S);
        }
        else
        {
            // Player 2 - Arrow keys
            steering = Input.GetKey(KeyCode.RightArrow) ? 1 : Input.GetKey(KeyCode.LeftArrow) ? -1 : 0;
            acceleration = Input.GetKey(KeyCode.UpArrow) ? 1 : Input.GetKey(KeyCode.DownArrow) ? -1 : 0;
            isReversing = Input.GetKey(KeyCode.DownArrow);
        }

        // Apply steering
        float steeringAngle = steering * maxSteerAngle;
        frontLeftWheelCollider.steerAngle = steeringAngle;
        frontRightWheelCollider.steerAngle = steeringAngle;

        // Calculate motor force based on input and direction
        float currentMotorForce = isReversing ? -motorForce * 0.7f : motorForce;  // 70% power in reverse

        // Apply motor force to all wheels
        if (Mathf.Abs(acceleration) > 0.1f)
        {
            frontLeftWheelCollider.motorTorque = acceleration * currentMotorForce;
            frontRightWheelCollider.motorTorque = acceleration * currentMotorForce;
            rearLeftWheelCollider.motorTorque = acceleration * currentMotorForce;
            rearRightWheelCollider.motorTorque = acceleration * currentMotorForce;
            
            // Clear brake torque when accelerating
            frontLeftWheelCollider.brakeTorque = 0;
            frontRightWheelCollider.brakeTorque = 0;
            rearLeftWheelCollider.brakeTorque = 0;
            rearRightWheelCollider.brakeTorque = 0;
        }
        else
        {
            // Apply brakes when no acceleration input
            frontLeftWheelCollider.motorTorque = 0;
            frontRightWheelCollider.motorTorque = 0;
            rearLeftWheelCollider.motorTorque = 0;
            rearRightWheelCollider.motorTorque = 0;
            
            frontLeftWheelCollider.brakeTorque = brakeForce;
            frontRightWheelCollider.brakeTorque = brakeForce;
            rearLeftWheelCollider.brakeTorque = brakeForce;
            rearRightWheelCollider.brakeTorque = brakeForce;
        }
    }

    private void Update()
    {
        if (rb == null) return;

        // Check for manual reset (R key for Player 1, P key for Player 2)
        if ((isPlayer1 && Input.GetKeyDown(KeyCode.R)) || (!isPlayer1 && Input.GetKeyDown(KeyCode.P)))
        {
            MoveToLastCheckpoint();
        }

        // Auto-reset if car is too tilted
        float currentTilt = Vector3.Angle(transform.up, Vector3.up);
        if (currentTilt > 60f)
        {
            MoveToLastCheckpoint();
        }

        // Handle engine sound
        if (engineSound != null && !isEngineMuted)
        {
            float currentSpeed = rb.linearVelocity.magnitude;
            float acceleration = isPlayer1 ? 
                (Input.GetKey(KeyCode.W) ? 1 : 0) - (Input.GetKey(KeyCode.S) ? 1 : 0) : 
                (Input.GetKey(KeyCode.UpArrow) ? 1 : 0) - (Input.GetKey(KeyCode.DownArrow) ? 1 : 0);
            
            bool shouldBeHighRPM = currentSpeed > rpmThreshold || (acceleration > 0.5f && currentSpeed > rpmThreshold * 0.5f);
            
            if (shouldBeHighRPM != isHighRPM)
            {
                isHighRPM = shouldBeHighRPM;
                engineSound.pitch = isHighRPM ? 1.5f : 0.5f;
            }
        }

        // Check if we're on track
        if (Time.time - lastTrackCheck >= trackCheckInterval)
        {
            CheckIfOnTrack();
            lastTrackCheck = Time.time;
        }
    }

    private void UpdateWheels()
    {
        UpdateWheelPos(frontLeftWheelCollider, frontLeftWheelTransform);
        UpdateWheelPos(frontRightWheelCollider, frontRightWheelTransform);
        UpdateWheelPos(rearLeftWheelCollider, rearLeftWheelTransform);
        UpdateWheelPos(rearRightWheelCollider, rearRightWheelTransform);
    }

    private void UpdateWheelPos(WheelCollider wheelCollider, Transform wheelTransform)
    {
        Vector3 pos;
        Quaternion rot;
        wheelCollider.GetWorldPose(out pos, out rot);
        wheelTransform.position = pos;
        wheelTransform.rotation = rot;
    }

    private void StabilizeCar()
    {
        if (!rb) return;

        // Cast rays to detect nearby walls
        float rayLength = 1.5f;
        RaycastHit hitInfo;
        
        // Cast rays to the sides
        bool hitLeft = Physics.Raycast(transform.position, -transform.right, out hitInfo, rayLength);
        bool hitRight = Physics.Raycast(transform.position, transform.right, out hitInfo, rayLength);

        // If we're close to a wall, apply a stabilizing force
        if (hitLeft || hitRight)
        {
            Vector3 stabilizeDir = Vector3.up;
            rb.AddForce(stabilizeDir * 5000f);
            
            // Also try to keep the car upright
            Quaternion targetRotation = Quaternion.FromToRotation(transform.up, Vector3.up) * transform.rotation;
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.fixedDeltaTime * 5f);
        }
    }

    private void ApplyAntiRoll()
    {
        if (!rb) return;

        float travelL, travelR;
        bool groundedL, groundedR;

        // Front anti-roll
        GetWheelCompression(frontLeftWheelCollider, out travelL, out groundedL);
        GetWheelCompression(frontRightWheelCollider, out travelR, out groundedR);
        float antiRollForceFront = (travelL - travelR) * antiRollForce;

        if (groundedL)
            rb.AddForceAtPosition(transform.up * antiRollForceFront, transform.position + transform.right * 0.5f);
        if (groundedR)
            rb.AddForceAtPosition(transform.up * -antiRollForceFront, transform.position - transform.right * 0.5f);

        // Rear anti-roll (slightly less force)
        GetWheelCompression(rearLeftWheelCollider, out travelL, out groundedL);
        GetWheelCompression(rearRightWheelCollider, out travelR, out groundedR);
        float antiRollForceRear = (travelL - travelR) * (antiRollForce * 0.7f);

        if (groundedL)
            rb.AddForceAtPosition(transform.up * antiRollForceRear, transform.position + transform.right * 0.5f);
        if (groundedR)
            rb.AddForceAtPosition(transform.up * -antiRollForceRear, transform.position - transform.right * 0.5f);
    }

    private void GetWheelCompression(WheelCollider wheel, out float travel, out bool grounded)
    {
        WheelHit hit;
        grounded = wheel.GetGroundHit(out hit);
        
        if (grounded)
        {
            travel = (-hit.point.y - wheel.radius) / wheel.suspensionDistance;
        }
        else
        {
            travel = 1.0f;
        }
    }

    private void CheckIfOnTrack()
    {
        if (!rb) return;

        bool wasOnTrack = isOnTrack;
        isOnTrack = false;
        int rayHits = 0;
        float totalDistance = 0f;

        // Cast rays to detect track
        float rayLength = maxDistanceFromTrack;
        Vector3[] rayDirections = new Vector3[] 
        { 
            Vector3.down,
            Vector3.down + transform.forward * 0.5f,
            Vector3.down - transform.forward * 0.5f,
            Vector3.down + transform.right * 0.5f,
            Vector3.down - transform.right * 0.5f
        };

        foreach (Vector3 direction in rayDirections)
        {
            Vector3 rayStart = transform.position + Vector3.up * 0.5f;
            RaycastHit hit;

            if (Physics.Raycast(rayStart, direction.normalized, out hit, rayLength, trackLayer))
            {
                rayHits++;
                totalDistance += hit.distance;
                isOnTrack = true;
            }
        }

        // Only log when car goes off track
        if (wasOnTrack && !isOnTrack)
        {
            Debug.Log($"{gameObject.name} has gone off track!");
        }

        // Move car to last checkpoint if it goes off track or is tilted too much
        if (!isOnTrack && rb.linearVelocity.magnitude > 1f)
        {
            MoveToLastCheckpoint();
        }
        else if (Vector3.Dot(transform.up, Vector3.up) < 0.5f)
        {
            MoveToLastCheckpoint();
        }
    }

    private void MoveToLastCheckpoint()
    {
        // Get checkpoint position and rotation
        Vector3 checkpointPos = checkpointManager.GetLastCheckpointPosition(gameObject.name);
        Quaternion checkpointRot = checkpointManager.GetLastCheckpointRotation(gameObject.name);
        
        Debug.Log($"{gameObject.name} moving to checkpoint position: {checkpointPos}");

        // Start smooth movement coroutine
        StartCoroutine(SmoothMoveToCheckpoint(checkpointPos, checkpointRot));
    }

    private IEnumerator SmoothMoveToCheckpoint(Vector3 targetPos, Quaternion targetRot)
    {
        // Initial setup - much faster transition
        float duration = 0.3f;
        float elapsedTime = 0f;
        
        // Store initial transform
        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;
        
        // Lower, faster arc
        Vector3 midPoint = Vector3.Lerp(startPos, targetPos, 0.5f) + Vector3.up * 1f;
        
        // Reset physics state
        rb.isKinematic = true;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        // Smooth movement
        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;
            
            // Faster easing curve
            float smoothT = t * t;
            
            // Calculate position with lower arc
            Vector3 a = Vector3.Lerp(startPos, midPoint, smoothT);
            Vector3 b = Vector3.Lerp(midPoint, targetPos + Vector3.up * 0.5f, smoothT);
            transform.position = Vector3.Lerp(a, b, smoothT);
            
            // Smooth rotation
            transform.rotation = Quaternion.Slerp(startRot, targetRot, smoothT);
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Ensure we end up exactly at the target
        transform.position = targetPos + Vector3.up * 0.5f;
        transform.rotation = targetRot;
        
        // Re-enable physics
        rb.isKinematic = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Only apply anti-flip force on collision
        if (Vector3.Dot(transform.up, Vector3.up) < 0.5f)
        {
            rb.AddTorque(Vector3.Cross(transform.up, Vector3.up) * antiRollForce * 2f);
        }
    }

    public void MuteEngine(bool mute)
    {
        isEngineMuted = mute;
        if (engineSound != null)
        {
            engineSound.mute = mute;
        }
    }

    public void StopEngine()
    {
        if (engineSound != null)
        {
            engineSound.Stop();
            engineSound.enabled = false;
        }
    }
}

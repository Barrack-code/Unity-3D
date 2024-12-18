using UnityEngine;
using System.Collections;
using UnityEngine.UI;  
using TMPro;  
using UnityEngine.SceneManagement;

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
    public float motorForce = 3000f;
    public float brakeForce = 3000f;
    public float maxSteerAngle = 30f;
    public float maxSpeed = 200f;
    public float accelerationCurve = 1f;
    public float powerDistribution = 1f;
    public float engineBraking = 0.3f;
    public float maxSlipRatio = 0.3f;
    public float tractionControlStiffness = 1f;
    public float downforceMultiplier = 100f;  // For aerodynamic downforce
    public float lateralStabilizationForce = 2000f;  // For reducing side slip
    public float wheelGripMultiplier = 1f;  // For wheel grip control
    public float tractionControl = 0.8f;  // For traction control (0-1)

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
    private bool isRespawning = false;

    private Vector3[] initialWheelPositions = new Vector3[4];
    private Quaternion[] initialWheelRotations = new Quaternion[4];

    private RaceManager raceManager;
    private CheckpointManager checkpointManager;

    private void Start()
    {
        // Check if we're in display mode (car selection scene or display parent)
        bool isDisplayMode = SceneManager.GetActiveScene().name.Contains("CarSelect") || 
                           transform.parent?.name.Contains("Display") == true ||
                           gameObject.name.Contains("Display");

        // Always get or add Rigidbody
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }

        // Configure basic rigidbody settings
        rb.mass = 1500f;
        rb.linearDamping = 0.05f;
        rb.angularDamping = 0.05f;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.centerOfMass = new Vector3(0, -0.5f, 0.1f);
        rb.isKinematic = false; // Make sure it's not kinematic by default

        if (isDisplayMode)
        {
            rb.isKinematic = true; // Only kinematic in display mode
            rb.useGravity = false;
            DisableWheelColliders();
            return;
        }

        // Get the checkpoint manager reference
        checkpointManager = UnityEngine.Object.FindAnyObjectByType<CheckpointManager>();

        // Get the race manager reference
        raceManager = UnityEngine.Object.FindAnyObjectByType<RaceManager>();

        // Configure wheel friction curves for better grip
        ConfigureWheelFriction(frontLeftWheelCollider);
        ConfigureWheelFriction(frontRightWheelCollider);
        ConfigureWheelFriction(rearLeftWheelCollider);
        ConfigureWheelFriction(rearRightWheelCollider);

        // Store initial wheel positions
        if (frontLeftWheelTransform != null) {
            initialWheelPositions[0] = frontLeftWheelTransform.localPosition;
            initialWheelRotations[0] = frontLeftWheelTransform.localRotation;
        }
        if (frontRightWheelTransform != null) {
            initialWheelPositions[1] = frontRightWheelTransform.localPosition;
            initialWheelRotations[1] = frontRightWheelTransform.localRotation;
        }
        if (rearLeftWheelTransform != null) {
            initialWheelPositions[2] = rearLeftWheelTransform.localPosition;
            initialWheelRotations[2] = rearLeftWheelTransform.localRotation;
        }
        if (rearRightWheelTransform != null) {
            initialWheelPositions[3] = rearRightWheelTransform.localPosition;
            initialWheelRotations[3] = rearRightWheelTransform.localRotation;
        }

        // Configure wheel colliders
        ConfigureWheelCollider(frontLeftWheelCollider, "Front Left");
        ConfigureWheelCollider(frontRightWheelCollider, "Front Right");
        ConfigureWheelCollider(rearLeftWheelCollider, "Rear Left");
        ConfigureWheelCollider(rearRightWheelCollider, "Rear Right");

        // Get the audio source
        engineSound = GetComponent<AudioSource>();

        // Set player number based on name
        isPlayer1 = gameObject.name.Contains("Player1");
    }

    private IEnumerator MaintainPosition(Vector3 position)
    {
        while (true)
        {
            if (transform.position != position)
            {
                transform.position = position;
            }
            yield return new WaitForFixedUpdate();
        }
    }

    private void DisableWheelColliders()
    {
        if (frontLeftWheelCollider != null) frontLeftWheelCollider.enabled = false;
        if (frontRightWheelCollider != null) frontRightWheelCollider.enabled = false;
        if (rearLeftWheelCollider != null) rearLeftWheelCollider.enabled = false;
        if (rearRightWheelCollider != null) rearRightWheelCollider.enabled = false;

        // Also disable the audio if present
        if (engineSound != null)
        {
            engineSound.enabled = false;
        }
    }

    private void ConfigureWheelCollider(WheelCollider wheel, string wheelName)
    {
        if (wheel == null)
        {
            Debug.LogError($"[Backup] {wheelName} wheel collider is null!");
            return;
        }

        // Configure wheel settings
        wheel.radius = 0.4f;
        wheel.wheelDampingRate = 0.25f;
        wheel.suspensionDistance = 0.15f;
        wheel.forceAppPointDistance = 0;
        wheel.mass = 20f;
        
        JointSpring suspensionSpring = wheel.suspensionSpring;
        suspensionSpring.spring = 35000f;
        suspensionSpring.damper = 4500f;
        wheel.suspensionSpring = suspensionSpring;

        WheelFrictionCurve fwdFriction = wheel.forwardFriction;
        fwdFriction.extremumSlip = 0.4f;
        fwdFriction.extremumValue = 2.0f;
        fwdFriction.asymptoteSlip = 0.8f;
        fwdFriction.asymptoteValue = 1.8f;
        fwdFriction.stiffness = 1.0f;
        wheel.forwardFriction = fwdFriction;

        WheelFrictionCurve sideFriction = wheel.sidewaysFriction;
        sideFriction.extremumSlip = 0.25f;
        sideFriction.extremumValue = 2.0f;
        sideFriction.asymptoteSlip = 0.5f;
        sideFriction.asymptoteValue = 1.8f;
        sideFriction.stiffness = 1.0f;
        wheel.sidewaysFriction = sideFriction;

        wheel.enabled = true;
        wheel.motorTorque = 0;
        wheel.brakeTorque = 0;
    }

    private void ConfigureWheelFriction(WheelCollider wheel)
    {
        WheelFrictionCurve forwardFriction = wheel.forwardFriction;
        WheelFrictionCurve sidewaysFriction = wheel.sidewaysFriction;

        // Adjust forward friction for better acceleration and braking
        forwardFriction.extremumSlip = 0.4f;
        forwardFriction.extremumValue = 1.5f * wheelGripMultiplier;  // Reduced from 2.0f
        forwardFriction.asymptoteSlip = 0.8f;
        forwardFriction.asymptoteValue = 1.4f * wheelGripMultiplier;  // Reduced from 1.8f
        forwardFriction.stiffness = tractionControlStiffness * 0.9f;  // Reduced stiffness

        // Adjust sideways friction for better cornering
        sidewaysFriction.extremumSlip = 0.25f;
        sidewaysFriction.extremumValue = 1.8f * wheelGripMultiplier;
        sidewaysFriction.asymptoteSlip = 0.5f;
        sidewaysFriction.asymptoteValue = 1.6f * wheelGripMultiplier;
        sidewaysFriction.stiffness = tractionControlStiffness;

        wheel.forwardFriction = forwardFriction;
        wheel.sidewaysFriction = sidewaysFriction;
    }

    private bool IsRaceScene()
    {
        string currentScene = SceneManager.GetActiveScene().name.ToUpper();
        return currentScene == "CIRCUIT" || currentScene == "TUNNEL";
    }

    private void FixedUpdate()
    {
        // Skip if rigidbody is missing
        if (!rb)
        {
            Debug.LogError("[Backup] Rigidbody is null in FixedUpdate!");
            return;
        }

        HandleInputs();
        UpdateWheels();
        ApplyAntiRoll();
        ApplyTractionControl();
    }

    private void HandleInputs()
    {
        float steering = 0f;
        float acceleration = 0f;
        bool isReversing = false;

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
        frontLeftWheelCollider.steerAngle = steering * maxSteerAngle;
        frontRightWheelCollider.steerAngle = steering * maxSteerAngle;

        // Calculate motor force based on input and direction
        float currentMotorForce = isReversing ? motorForce * 0.5f : motorForce;

        if (Mathf.Abs(acceleration) > 0.1f)
        {
            // Clear brake torque
            frontLeftWheelCollider.brakeTorque = 0;
            frontRightWheelCollider.brakeTorque = 0;
            rearLeftWheelCollider.brakeTorque = 0;
            rearRightWheelCollider.brakeTorque = 0;

            // Apply motor force
            float rearWheelForce = currentMotorForce * acceleration;
            float frontWheelForce = currentMotorForce * acceleration * 0.3f;

            rearLeftWheelCollider.motorTorque = rearWheelForce;
            rearRightWheelCollider.motorTorque = rearWheelForce;
            frontLeftWheelCollider.motorTorque = frontWheelForce;
            frontRightWheelCollider.motorTorque = frontWheelForce;
        }
        else
        {
            // Clear motor torque
            frontLeftWheelCollider.motorTorque = 0;
            frontRightWheelCollider.motorTorque = 0;
            rearLeftWheelCollider.motorTorque = 0;
            rearRightWheelCollider.motorTorque = 0;
            
            // Apply brakes
            float frontBrakeForce = brakeForce;
            float rearBrakeForce = brakeForce * 0.6f;
            
            frontLeftWheelCollider.brakeTorque = frontBrakeForce;
            frontRightWheelCollider.brakeTorque = frontBrakeForce;
            rearLeftWheelCollider.brakeTorque = rearBrakeForce;
            rearRightWheelCollider.brakeTorque = rearBrakeForce;
        }
    }

    private void ProcessPhysics()
    {
        // Apply downforce based on speed
        float speed = rb.linearVelocity.magnitude;
        Vector3 downforce = -transform.up * (speed * speed * downforceMultiplier);
        rb.AddForce(downforce);

        // Apply lateral stabilization
        Vector3 lateralVelocity = Vector3.Project(rb.linearVelocity, transform.right);
        rb.AddForce(-lateralVelocity * lateralStabilizationForce);

        // Process physics for the car
        if (isGrounded)
        {
            // Apply extra gravity when grounded for better stability
            rb.AddForce(Physics.gravity * (2f - 1));
            
            // Apply air resistance to prevent floating
            Vector3 airResistanceForce = -rb.linearVelocity * 0.1f;
            rb.AddForce(airResistanceForce);
        }

        currentSpeed = rb.linearVelocity.magnitude;
    }

    private bool IsCarGrounded()
    {
        WheelHit hit;
        int groundedWheels = 0;
        WheelCollider[] wheels = { 
            frontLeftWheelCollider, 
            frontRightWheelCollider, 
            rearLeftWheelCollider, 
            rearRightWheelCollider 
        };

        foreach (var wheel in wheels)
        {
            if (wheel != null && wheel.GetGroundHit(out hit))
            {
                groundedWheels++;
            }
        }

        // Car is considered grounded if at least 3 wheels are touching
        return groundedWheels >= 3;
    }

    private void UpdateWheelTransforms()
    {
        UpdateWheelTransform(frontLeftWheelCollider, frontLeftWheelTransform);
        UpdateWheelTransform(frontRightWheelCollider, frontRightWheelTransform);
        UpdateWheelTransform(rearLeftWheelCollider, rearLeftWheelTransform);
        UpdateWheelTransform(rearRightWheelCollider, rearRightWheelTransform);
    }

    private void UpdateWheelTransform(WheelCollider wheelCollider, Transform wheelTransform)
    {
        if (wheelCollider == null || wheelTransform == null) return;

        Vector3 pos;
        Quaternion rot;
        wheelCollider.GetWorldPose(out pos, out rot);
        wheelTransform.position = pos;
        wheelTransform.rotation = rot;
    }

    private void UpdateWheelFrictions(float speedFactor)
    {
        // Dynamically adjust wheel friction based on speed
        float currentGrip = wheelGripMultiplier * (1f + speedFactor * 0.5f);
        
        WheelCollider[] wheels = { 
            frontLeftWheelCollider, 
            frontRightWheelCollider, 
            rearLeftWheelCollider, 
            rearRightWheelCollider 
        };

        foreach (var wheel in wheels)
        {
            if (wheel == null) continue;

            WheelFrictionCurve fwdFriction = wheel.forwardFriction;
            WheelFrictionCurve sideFriction = wheel.sidewaysFriction;

            // Increase friction with speed
            fwdFriction.stiffness = tractionControlStiffness * (1f + speedFactor);
            sideFriction.stiffness = tractionControlStiffness * (1f + speedFactor * 1.2f);

            wheel.forwardFriction = fwdFriction;
            wheel.sidewaysFriction = sideFriction;
        }
    }

    private void ApplyTractionControl()
    {
        WheelHit hit;
        float slipAllowance = (1f - tractionControl) * maxSlipRatio;

        // Check and adjust rear wheels
        if (rearLeftWheelCollider.GetGroundHit(out hit))
        {
            AdjustWheelForce(rearLeftWheelCollider, hit.forwardSlip, slipAllowance);
        }
        if (rearRightWheelCollider.GetGroundHit(out hit))
        {
            AdjustWheelForce(rearRightWheelCollider, hit.forwardSlip, slipAllowance);
        }
    }

    private void AdjustWheelForce(WheelCollider wheel, float currentSlip, float maxSlip)
    {
        if (Mathf.Abs(currentSlip) > maxSlip)
        {
            float reduction = 1.0f - ((Mathf.Abs(currentSlip) - maxSlip) / maxSlip);
            reduction = Mathf.Clamp(reduction, 0.1f, 1.0f);
            wheel.motorTorque *= reduction;
        }
    }

    private void Update()
    {
        if (rb == null) return;

        // Handle respawn input with force
        if ((isPlayer1 && Input.GetKeyDown(KeyCode.R)) || (!isPlayer1 && Input.GetKeyDown(KeyCode.P)))
        {
            Debug.Log($"[{gameObject.name}] Respawn requested by player input");
            if (checkpointManager == null)
            {
                checkpointManager = UnityEngine.Object.FindAnyObjectByType<CheckpointManager>();
                if (checkpointManager == null)
                {
                    Debug.LogError($"[{gameObject.name}] No checkpoint manager found!");
                    return;
                }
            }
            StartCoroutine(RespawnSequence());
            return;
        }

        // Auto-reset if car is too tilted
        float currentTilt = Vector3.Angle(transform.up, Vector3.up);
        if (currentTilt > 60f && !isRespawning)
        {
            Debug.Log($"[{gameObject.name}] Auto-respawn triggered due to tilt: {currentTilt}");
            StartCoroutine(RespawnSequence());
            return;
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
        UpdateWheelPos(frontLeftWheelCollider, frontLeftWheelTransform, 0, "Front Left");
        UpdateWheelPos(frontRightWheelCollider, frontRightWheelTransform, 1, "Front Right");
        UpdateWheelPos(rearLeftWheelCollider, rearLeftWheelTransform, 2, "Rear Left");
        UpdateWheelPos(rearRightWheelCollider, rearRightWheelTransform, 3, "Rear Right");
    }

    private void UpdateWheelPos(WheelCollider wheelCollider, Transform wheelTransform, int wheelIndex, string wheelName)
    {
        if (wheelCollider == null || wheelTransform == null)
        {
            Debug.LogError($"[Backup] {wheelName} wheel components missing!");
            return;
        }

        Vector3 pos;
        Quaternion rot;
        wheelCollider.GetWorldPose(out pos, out rot);

        // Convert world position to local space relative to car
        Vector3 localPos = transform.InverseTransformPoint(pos);

        // Keep original X and Z positions, only update Y for suspension
        Vector3 newLocalPos = new Vector3(
            initialWheelPositions[wheelIndex].x,
            localPos.y,
            initialWheelPositions[wheelIndex].z
        );

        // Apply the position in local space
        wheelTransform.localPosition = newLocalPos;

        // Calculate rotation relative to car
        Quaternion localRot = Quaternion.Inverse(transform.rotation) * rot;
        
        // Combine with initial rotation
        Quaternion finalRotation = localRot * initialWheelRotations[wheelIndex];
        wheelTransform.localRotation = finalRotation;
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
        if (Time.time - lastTrackCheck < trackCheckInterval) return;
        lastTrackCheck = Time.time;

        bool wasOnTrack = isOnTrack;
        isOnTrack = false;

        // Cast multiple rays in a cone shape to detect track
        int numRays = 8;
        float rayLength = maxDistanceFromTrack;
        float coneAngle = 45f;
        int rayHits = 0;
        float totalDistance = 0f;

        for (int i = 0; i < numRays; i++)
        {
            float angle = (i / (float)(numRays - 1) - 0.5f) * coneAngle;
            Vector3 direction = Quaternion.Euler(0f, angle, 0f) * -Vector3.up;

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
        if ((!isOnTrack && rb != null && rb.linearVelocity.magnitude > 1f) || 
            (transform.up != null && Vector3.Dot(transform.up, Vector3.up) < 0.5f))
        {
            StartCoroutine(RespawnSequence());
        }
    }

    private IEnumerator RespawnSequence()
    {
        if (isRespawning)
        {
            Debug.Log($"[{gameObject.name}] Already respawning, skipping new request");
            yield break;
        }

        isRespawning = true;
        Debug.Log($"[{gameObject.name}] Starting respawn sequence");

        // Get checkpoint data
        Vector3 checkpointPos = checkpointManager.GetLastCheckpointPosition(gameObject.name);
        Quaternion checkpointRot = checkpointManager.GetLastCheckpointRotation(gameObject.name);
        Debug.Log($"[{gameObject.name}] Respawn position: {checkpointPos}, rotation: {checkpointRot.eulerAngles}");

        // Completely freeze the car
        rb.isKinematic = true;
        rb.detectCollisions = false;  // Temporarily disable collisions
        
        // Reset all wheel states immediately
        WheelCollider[] wheels = { frontLeftWheelCollider, frontRightWheelCollider, rearLeftWheelCollider, rearRightWheelCollider };
        foreach (var wheel in wheels)
        {
            if (wheel != null)
            {
                wheel.motorTorque = 0;
                wheel.brakeTorque = brakeForce * 2f;  // Double brake force
                wheel.steerAngle = 0;
            }
        }

        Debug.Log($"[{gameObject.name}] Disabled physics and reset wheels");

        // Force position update multiple times to ensure it takes
        for (int i = 0; i < 3; i++)
        {
            transform.position = checkpointPos + (Vector3.up * 0.5f);
            transform.rotation = checkpointRot;
            yield return new WaitForFixedUpdate();
        }

        Debug.Log($"[{gameObject.name}] Forced position update");

        // Re-enable physics with controlled state
        rb.detectCollisions = true;
        rb.isKinematic = false;
        
        // Force velocity reset multiple times
        for (int i = 0; i < 3; i++)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            yield return new WaitForFixedUpdate();
        }

        Debug.Log($"[{gameObject.name}] Re-enabled physics with zero velocity");

        // Apply strong downward force
        rb.AddForce(Vector3.down * rb.mass * Physics.gravity.magnitude * 5f, ForceMode.Impulse);
        Debug.Log($"[{gameObject.name}] Applied downward force");

        // Reset car state
        currentSpeed = 0f;
        isOnTrack = true;

        // Gradually release brakes
        float brakeReleaseTime = 0.5f;
        float elapsedTime = 0f;
        float initialBrakeForce = brakeForce * 2f;

        while (elapsedTime < brakeReleaseTime)
        {
            float brakeFactor = 1f - (elapsedTime / brakeReleaseTime);
            float currentBrakeForce = initialBrakeForce * brakeFactor;

            foreach (var wheel in wheels)
            {
                if (wheel != null)
                {
                    wheel.brakeTorque = currentBrakeForce;
                }
            }

            elapsedTime += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        // Final brake release
        foreach (var wheel in wheels)
        {
            if (wheel != null)
            {
                wheel.brakeTorque = 0;
            }
        }

        Debug.Log($"[{gameObject.name}] Respawn sequence complete");
        isRespawning = false;
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

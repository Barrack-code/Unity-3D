using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    private Backup carController;
    private bool isPlayer1;
    private Rigidbody rb;

    void Start()
    {
        carController = GetComponent<Backup>();
        rb = GetComponent<Rigidbody>();
        isPlayer1 = CompareTag("Player1");
        Debug.Log($"PlayerInput initialized for {(isPlayer1 ? "Player 1" : "Player 2")} on GameObject: {gameObject.name} with tag: {gameObject.tag}");
    }

    void Update()
    {
        if (carController == null || rb == null) return;

        float steering = 0f;
        float acceleration = 0f;
        bool isReversing = false;

        if (isPlayer1)
        {
            // WASD controls for Player 1
            steering = Input.GetKey(KeyCode.D) ? 1 : Input.GetKey(KeyCode.A) ? -1 : 0;
            acceleration = Input.GetKey(KeyCode.W) ? 1 : Input.GetKey(KeyCode.S) ? -1 : 0;
            isReversing = Input.GetKey(KeyCode.S);
            Debug.Log($"P1 Input - Steering: {steering}, Acceleration: {acceleration}");
        }
        else
        {
            // Arrow keys for Player 2
            steering = Input.GetKey(KeyCode.RightArrow) ? 1 : Input.GetKey(KeyCode.LeftArrow) ? -1 : 0;
            acceleration = Input.GetKey(KeyCode.UpArrow) ? 1 : Input.GetKey(KeyCode.DownArrow) ? -1 : 0;
            isReversing = Input.GetKey(KeyCode.DownArrow);
            Debug.Log($"P2 Input - Steering: {steering}, Acceleration: {acceleration}");
        }

        // Reduce maximum steer angle at higher speeds for better stability
        float speedFactor = rb.linearVelocity.magnitude / 30f; // 30 is max speed reference
        float currentMaxSteerAngle = Mathf.Lerp(carController.maxSteerAngle, carController.maxSteerAngle * 0.5f, speedFactor);
        float steerAngle = currentMaxSteerAngle * steering;

        // Apply steering
        carController.frontLeftWheelCollider.steerAngle = steerAngle;
        carController.frontRightWheelCollider.steerAngle = steerAngle;

        // Calculate motor force based on input and direction
        float currentMotorForce = isReversing ? carController.motorForce * 0.5f : carController.motorForce;

        if (Mathf.Abs(acceleration) > 0.1f)
        {
            // Clear brake torque
            carController.frontLeftWheelCollider.brakeTorque = 0;
            carController.frontRightWheelCollider.brakeTorque = 0;
            carController.rearLeftWheelCollider.brakeTorque = 0;
            carController.rearRightWheelCollider.brakeTorque = 0;

            // Apply motor force with enhanced traction
            float rearWheelForce = currentMotorForce * acceleration * 1.5f; // 50% more power
            float frontWheelForce = currentMotorForce * acceleration * 0.3f * 1.5f;

            carController.rearLeftWheelCollider.motorTorque = rearWheelForce;
            carController.rearRightWheelCollider.motorTorque = rearWheelForce;
            carController.frontLeftWheelCollider.motorTorque = frontWheelForce;
            carController.frontRightWheelCollider.motorTorque = frontWheelForce;
        }
        else
        {
            // Clear motor torque
            carController.frontLeftWheelCollider.motorTorque = 0;
            carController.frontRightWheelCollider.motorTorque = 0;
            carController.rearLeftWheelCollider.motorTorque = 0;
            carController.rearRightWheelCollider.motorTorque = 0;

            // Apply brakes with 60/40 brake bias
            float frontBrakeForce = carController.brakeForce;
            float rearBrakeForce = carController.brakeForce * 0.6f;

            carController.frontLeftWheelCollider.brakeTorque = frontBrakeForce;
            carController.frontRightWheelCollider.brakeTorque = frontBrakeForce;
            carController.rearLeftWheelCollider.brakeTorque = rearBrakeForce;
            carController.rearRightWheelCollider.brakeTorque = rearBrakeForce;
        }
    }
}

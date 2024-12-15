using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    private CarController carController;
    private bool isPlayer1;

    void Start()
    {
        carController = GetComponent<CarController>();
        isPlayer1 = CompareTag("Player1");
    }

    void Update()
    {
        if (carController == null) return;

        float horizontal = 0f;
        float vertical = 0f;
        bool brake = false;

        if (isPlayer1)
        {
            // WASD controls for Player 1 (top screen)
            horizontal = Input.GetKey(KeyCode.D) ? 1 : Input.GetKey(KeyCode.A) ? -1 : 0;
            vertical = Input.GetKey(KeyCode.W) ? 1 : Input.GetKey(KeyCode.S) ? -1 : 0;
            brake = Input.GetKey(KeyCode.Space);
        }
        else
        {
            // Arrow keys for Player 2 (bottom screen)
            horizontal = Input.GetKey(KeyCode.RightArrow) ? 1 : Input.GetKey(KeyCode.LeftArrow) ? -1 : 0;
            vertical = Input.GetKey(KeyCode.UpArrow) ? 1 : Input.GetKey(KeyCode.DownArrow) ? -1 : 0;
            brake = Input.GetKey(KeyCode.RightControl);
        }

        // Set the values in the car controller
        carController.horizontalInput = horizontal;
        carController.verticalInput = vertical;
        carController.isBraking = brake;
    }
}

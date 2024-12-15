using UnityEngine;

public class InputController : MonoBehaviour
{
    private bool isPlayer1;
    private float lastHorizontal;
    private float lastVertical;
    private RaceManager raceManager;

    void Start()
    {
        isPlayer1 = gameObject.name == "Player1Car";
        raceManager = FindFirstObjectByType<RaceManager>();
        if (raceManager == null)
        {
            Debug.LogError("No RaceManager found in scene!");
        }
    }

    void Update()
    {
        // Don't allow input until race has started
        if (!raceManager.HasRaceStarted)
        {
            lastHorizontal = 0;
            lastVertical = 0;
            return;
        }

        // Reset the input axes to prevent other scripts from reading them directly
        Input.GetAxis("Horizontal");
        Input.GetAxis("Vertical");

        // Store our own input values
        if (isPlayer1)
        {
            // Player 1 (top) - WASD controls
            if (Input.GetKey(KeyCode.D)) lastHorizontal = 1;
            else if (Input.GetKey(KeyCode.A)) lastHorizontal = -1;
            else lastHorizontal = 0;

            if (Input.GetKey(KeyCode.W)) lastVertical = 1;
            else if (Input.GetKey(KeyCode.S)) lastVertical = -1;
            else lastVertical = 0;
        }
        else
        {
            // Player 2 (bottom) - Arrow keys
            if (Input.GetKey(KeyCode.RightArrow)) lastHorizontal = 1;
            else if (Input.GetKey(KeyCode.LeftArrow)) lastHorizontal = -1;
            else lastHorizontal = 0;

            if (Input.GetKey(KeyCode.UpArrow)) lastVertical = 1;
            else if (Input.GetKey(KeyCode.DownArrow)) lastVertical = -1;
            else lastVertical = 0;
        }
    }

    // This will be called by Unity's Input system when any script tries to read the axis
    float OnAxisGet(string axisName)
    {
        if (axisName == "Horizontal") return lastHorizontal;
        if (axisName == "Vertical") return lastVertical;
        return 0;
    }
}

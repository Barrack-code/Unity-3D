using UnityEngine;

public class CarInputManager : MonoBehaviour
{
    public static class InputAxes
    {
        public const string Horizontal = "Horizontal";
        public const string Vertical = "Vertical";
    }

    private bool isPlayer1;
    private RaceManager raceManager;

    void Start()
    {
        // Check if this is player 1's car (top screen) or player 2's car (bottom screen)
        isPlayer1 = gameObject.name == "Player1Car";
        
        // Get reference to RaceManager
        raceManager = Object.FindAnyObjectByType<RaceManager>();
    }

    public float GetAxisRaw(string axisName)
    {
        // If race hasn't started, return no input
        if (raceManager != null && !raceManager.HasRaceStarted)
            return 0f;

        // For Player 1 (top screen) - WASD controls
        if (isPlayer1)
        {
            if (axisName == InputAxes.Horizontal)
            {
                if (Input.GetKey(KeyCode.D)) return 1f;
                if (Input.GetKey(KeyCode.A)) return -1f;
                return 0f;
            }
            if (axisName == InputAxes.Vertical)
            {
                if (Input.GetKey(KeyCode.W)) return 1f;
                if (Input.GetKey(KeyCode.S)) return -1f;
                return 0f;
            }
        }
        // For Player 2 (bottom screen) - Arrow keys
        else
        {
            if (axisName == InputAxes.Horizontal)
            {
                if (Input.GetKey(KeyCode.RightArrow)) return 1f;
                if (Input.GetKey(KeyCode.LeftArrow)) return -1f;
                return 0f;
            }
            if (axisName == InputAxes.Vertical)
            {
                if (Input.GetKey(KeyCode.UpArrow)) return 1f;
                if (Input.GetKey(KeyCode.DownArrow)) return -1f;
                return 0f;
            }
        }
        return 0f;
    }

    public bool GetBrakeInput()
    {
        // If race hasn't started, return no brake input
        if (raceManager != null && !raceManager.HasRaceStarted)
            return false;
            
        return isPlayer1 ? Input.GetKey(KeyCode.Space) : Input.GetKey(KeyCode.RightControl);
    }
}

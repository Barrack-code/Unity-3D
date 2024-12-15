using UnityEngine;

public static class GameMode
{
    public enum Mode
    {
        SinglePlayer,
        Multiplayer
    }

    private static Mode currentMode = Mode.SinglePlayer;

    public static Mode CurrentMode
    {
        get
        {
            // Try to load from PlayerPrefs if not set
            if (!PlayerPrefs.HasKey("GameMode"))
            {
                PlayerPrefs.SetInt("GameMode", (int)currentMode);
            }
            return (Mode)PlayerPrefs.GetInt("GameMode", 0);
        }
        set
        {
            currentMode = value;
            PlayerPrefs.SetInt("GameMode", (int)value);
            PlayerPrefs.Save();
        }
    }

    public static bool IsSinglePlayer()
    {
        return CurrentMode == Mode.SinglePlayer;
    }

    public static bool IsMultiplayer()
    {
        return CurrentMode == Mode.Multiplayer;
    }
}

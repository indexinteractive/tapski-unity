using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Tap Ski/Game State")]
public class GameState : ScriptableObject
{
    [Tooltip("Controls whether audio is enabled")]
    public bool AudioIsEnabled;

    [Tooltip("Character selection made by the player")]
    public GameObject SelectedCharacter;

    [Tooltip("Tracks the player score during a session. Should be reset on a new game")]
    public int SessionScore;

    [Tooltip("Tracks the high score achieved on the device")]
    public int DeviceHighScore;

    /// <summary>
    /// Called by <see cref="WorldGenerator" /> when a new game begins
    /// </summary>
    public void Reset()
    {
        SessionScore = 0;
    }

    /// <summary>
    /// Called by <see cref="GameOver.OnPlayerDead" /> to set the high score
    /// </summary>
    public void SetHighScore(int sessionScore)
    {
        if (sessionScore > DeviceHighScore)
        {
            DeviceHighScore = sessionScore;
        }
    }
}

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

    [Tooltip("Username generated or chosen by the player")]
    public string Username;

    /// <summary>
    /// This value is set in <see cref="LoadManager.FlipSessionFlags"/> and read in <see cref="MenuManager.OnStartGameClick"/>.
    /// If the value has already been set to true, the player will not see the tutorial.
    /// </summary>
    [Tooltip("Controls whether the tutorial is shown")]
    public bool WillShowTutorial;

    /// <summary>
    /// Called by <see cref="WorldGenerator" /> when a new game begins
    /// </summary>
    public void Reset()
    {
        SessionScore = 0;
    }
}

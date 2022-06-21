using UnityEngine;

[CreateAssetMenu(menuName = "Tap Ski/Game State")]
public class GameState : ScriptableObject
{
    [Tooltip("Controls whether audio is enabled")]
    public bool AudioIsEnabled;

    [Tooltip("Character selection made by the player")]
    public GameObject SelectedCharacter;
}

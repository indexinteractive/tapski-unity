using UnityEngine;

[CreateAssetMenu(menuName = "Tap Ski/Game State")]
public class GameState : ScriptableObject
{
    [Tooltip("Character selection made by the player")]
    public GameObject SelectedCharacter;
}

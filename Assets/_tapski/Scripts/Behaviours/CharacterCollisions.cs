using UnityEngine;
using UnityEngine.Assertions;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(BaseCharacter))]
public class CharacterCollisions : MonoBehaviour
{
    #region Private Fields
    private BaseCharacter _character;
    #endregion

    #region Unity Lifecycle
    private void Awake()
    {
        _character = GetComponent<BaseCharacter>();
        Assert.IsNotNull(_character, $"[CharacterCollisions] No BaseCharacter found on character {name}");
    }
    #endregion

    #region Unity Events
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("obstacle"))
        {
            _character.OnCollideWithObstacle();
        }
    }
    #endregion
}

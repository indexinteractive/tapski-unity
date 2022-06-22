using UnityEngine;
using UnityEngine.Assertions;

[RequireComponent(typeof(Collider2D))]
public class Checkpoint : MonoBehaviour
{
    #region Public Properties
    public AudioSource SuccessClip;
    public GameState State;
    #endregion

    #region Unity Lifecycle
    private void Awake()
    {
        Assert.IsNotNull(SuccessClip, "[Checkpoint] SuccessClip is unassigned");
        Assert.IsNotNull(State, "[Checkpoint] Game State is unassigned");
    }
    #endregion

    #region Unity Events
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (State.AudioIsEnabled)
        {
            SuccessClip.Play();
        }

        State.SessionScore++;
    }
    #endregion
}

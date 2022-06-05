using UnityEngine;
using UnityEngine.Assertions;

[RequireComponent(typeof(Collider2D))]
public class Checkpoint : MonoBehaviour
{
    #region Public Properties
    public AudioSource SuccessClip;
    #endregion

    #region Unity Lifecycle
    private void Awake()
    {
        Assert.IsNotNull(SuccessClip, "[Checkpoint] SuccessClip is unassigned");
    }
    #endregion

    #region Unity Events
    private void OnTriggerEnter2D(Collider2D other)
    {
        SuccessClip.Play();
    }
    #endregion
}

using UnityEngine;
using UnityEngine.Assertions;

[RequireComponent(typeof(Collider2D))]
public class WompCollide : MonoBehaviour
{
    #region Public Properties
    [Header("Audio")]
    public AudioSource WompClip;
    [Header("Animation")]
    public Animator Animator;
    public string AnimationClip;
    #endregion

    #region Unity Lifecycle
    private void Awake()
    {
        Assert.IsNotNull(WompClip, "[WompCollide] WompClip is unassigned");
    }
    #endregion

    #region Unity Events
    private void OnTriggerEnter2D(Collider2D other)
    {
        WompClip.Play();

        if (!string.IsNullOrEmpty(AnimationClip))
        {
            Assert.IsNotNull(Animator, "[WompCollide] No Animator found on WompCollide.");
            Animator.Play(AnimationClip);
        }
    }
    #endregion
}

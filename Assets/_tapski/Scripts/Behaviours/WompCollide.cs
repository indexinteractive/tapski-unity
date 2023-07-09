using UnityEngine;
using UnityEngine.Assertions;

[RequireComponent(typeof(Collider2D))]
public class WompCollide : Resetable
{
    #region Public Properties
    public GameState State;

    [Header("Audio")]
    public AudioSource WompClip;

    [Header("Animation")]
    public string AnimationClip;
    #endregion

    #region Private Fields
    protected Animator _animator;
    #endregion

    #region Unity Lifecycle
    private void Awake()
    {
        Assert.IsNotNull(WompClip, "[WompCollide] WompClip is unassigned");
        Assert.IsNotNull(State, "[WompCollide] Game State is unassigned");

        _animator = GetComponentInParent<Animator>();
    }
    #endregion

    public void happen()
    {
        OnTriggerEnter2D(null);
    }

    #region Unity Events
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (State.AudioIsEnabled)
        {
            WompClip.Play();
        }

        if (!string.IsNullOrEmpty(AnimationClip))
        {
            Assert.IsNotNull(_animator, "[WompCollide] No Animator found on " + gameObject.name);
            _animator.speed = 1f;
            _animator.Play(AnimationClip, -1, 0f);
        }
    }
    #endregion

    #region Resetable
    public override void OnReset()
    {
        if (!string.IsNullOrEmpty(AnimationClip))
        {
            Assert.IsNotNull(_animator, "[WompCollide] No Animator found on " + gameObject.name);
            _animator.Play(AnimationClip, -1, 0f);
            _animator.speed = 0f;
        }
    }
    #endregion
}

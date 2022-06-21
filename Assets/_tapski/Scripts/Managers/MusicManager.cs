using UnityEngine;
using UnityEngine.Assertions;

[RequireComponent(typeof(AudioSource))]
public class MusicManager : MonoBehaviour
{
    #region Public Properties
    public GameState State;
    #endregion

    #region Private Fields
    private AudioSource _audioSource;
    #endregion

    #region Unity Lifecycle
    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        Assert.IsNotNull(_audioSource, "[MusicManager] _audioSource was not found");
        Assert.IsNotNull(State, "[MusicManager] Game State is unassigned");
    }

    private void Update()
    {
        if (State.AudioIsEnabled && !_audioSource.isPlaying)
        {
            _audioSource.UnPause();
        }
        else if (!State.AudioIsEnabled && _audioSource.isPlaying)
        {
            _audioSource.Pause();
        }
    }
    #endregion
}

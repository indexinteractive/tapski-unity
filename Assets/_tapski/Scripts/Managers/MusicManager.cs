using UnityEngine;
using UnityEngine.Assertions;

public class MusicManager : MonoBehaviour
{
    #region Public Properties
    public GameState State;
    public AudioSource MainMenuClip;
    public AudioSource[] GameplayCips;
    #endregion

    #region Private Fields
    private AudioSource _audioSource;
    #endregion

    #region Unity Lifecycle
    private void Awake()
    {
        Assert.IsNotNull(MainMenuClip, "[MusicManager] Menu Music clip is unassigned");
        Assert.IsTrue(GameplayCips.Length > 0, "[MusicManager] No gameplay clips were assigned");

        Assert.IsNotNull(State, "[MusicManager] Game State is unassigned");

        SwitchToMenu();
    }

    private void Update()
    {
        if (_audioSource != null)
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
    }
    #endregion

    #region Public Methods
    public void StopAudio()
    {
        if (_audioSource != null)
        {
            _audioSource.Stop();
        }
    }

    public void SwitchToMenu()
    {
        Debug.Log("[MusicManager] Switching to main menu music");
        StopAudio();

        _audioSource = MainMenuClip;
        _audioSource.Play();
    }

    public void SwitchToGame()
    {
        Debug.Log("[MusicManager] Switching to game music");
        StopAudio();

        int clipIndex = Random.Range(0, GameplayCips.Length);
        _audioSource = GameplayCips[clipIndex];
        _audioSource.Play();
    }
    #endregion
}

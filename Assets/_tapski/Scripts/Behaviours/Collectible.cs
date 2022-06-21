using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Assertions;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(Renderer))]
public class Collectible : MonoBehaviour
{
    #region Public Properties
    public GameState State;
    #endregion

    #region Private Fields
    private AudioSource _audio;
    private Renderer _renderer;
    #endregion

    #region Unity Lifecycle
    private void Awake()
    {
        _audio = GetComponent<AudioSource>();
        _renderer = GetComponent<Renderer>();

        Assert.IsNotNull(State, "[Collectible] Game State is unassigned");
    }

    private void OnEnable()
    {
        _renderer.enabled = true;
    }
    #endregion

    #region Unity Events
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (State.AudioIsEnabled)
        {
            _audio.Play();
        }

        _renderer.enabled = false;
        DisableInSecondsAsync(_audio.clip.length);
    }
    #endregion

    #region Helpers
    private async void DisableInSecondsAsync(float seconds)
    {
        await Task.Delay(TimeSpan.FromSeconds(seconds));
        gameObject.SetActive(false);
    }
    #endregion
}

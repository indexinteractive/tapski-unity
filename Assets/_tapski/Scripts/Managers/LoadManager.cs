using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

[RequireComponent(typeof(AudioSource))]
public class LoadManager : MonoBehaviour
{
    #region Public Properties
    [Header("References")]
    public string MainMenuScene;
    public UIDocument SplashUI;
    public GameState State;

    [Header("Animation")]
    public float FadeInSec = 2f;
    public float FadeOutSec = 2f;
    #endregion

    #region Private Fields
    public VisualElement _uiRoot;
    public float _startingOpacity;
    public bool _fadeInComplete;
    public AsyncOperation _menuLoadOperation;
    #endregion

    #region Unity Lifecycle
    private void OnEnable()
    {
        // Without this line, performance on iOS is miserable.
        Application.targetFrameRate = 60;

        Assert.IsNotNull(MainMenuScene, "[LoadManager] MainMenuScene has no value");
        Assert.IsNotNull(SplashUI, "[LoadManager] SplashUI is unassigned");
        Assert.IsNotNull(State, "[LoadManager] Game State is unassigned");

        _uiRoot = SplashUI.rootVisualElement;
        _uiRoot.style.opacity = 0;

        var audio = GetComponent<AudioSource>();
        Assert.IsNotNull(audio, "[LoadManager] Unable to find AudioSource component");
        audio.enabled = State.AudioIsEnabled;

        LoadGameAsync();
        FlipSessionFlags();
    }
    #endregion

    #region Load Operations
    private void FlipSessionFlags()
    {
        State.WillShowTutorial = true;
    }

    private async void LoadGameAsync()
    {
        await Task.WhenAll(
            FadeUI(FadeInSec, 0, 1),
            LoadMenuScene(),
            AuthUser.Instance.GetUser(State)
        );

        await FadeUI(FadeOutSec, 1, 0);
        _menuLoadOperation.allowSceneActivation = true;

        Debug.Log("[LoadManager] Game loaded for userId " + AuthUser.Instance.UserId);
    }

    private async Task LoadMenuScene()
    {
        // NOTE: This unnecessary delay is here because of a bug with SceneManager.LoadSceneAsync
        // https://issuetracker.unity3d.com/issues/loadsceneasync-allowsceneactivation-flag-is-ignored-in-awake?page=1#comments
        await Task.Delay(1000);

        _menuLoadOperation = SceneManager.LoadSceneAsync(MainMenuScene, LoadSceneMode.Single);
        _menuLoadOperation.allowSceneActivation = false;

        while (_menuLoadOperation.progress < 0.9f)
        {
            await Task.Yield();
        }
    }
    #endregion

    #region UI Operations
    private async Task FadeUI(float durationSec, float startValue, float endValue)
    {
        float timeElapsed = 0;
        while (timeElapsed < durationSec)
        {
            _uiRoot.style.opacity = Mathf.Lerp(startValue, endValue, timeElapsed / durationSec);

            timeElapsed += Time.deltaTime;
            await Task.Yield();
        }

        _uiRoot.style.opacity = endValue;
    }
    #endregion
}

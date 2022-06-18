using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class LoadManager : MonoBehaviour
{
    #region Public Properties
    public string MainMenuScene;
    public UIDocument SplashUI;
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
        Assert.IsNotNull(MainMenuScene, "[LoadManager] MainMenuScene has no value");
        Assert.IsNotNull(SplashUI, "[LoadManager] SplashUI is unassigned");

        _uiRoot = SplashUI.rootVisualElement;
        _uiRoot.style.opacity = 0;

        LoadGameAsync();
    }
    #endregion

    #region Load Operations
    private async void LoadGameAsync()
    {
        await Task.WhenAll(
            FadeUI(FadeInSec, 0, 1),
            LoadMenuScene()
        );

        await FadeUI(FadeOutSec, 1, 0);
        _menuLoadOperation.allowSceneActivation = true;
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

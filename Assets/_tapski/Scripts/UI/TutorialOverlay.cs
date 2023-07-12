using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

/// <summary>
/// Appears based on the flag set in <see cref="GameState.WillShowTutorial" />
/// </summary>
[RequireComponent(typeof(UIDocument))]
public class TutorialOverlay : MonoBehaviour
{
    #region Public Properties
    [Header("References")]
    public GameState State;

    [Header("Menu Input Commands")]
    public InputAction InputSkipAction;

    [Header("Selectors")]
    public string ArrowLeftSelector = "arrow-left";
    public string ArrowRightSelector = "arrow-right";

    [Header("Settings")]
    public int MinWaitTimeMs = 3000;
    #endregion

    #region Private Fields
    private VisualElement _rootElement;
    private VisualElement _arrowLeft;
    private VisualElement _arrowRight;

    private bool _minTimeWaited;
    private bool _screenTapped;
    #endregion

    #region Unity Lifecycle
    private void OnEnable()
    {
        Assert.IsNotNull(State, "[TutorialOverlay] Game State is unassigned");

        _rootElement = GetComponent<UIDocument>()?.rootVisualElement;
        Assert.IsNotNull(_rootElement, "[TutorialOverlay] Overlay is unassigned");

        _arrowLeft = _rootElement.Q<VisualElement>(ArrowLeftSelector);
        Assert.IsNotNull(_arrowLeft, "[TutorialOverlay] Could not find left arrow");

        _arrowRight = _rootElement.Q<VisualElement>(ArrowRightSelector);
        Assert.IsNotNull(_arrowRight, "[TutorialOverlay] Could not find right arrow");

        _rootElement.schedule.Execute(FlipVisibility).Every(1200);
        _rootElement.schedule.Execute(SetWaitedFlag).StartingIn(MinWaitTimeMs);

        _screenTapped = false;
        _minTimeWaited = false;

        InputSkipAction.performed += OnOverlayTapped;
        InputSkipAction.Enable();
    }

    private void OnDisable()
    {
        InputSkipAction.Disable();
    }
    #endregion

    #region Screen Waiting
    public async Task WaitForSkipAsync()
    {
        while (!(_screenTapped && _minTimeWaited))
        {
            await Task.Yield();
        }

        State.WillShowTutorial = false;
    }
    #endregion

    #region Events
    private void SetWaitedFlag(TimerState evt)
    {
        _minTimeWaited = true;
    }

    private void OnOverlayTapped(InputAction.CallbackContext context)
    {
        _screenTapped = true;
    }

    private void FlipVisibility(TimerState obj)
    {
        if (_arrowLeft.style.visibility == Visibility.Hidden)
        {
            _arrowLeft.style.visibility = Visibility.Visible;
            _arrowRight.style.visibility = Visibility.Hidden;
        }
        else
        {
            _arrowLeft.style.visibility = Visibility.Hidden;
            _arrowRight.style.visibility = Visibility.Visible;
        }
    }
    #endregion
}

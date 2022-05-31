using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.InputSystem;

public enum PlayerStates
{
    Idle,
    Straight,
    TurnLeft,
    TurnRight,
    Jumping,
    Dead
}

public class BaseCharacter : MonoBehaviour
{
    #region Public Properties
    public bool ShowDebug = false;

    /// <summary>
    /// Keeps track of time spend in a state
    /// </summary>
    public float StateTimer = 0f;

    /// <summary>
    /// Indicates the player's current state. Changing the state will
    /// automatically play it's associated animation
    /// </summary>
    public PlayerStates State
    {
        get { return state; }

        set
        {
            if (state != value)
            {
                StateTimer = 0f;
                state = value;
                PlayAnimation(value);
            }
        }
    }
    #endregion

    #region Private / Inherited Fields
    private Animator _animator;

    private float _screenMidpoint;
    private PlayerInput _input;
    private bool _inputIsPressed = false;

    private PlayerStates state;
    #endregion

    #region Unity Lifecycle
    private void Awake()
    {
        _animator = GetComponent<Animator>();
        Assert.IsNotNull(_animator, $"[BaseCharacter] No Animator found on character {name}");

        _screenMidpoint = Screen.width / 2;

        _input = new PlayerInput();
        _input.Enable();

        _input.Player.Tap.performed += OnInputEvent;

        state = PlayerStates.Idle;
    }

    private void Update()
    {
        StateTimer += Time.deltaTime;

        if (_inputIsPressed)
        {
            Vector2 position = Pointer.current.position.ReadValue();
            if (position.x < _screenMidpoint)
            {
                State = PlayerStates.TurnLeft;
            }
            else
            {
                State = PlayerStates.TurnRight;
            }

            if (ShowDebug)
            {
                Debug.Log("Tap: " + position);
            }
        }
        else
        {
            State = PlayerStates.Straight;
        }
    }
    #endregion

    #region Input
    private void OnInputEvent(UnityEngine.InputSystem.InputAction.CallbackContext e)
    {
        _inputIsPressed = e.ReadValueAsButton();
    }
    #endregion

    #region Animations
    protected void PlayAnimation(PlayerStates state)
    {
        _animator.Play(state.ToString());
    }
    #endregion
}

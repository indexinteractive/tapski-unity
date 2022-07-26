using System;
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
    #region Traits
    [Header("Timing")]
    [Tooltip("Amount of time before the player sets off running (seconds)")]
    public float RestTime = 1.3f;

    [Tooltip("Amount of time the player is in the air while jumping (seconds)")]
    public float JumpTime = 0.5f;

    [Header("Speeds")]
    public AnimationCurve SpeedCurve;

    [Tooltip("Max speed the player can move")]
    public float MaxSpeed = 1f;

    [Tooltip("Percent speed increase while in the air")]
    public float JumpSpeedMultiplier = 0.2f;

    [Header("Speed curve values")]
    [Tooltip("Rate at which the speed curve (x) increases")]
    public float SpeedCurveRate = 0.05f;

    [Tooltip("Maximum value that can be reached on the speed curve")]
    public float MaxCurveValue = 1f;

    [Tooltip("Strength in which the turn is executed")]
    public float DriftFactor = 1.5f;

    [Tooltip("Rate of turn drift")]
    public float DriftRate = 0.5f;
    #endregion

    #region Public Properties
    /// <summary>
    /// Indicates the player's current state. Changing the state will
    /// automatically play it's associated animation
    /// </summary>
    public PlayerStates State
    {
        get { return _state; }

        private set
        {
            if (_state != value)
            {
                _stateTimer = 0f;
                _state = value;
                PlayAnimation(value);
            }
        }
    }

    /// <summary>
    /// Indicates the direction the object is facing
    /// </summary>
    public bool FacesLeft { get; set; }

    public Action OnPlayerDied;
    #endregion

    #region Private / Inherited Fields
    private Animator _animator;
    private PlayerInput _input;
    private PlayerStates _state;

    /// <summary>
    /// Keeps track of time spent in a state
    /// </summary>
    private float _stateTimer = 0f;

    /// <summary>
    /// Tracks the time spent accelerating to pick a speed from <see cref="SpeedCurve"/>
    /// </summary>
    private float _speedCurveX;

    /// <summary>
    /// Tracks the amount of drift that will be applied in the horizontal direction
    /// </summary>
    private float _driftAmount;
    #endregion

    #region Unity Lifecycle
    private void Awake()
    {
        _animator = GetComponent<Animator>();
        Assert.IsNotNull(_animator, $"[BaseCharacter] No Animator found on character {name}");

        _input = new PlayerInput();
        _input.Enable();

        _input.Player.Tap.performed += OnTapInput;
        _input.Player.DirectionKey.performed += OnDirectionKeyInput;

        _state = PlayerStates.Idle;
    }

    private void Update()
    {
        _stateTimer += Time.deltaTime;

        switch (State)
        {
            case PlayerStates.Idle:
                if (_stateTimer > RestTime)
                {
                    State = PlayerStates.Straight;
                }

                break;

            case PlayerStates.Straight:
                _driftAmount = Mathf.Clamp(_driftAmount -= Time.deltaTime, 0, DriftRate);

                // Speed the player up while under the max speed
                if (_speedCurveX < MaxCurveValue)
                {
                    _speedCurveX += Time.deltaTime * SpeedCurveRate;
                }

                // If above max speed, the player just jumped, slow down
                if (_speedCurveX >= MaxCurveValue)
                {
                    _speedCurveX -= Time.deltaTime;
                }
                break;

            case PlayerStates.TurnLeft:
            case PlayerStates.TurnRight:
                _driftAmount = Mathf.Clamp(_driftAmount += Time.deltaTime, 0, DriftRate);
                break;

            case PlayerStates.Jumping:
                _speedCurveX += Mathf.Clamp((_speedCurveX * JumpSpeedMultiplier) * Time.deltaTime, 0, MaxCurveValue);

                if (_stateTimer > JumpTime)
                {
                    State = PlayerStates.Straight;
                }
                break;

            case PlayerStates.Dead:
                _speedCurveX = 0;
                _driftAmount = 0;

                if (_stateTimer > RestTime && OnPlayerDied != null)
                {
                    OnPlayerDied.Invoke();
                    OnPlayerDied = null;
                }
                break;
        }

        UpdateMovement();
    }

    private void UpdateMovement()
    {
        var vertical = SpeedCurve.Evaluate(_speedCurveX) * MaxSpeed;
        var horizontal = SpeedCurve.Evaluate(_speedCurveX) * MaxSpeed * (_driftAmount * DriftFactor);

        Vector3 newPosition = new Vector3(
            transform.position.x + (horizontal * Time.deltaTime) * (FacesLeft ? -1 : 1),
            transform.position.y - (vertical * Time.deltaTime),
            transform.position.z
        );

        transform.position = newPosition;
    }
    #endregion

    #region Input
    private bool InputStateIsValid()
    {
        if (_state == PlayerStates.Jumping || _state == PlayerStates.Idle)
        {
            return false;
        }

        return true;
    }

    private void OnTapInput(UnityEngine.InputSystem.InputAction.CallbackContext e)
    {
        bool inputIsPressed = e.ReadValueAsButton();

        if (InputStateIsValid() && inputIsPressed)
        {

            float screenMidpoint = Screen.width / 2;
            Vector2 position = Pointer.current.position.ReadValue();

            if (position.x < screenMidpoint)
            {
                if (!FacesLeft)
                {
                    _driftAmount = 0;
                }

                State = PlayerStates.TurnLeft;
                FacesLeft = true;
            }
            else
            {
                if (FacesLeft)
                {
                    _driftAmount = 0;
                }

                State = PlayerStates.TurnRight;
                FacesLeft = false;
            }
        }
        else
        {
            State = PlayerStates.Straight;
        }
    }

    private void OnDirectionKeyInput(InputAction.CallbackContext e)
    {
        if (!InputStateIsValid())
        {
            return;
        }

        float value = e.ReadValue<float>();
        if (value > 0)
        {
            if (FacesLeft)
            {
                _driftAmount = 0;
            }

            State = PlayerStates.TurnRight;
            FacesLeft = false;
        }
        else if (value < 0)
        {
            if (!FacesLeft)
            {
                _driftAmount = 0;
            }

            State = PlayerStates.TurnLeft;
            FacesLeft = true;
        }
        else
        {
            State = PlayerStates.Straight;
        }
    }
    #endregion

    #region Animations
    protected void PlayAnimation(PlayerStates state)
    {
        _animator.Play(state.ToString());
    }
    #endregion

    #region Interactions
    public void OnCollideWithObstacle()
    {
        _input.Player.Tap.performed -= OnTapInput;
        _input.Player.DirectionKey.performed -= OnDirectionKeyInput;

        State = PlayerStates.Dead;
    }

    public void OnCollideWithRamp()
    {
        State = PlayerStates.Jumping;
    }
    #endregion
}

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

    [Tooltip("Max speed while jumping")]
    public float JumpSpeedMultiplier = 1.4f;
    #endregion

    #region Public Properties
    /// <summary>
    /// Keeps track of time spent in a state
    /// </summary>
    private float _stateTimer = 0f;

    /// <summary>
    /// Tracks the time spent accelerating to pick a speed from <see cref="SpeedCurve"/>
    /// </summary>
    private float _speedCurveX;

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
    #endregion

    #region Private / Inherited Fields
    private Animator _animator;
    // private TrailRenderer _trail;
    private PlayerInput _input;
    private PlayerStates _state;

    /// <summary>
    /// Velocity of the object per update
    /// </summary>
    private Vector2 _velocity;

    /// <summary>
    /// Player's current descent speed
    /// </summary>
    private float _verticalSpeed = 0;
    #endregion

    #region Unity Lifecycle
    private void Awake()
    {
        _animator = GetComponent<Animator>();
        // _trail = GetComponent<TrailRenderer>();
        Assert.IsNotNull(_animator, $"[BaseCharacter] No Animator found on character {name}");
        // Assert.IsNotNull(_trail, $"[BaseCharacter] No TrailRenderer found on character {name}");

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
                // Speed the player up while under the max speed
                if (_verticalSpeed < MaxSpeed)
                {
                    _speedCurveX += Time.deltaTime;
                    _verticalSpeed = SpeedCurve.Evaluate(_speedCurveX) * MaxSpeed;
                }

                // If above max speed, the player just jumped, slow down
                if (_verticalSpeed > MaxSpeed)
                {
                    _speedCurveX -= Time.deltaTime;
                }

                _velocity = new Vector2(0, _verticalSpeed);
                break;

            case PlayerStates.TurnLeft:
            case PlayerStates.TurnRight:
                _speedCurveX -= Time.deltaTime;
                _verticalSpeed = SpeedCurve.Evaluate(_speedCurveX) * MaxSpeed;

                _velocity = new Vector2(_verticalSpeed * 0.5f, _verticalSpeed);
                break;

            case PlayerStates.Jumping:
                _velocity = new Vector2(0, _verticalSpeed);

                if (_stateTimer > JumpTime)
                {
                    State = PlayerStates.Straight;
                }
                break;

            case PlayerStates.Dead:
                if (_stateTimer > RestTime)
                {
                    // levelManager.EndGame();
                }
                break;
        }

        UpdateMovement();
    }

    private void UpdateMovement()
    {
        Vector3 newPosition = new Vector3(
            transform.position.x + (_velocity.x * Time.deltaTime) * (FacesLeft ? -1 : 1),
            transform.position.y - (_velocity.y * Time.deltaTime),
            transform.position.z
        );

        transform.position = newPosition;
    }
    #endregion

    #region Input
    private void OnTapInput(UnityEngine.InputSystem.InputAction.CallbackContext e)
    {
        if (_state == PlayerStates.Jumping || _state == PlayerStates.Idle)
        {
            return;
        }

        bool inputIsPressed = e.ReadValueAsButton();

        if (inputIsPressed)
        {
            float screenMidpoint = Screen.width / 2;
            Vector2 position = Pointer.current.position.ReadValue();

            if (position.x < screenMidpoint)
            {
                State = PlayerStates.TurnLeft;
                FacesLeft = true;
            }
            else
            {
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
        if (_state == PlayerStates.Jumping || _state == PlayerStates.Idle)
        {
            return;
        }

        float value = e.ReadValue<float>();
        if (value > 0)
        {
            State = PlayerStates.TurnRight;
            FacesLeft = false;
        }
        else if (value < 0)
        {
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
        _velocity = Vector2.zero;
        _speedCurveX = 0;

        _input.Player.Tap.performed -= OnTapInput;
        _input.Player.DirectionKey.performed -= OnDirectionKeyInput;

        // Setting the trail lifespan to <infinity> will keep it from disappearing on collision
        // _trail.time = float.PositiveInfinity;

        State = PlayerStates.Dead;
    }

    public void OnCollideWithRamp()
    {
        _verticalSpeed = _verticalSpeed * JumpSpeedMultiplier;
        State = PlayerStates.Jumping;
    }
    #endregion
}

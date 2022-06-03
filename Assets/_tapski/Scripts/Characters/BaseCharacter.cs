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
    public float JumpSpeed = 2f;
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

    /// <summary>
    /// Initial turning speed
    /// </summary>
    private float _horizontalSpeed;
    #endregion

    #region Unity Lifecycle
    private void Awake()
    {
        _animator = GetComponent<Animator>();
        Assert.IsNotNull(_animator, $"[BaseCharacter] No Animator found on character {name}");

        _input = new PlayerInput();
        _input.Enable();

        _input.Player.Tap.performed += OnInputEvent;

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
                _velocity = new Vector2(0, MaxSpeed);
                // Speed the player up while under the max speed
                // if (_verticalSpeed < MaxSpeed)
                // {
                //     _speedCurveX += Time.deltaTime;
                //     _verticalSpeed = SpeedCurve.Evaluate(_speedCurveX) * MaxSpeed;
                // }

                // If above max speed, the player just jumped, slow down
                // if (_verticalSpeed > MaxSpeed)
                // {
                //     _speedCurveX -= Time.deltaTime;
                // }

                // _velocity = new Vector2(0, _verticalSpeed);

                // trailController.AddTrail(WorldRectangle);
                break;

            case PlayerStates.TurnLeft:
            case PlayerStates.TurnRight:
                _velocity = new Vector2(MaxSpeed * 0.5f, MaxSpeed);
                // When turning, slow down the vertical speed
                // if (_verticalSpeed > _horizontalSpeed)
                // {
                //     _verticalSpeed -= (float)Mathf.Log(_verticalSpeed) * Time.deltaTime;
                // }

                // Speed up the turning velocity
                // if (_horizontalSpeed < MaxSpeed)
                // {
                //     _horizontalSpeed += (float)Math.Pow(_horizontalSpeed, 0.25) * Time.deltaTime;
                // }

                // _velocity = new Vector2(_horizontalSpeed, _verticalSpeed);

                // trailController.AddTrail(WorldRectangle, 0);
                break;

            case PlayerStates.Jumping:
                // if (_verticalSpeed < JumpSpeed)
                // {
                //     _verticalSpeed += (float)Math.Pow(_verticalSpeed * 0.01, 2) * Time.deltaTime;
                // }

                // _velocity = new Vector2(0, _verticalSpeed);

                // if (_stateTimer > JumpTime)
                // {
                //     State = PlayerStates.Straight;
                // }
                break;

            case PlayerStates.Dead:
                if (_stateTimer > RestTime)
                {
                    // levelManager.EndGame();
                }
                break;
        }

        Debug.Log($"{name} velocity: {_velocity}");
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
    private void OnInputEvent(UnityEngine.InputSystem.InputAction.CallbackContext e)
    {
        if (_state == PlayerStates.Dead || _state == PlayerStates.Idle)
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
    #endregion

    #region Animations
    protected void PlayAnimation(PlayerStates state)
    {
        _animator.Play(state.ToString());
    }
    #endregion
}

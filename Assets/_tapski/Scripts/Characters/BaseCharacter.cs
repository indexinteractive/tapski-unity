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
    public float REST_TIME = 1.3f;

    [Tooltip("Amount of time the player is in the air while jumping (seconds)")]
    public float JUMP_TIME = 0.5f;

    [Header("Speeds")]
    [Tooltip("Max speed the player can move (pixels per second)")]
    public float MAX_SPEED = 1f;

    [Tooltip("Max speed while jumping")]
    public float JUMP_SPEED = 2f;
    #endregion

    #region Public Properties
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

    /// <summary>
    /// Indicates the direction the object is facing
    /// </summary>
    public bool FacesLeft { get; set; }
    #endregion

    #region Private / Inherited Fields
    private Animator _animator;

    private PlayerInput _input;

    private PlayerStates state;

    /// <summary>
    /// Velocity of the object per update (pixels per second)
    /// </summary>
    private Vector2 velocity;

    /// <summary>
    /// Player's current descent speed (pixels per second)
    /// </summary>
    private float verticalSpeed;

    /// <summary>
    /// Initial turning speed
    /// </summary>
    private float horizontalSpeed;

    /// <summary>
    /// Determines if this object has already had a collision
    /// </summary>
    protected bool firstCollision = true;

    /// <summary>
    /// Counts how many times the player has died in order to decide
    /// when to show a popup ad
    /// </summary>
    public int Deaths { get; set; }
    #endregion

    #region Unity Lifecycle
    private void Awake()
    {
        _animator = GetComponent<Animator>();
        Assert.IsNotNull(_animator, $"[BaseCharacter] No Animator found on character {name}");

        verticalSpeed = 0.1f;

        _input = new PlayerInput();
        _input.Enable();

        _input.Player.Tap.performed += OnInputEvent;

        state = PlayerStates.Idle;
    }

    private void Update()
    {
        StateTimer += Time.deltaTime;

        switch (State)
        {
            case PlayerStates.Idle:
                if (StateTimer > REST_TIME)
                {
                    State = PlayerStates.Straight;
                }

                break;

            case PlayerStates.Straight:
                // Set the turning speed back to a percentage of the vertical speed
                horizontalSpeed = verticalSpeed * 0.5f;

                // Speed the player up while under the max speed
                if (verticalSpeed < MAX_SPEED)
                {
                    verticalSpeed += (float)Mathf.Pow(verticalSpeed, 0.25f) * Time.deltaTime;
                }

                // If above max speed, the player just jumped, slow down
                if (verticalSpeed > MAX_SPEED)
                {
                    verticalSpeed -= (float)Mathf.Log(verticalSpeed) * Time.deltaTime;
                }

                velocity = new Vector2(0, verticalSpeed);

                // trailController.AddTrail(WorldRectangle);
                break;

            case PlayerStates.TurnLeft:
            case PlayerStates.TurnRight:
                // When turning, slow down the vertical speed
                if (verticalSpeed > horizontalSpeed)
                {
                    verticalSpeed -= (float)Mathf.Log(verticalSpeed) * Time.deltaTime;
                }

                // Speed up the turning velocity
                if (horizontalSpeed < MAX_SPEED)
                {
                    horizontalSpeed += (float)Math.Pow(horizontalSpeed, 0.25) * Time.deltaTime;
                }

                velocity = new Vector2(horizontalSpeed, verticalSpeed);

                // trailController.AddTrail(WorldRectangle, 0);
                break;

            case PlayerStates.Jumping:
                if (verticalSpeed < JUMP_SPEED)
                {
                    verticalSpeed += (float)Math.Pow(verticalSpeed * 0.01, 2) * Time.deltaTime;
                }

                velocity = new Vector2(0, verticalSpeed);

                if (StateTimer > JUMP_TIME)
                {
                    State = PlayerStates.Straight;
                }
                break;

            case PlayerStates.Dead:
                if (firstCollision && StateTimer > REST_TIME)
                {
                    Deaths++;
                    // levelManager.EndGame();
                    firstCollision = false;
                }
                break;
        }

        Debug.Log($"{name} velocity: {velocity}");
        UpdateMovement();
    }

    private void UpdateMovement()
    {
        // Check collisions against the WORLD only
        // moveAmount = CheckWorldCollisions(moveAmount);

        // Now make sure we are within world boundaries (with a little
        // extra room to fly over the top of the map)
        // FINALLY! Set the object's position to the newly calculated position
        // WorldOrigin.X = MathHelper.Clamp(newPosition.X, -CollisionRectangle.Width / 2, Camera.WorldRectangle.Width - CollisionRectangle.Width);
        // WorldOrigin.Y = MathHelper.Clamp(newPosition.Y, (-FrameHeight), Camera.WorldRectangle.Height - FrameHeight);

        Vector3 newPosition = new Vector3(
            transform.position.x + (velocity.x * Time.deltaTime) * (FacesLeft ? -1 : 1),
            transform.position.y - (velocity.y * Time.deltaTime),
            transform.position.z
        );

        transform.position = newPosition;
    }
    #endregion

    #region Input
    private void OnInputEvent(UnityEngine.InputSystem.InputAction.CallbackContext e)
    {
        if (state == PlayerStates.Dead || state == PlayerStates.Idle)
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

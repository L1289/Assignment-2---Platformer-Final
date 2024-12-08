using UnityEngine;

public enum PlayerDirection
{
    left, right
}

public enum PlayerState
{
    idle, walking, jumping, dead
}

public class PlayerController : MonoBehaviour
{
    [SerializeField] private Rigidbody2D body;
    private PlayerDirection currentDirection = PlayerDirection.right;
    public PlayerState currentState = PlayerState.idle;
    public PlayerState previousState = PlayerState.idle;

    [Header("Horizontal")]
    public float maxSpeed = 5f;
    public float accelerationTime = 0.25f;
    public float decelerationTime = 0.15f;

    [Header("Vertical")]
    public float apexHeight = 5f;
    public float apexTime = 0.5f;
    public float numberOfJumps = 2;
    private float totalJumps;
    private bool multipleJump = false;

    [Header("Ground Checking")]
    public float groundCheckOffset = 0.6f;
    public Vector2 groundCheckSize = new(0.5f, 0.1f);
    public LayerMask groundCheckMask;

    private float accelerationRate;
    private float decelerationRate;

    private float gravity;
    private float initialJumpSpeed;

    private bool isGrounded = false;
    public bool isDead = false;

    private Vector2 velocity;

    public void Start()
    {
        body.gravityScale = 0;

        accelerationRate = maxSpeed / accelerationTime;
        decelerationRate = maxSpeed / decelerationTime;

        gravity = -2 * apexHeight / (apexTime * apexTime);
        initialJumpSpeed = 2 * apexHeight / apexTime;

        totalJumps = numberOfJumps;
    }

    public void Update()
    {
        previousState = currentState;

        CheckForGround();

        Vector2 playerInput = new Vector2();
        playerInput.x = Input.GetAxisRaw("Horizontal");

        if (isDead)
        {
            currentState = PlayerState.dead;
        }

        switch(currentState)
        {
            case PlayerState.dead:
                // do nothing - we ded.
                break;
            case PlayerState.idle:
                if (!isGrounded) currentState = PlayerState.jumping;
                else if (velocity.x != 0) currentState = PlayerState.walking;
                break;
            case PlayerState.walking:
                if (!isGrounded) currentState = PlayerState.jumping;
                else if (velocity.x == 0) currentState = PlayerState.idle;
                break;
            case PlayerState.jumping:
                if (isGrounded)
                {
                    if (velocity.x != 0) currentState = PlayerState.walking;
                    else currentState = PlayerState.idle;
                }
                break;
        }

        //Running the QuickTurn Function in update
        QuickTurn(playerInput);
        MovementUpdate(playerInput);
        JumpUpdate();


        if (!isGrounded)
            velocity.y += gravity * Time.deltaTime;
        else
            velocity.y = 0;

        body.velocity = velocity;
    }

    //QuickTurn Function to organize everything 
    private void QuickTurn(Vector2 playerInput)
    {
        //Left QuickTurn
        //If player decide to move left well moving in the right direction all speed will come full stop well changing the players direction to left
        if (playerInput.x < 0 && currentDirection==PlayerDirection.right)
        {
            velocity.x = 0;
            currentDirection = PlayerDirection.left;
        }

        //Right QuickTurn
        //If player decide to move right well moving in the left direction all speed will come full stop well changing the players direction to right
        else if (playerInput.x > 0 && currentDirection == PlayerDirection.left)
        {
            velocity.x = 0;
            currentDirection = PlayerDirection.right;
        }
    }

    private void MovementUpdate(Vector2 playerInput)
    {
        if (playerInput.x < 0)
            currentDirection = PlayerDirection.left;
        else if (playerInput.x > 0)
            currentDirection = PlayerDirection.right;

        if (playerInput.x != 0)
        {
            velocity.x += accelerationRate * playerInput.x * Time.deltaTime;
            velocity.x = Mathf.Clamp(velocity.x, -maxSpeed, maxSpeed);
        }
        else
        {
            if (velocity.x > 0)
            {
                velocity.x -= decelerationRate * Time.deltaTime;
                velocity.x = Mathf.Max(velocity.x, 0);
            }
            else if (velocity.x < 0)
            {
                velocity.x += decelerationRate * Time.deltaTime;
                velocity.x = Mathf.Min(velocity.x, 0);
            }
        }
    }

    private void JumpUpdate()
    {
        //initialJump
        if (isGrounded == true && Input.GetButtonDown("Jump"))
        {
            velocity.y = initialJumpSpeed;

            multipleJump = true;
            numberOfJumps--;
            isGrounded = false;

            if (numberOfJumps <= 0)
            {
                multipleJump = false;
                numberOfJumps = totalJumps;
            }
        }

        else if (multipleJump == true && Input.GetButtonDown("Jump"))
        {
            if (numberOfJumps >= 1)
            {
                velocity.y = initialJumpSpeed;

                numberOfJumps--;
            }

            else if (numberOfJumps <= 0)
            {
                multipleJump = false;
                numberOfJumps = totalJumps;
            }
        }
    }

    private void CheckForGround()
    {
        isGrounded = Physics2D.OverlapBox(
            transform.position + Vector3.down * groundCheckOffset,
            groundCheckSize,
            0,
            groundCheckMask);
    }

    public void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position + Vector3.down * groundCheckOffset, groundCheckSize);
    }

    public bool IsWalking()
    {
        return velocity.x != 0;
    }
    public bool IsGrounded()
    {
        return isGrounded;
    }

    public PlayerDirection GetFacingDirection()
    {
        return currentDirection;
    }
}

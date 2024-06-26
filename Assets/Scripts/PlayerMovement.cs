using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    private float horizontal;
    private Rigidbody2D rb;
    public Transform groundCheck;
    public LayerMask groundLayer;
    public ParticleSystem smokeFx;

    [Header("Jumping System")]
    [SerializeField]
    private float jumpForce = 10f;

    [SerializeField]
    [Range(0, 1)]
    private float jumpDecay = 0.5f;

    // JUMP BUFFERING TIME
    private float jumpBufferTime = 0.12f;
    private float jumpBufferCounter;

    // JUMP COYOTE TIME
    private float jumpCoyoteTime = 0.2f;
    private float jumpCoyoteCounter;

    [Header("Movement System")]
    public float moveSpeed = 8f;
    public float maxSpeed = 20f;
    public float acceleration = 38f;
    public float deceleration = 45f;

    [Header("Gravity")]
    public float gravityScale = 5f;
    public float fallGravityScale = 10f;

    // CHECKING PLAYER FACING DIRECTION
    private bool facingRight;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        facingRight = true;
    }

    // Update is called once per frame
    void Update()
    {
        UpdateMovementSpeed();
        Gravity();
        Flip();

        // Coyote time
        if (IsGrounded())
        {
            jumpCoyoteCounter = jumpCoyoteTime;
        }
        else
        {
            jumpCoyoteCounter -= Time.deltaTime;
        }

        // Check if player is not pressing jumping button
        if (!Input.GetButtonDown("Jump"))
        {
            jumpBufferCounter -= Time.deltaTime;
        }

        if (CanPlaySmokeFX())
        {
            smokeFx.Play();
        }

        // Restart player if it falls off the map
        RestartPlayer();
    }

    void FixedUpdate()
    {
        ApplyMovement();

        if (jumpBufferCounter > 0f && jumpCoyoteCounter > 0f)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            jumpBufferCounter = 0f;
        }
    }

    // ACCELERAT OR DECELERATE PLAYER
    private void UpdateMovementSpeed()
    {
        if (horizontal > 0f && Input.GetButtonDown("Horizontal") || horizontal < 0f && Input.GetButtonDown("Horizontal"))
        {
            moveSpeed = 0f;
        }

        // Increase speed to max speed if player is moving
        if (Mathf.Abs(horizontal) > 0f && moveSpeed < maxSpeed)
        {
            moveSpeed += acceleration * Time.deltaTime;
        }
        // Decrease speed to 0 if player stops moving
        else if (Mathf.Abs(horizontal) == 0f && moveSpeed > 0f)
        {
            moveSpeed -= deceleration * Time.deltaTime;
            jumpCoyoteCounter = 0f;
        }
    }

    private void ApplyMovement()
    {
        // Get max speed based on value of horizontal (-1, 1)
        float targetSpeedValue = horizontal * maxSpeed;

        // Get current velocity of the player
        float currentSpeedValue = rb.velocity.x;

        // If player is moving
        if (horizontal != 0)
        {
            currentSpeedValue = Mathf.MoveTowards(
                currentSpeedValue,
                targetSpeedValue,
                acceleration * Time.deltaTime
            );
        }
        else
        {
            currentSpeedValue = Mathf.MoveTowards(
                currentSpeedValue,
                0f,
                deceleration * Time.deltaTime
            );
        }

        // Apply new velocity to the player
        rb.velocity = new Vector2(currentSpeedValue, rb.velocity.y);
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        horizontal = context.ReadValue<Vector2>().x;
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            smokeFx.Play();
            jumpBufferCounter = jumpBufferTime;
        }

        if (context.canceled && rb.velocity.y > 0f)
        {
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * jumpDecay);
            jumpCoyoteCounter = 0f;
        }
    }

    private bool IsGrounded()
    {
        return Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer);
    }

    // GRAVITY FOR FALLING
    private void Gravity()
    {
        if (rb.velocity.y >= 0)
        {
            rb.gravityScale = gravityScale;
        }
        else
        {
            // smokeFx.Play();
            rb.gravityScale = fallGravityScale;
        }
    }

    private void RestartPlayer()
    {
        if (transform.position.y < -20f)
        {
            rb.velocity = Vector2.zero;
            transform.position = new Vector2(1f, -2f);
        }
    }

    private void Flip()
    {
        if (facingRight && horizontal < 0 || !facingRight && horizontal > 0)
        {
            facingRight = !facingRight;
            Vector3 scale = transform.localScale;
            scale.x *= -1;
            transform.localScale = scale;
        }
    }

    private bool CanPlaySmokeFX()
    {
        if (moveSpeed >= maxSpeed)
        {
            return true;
        }
        return false;
    }
}

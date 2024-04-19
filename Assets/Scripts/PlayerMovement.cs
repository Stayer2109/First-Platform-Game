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

    [Header("Player Movement")]
    public float moveSpeed = 8f;
    public float maxSpeed = 20f;
    public float acceleration = 38f;
    public float deceleration = 45f;
    public float jumpForce = 10f;

    [Header("Gravity")]
    public float baseGravity = 5f;
    public float maxFallSpeed = 10f;
    public float fallMultiplier = 1.5f;

    // JUMP BUFFERING TIME
    private float jumpBufferTime = 0.12f;
    private float jumpBufferCounter;

    // JUMP COYOTE TIME
    private float jumpCoyoteTime = 0.2f;
    private float jumpCoyoteCounter;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateMovementSpeed();
        Gravity();

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
            jumpBufferCounter = jumpBufferTime;
        }

        if (context.canceled && rb.velocity.y > 0f)
        {
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * 0.5f);
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
        if (rb.velocity.y < 0)
        {
            // Scale the gravity to fall faster
            float targetGravityScale = Mathf.Min(rb.gravityScale + fallMultiplier, maxFallSpeed);
            rb.gravityScale = targetGravityScale;
            rb.velocity = new Vector2(rb.velocity.x, Mathf.Max(rb.velocity.y, -maxFallSpeed));
        }
        else
        {
            rb.gravityScale = baseGravity;
        }
    }

    private void RestartPlayer()
    {
        if (transform.position.y < -20f)
        {
            rb.velocity = Vector2.zero;
            transform.position = Vector3.zero;
        }
    }
}

using UnityEngine;

public class Slime : MonoBehaviour
{
    Rigidbody2D rb;
    SpriteRenderer sprite;

    float changeDirectionTime;
    float directionChangeInterval = 3f;
    float nextJumpTime;
    float moveDirection = 1f;
    float jumpForce = 7f;
    float moveSpeed = 3f;
    bool isGrounded;
    float originX;
    float patrolRange = 10f; // distance left/right from origin the slime can move

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sprite = GetComponent<SpriteRenderer>();
        nextJumpTime = Time.time + Random.Range(1f, 3f);
        changeDirectionTime = Time.time + directionChangeInterval;
        originX = transform.position.x;
    }

    public void DestroySlime()
    {
        Destroy(gameObject);
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.contacts[0].normal.y > 0.7f)
        {
            isGrounded = true;
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        isGrounded = false;
    }

    // Update is called once per frame
    void Update()
    {
        // Keep slime inside patrol bounds: if it reaches the limit, force direction change
        float posX = transform.position.x;
        if (posX >= originX + patrolRange && moveDirection > 0f)
        {
            moveDirection = -1f;
            sprite.flipX = moveDirection < 0; // look left
            changeDirectionTime = Time.time + directionChangeInterval;
        }
        else if (posX <= originX - patrolRange && moveDirection < 0f)
        {
            moveDirection = 1f;
            sprite.flipX = moveDirection < 0; // look right
            changeDirectionTime = Time.time + directionChangeInterval;
        }

        if (isGrounded)
        {
                rb.linearVelocity = new Vector2(moveDirection * moveSpeed, rb.linearVelocity.y);
        }

        if (Time.time > changeDirectionTime)
        {
            // Toggle direction, but respect patrol bounds (don't toggle into a wall)
            moveDirection *= -1f;
            // If toggling would immediately move outside bounds, reverse again
            float projectedX = posX + moveDirection * 0.1f; // small probe
            if (projectedX > originX + patrolRange)
            {
                moveDirection = -1f;
            }
            else if (projectedX < originX - patrolRange)
            {
                moveDirection = 1f;
            }
            sprite.flipX = moveDirection < 0; 
            changeDirectionTime = Time.time + directionChangeInterval;
        }

        if (isGrounded && Time.time > nextJumpTime)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            nextJumpTime = Time.time + Random.Range(1f, 3f);
        }
    }
}

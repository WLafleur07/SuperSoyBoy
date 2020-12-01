using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(SpriteRenderer), typeof(Rigidbody2D), typeof(Animator))]
public class SoyBoyController : MonoBehaviour
{
    public AudioClip runClip;
    public AudioClip jumpClip;
    public AudioClip slideClip;
    private AudioSource audioSource;

    public float jump = 14f;

    public float airAccel = 3f;

    public float jumpDurationThreshold = 0.25f;
    private float jumpDuration;

    public bool isJumping;
    public float jumpSpeed = 8f;
    private float rayCastLengthCheck = 0.005f;
    private float width;
    private float height;

    public float speed = 14f;
    public float accel = 6f;
    // -x = left, -y = down, +x = right, +y = up
    private Vector2 input;
    private SpriteRenderer sr;
    private Rigidbody2D rb;
    private Animator animator;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();

        // grabbing width and height of soyboy and adding buffer
        width = GetComponent<Collider2D>().bounds.extents.x + 0.1f;
        height = GetComponent<Collider2D>().bounds.extents.y + 0.2f;

        audioSource = GetComponent<AudioSource>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // built in Unity control axes, values will be either -1, 0, 1
        input.x = Input.GetAxis("Horizontal");
        input.y = Input.GetAxis("Jump");

        animator.SetFloat("Speed", Mathf.Abs(input.x));

        if(input.x > 0f)
        {
            // if x is greater than 0, player is facing right
            // sprite gets flipped on X axis
            sr.flipX = false;
        }
        else
        {
            // otherwise player is facing left, sprite is set back to not flipped
            sr.flipX = true;
        }

        if (input.y >= 1f)
        {
            jumpDuration += Time.deltaTime;
            animator.SetBool("IsJumping", true);
        }
        else
        {
            isJumping = false;
            animator.SetBool("IsJumping", false);
            jumpDuration = 0f;
        }

        // checks to see if soyboy is on ground and isn't currently jumping
        if (PlayerIsOnGround() && !isJumping)
        {
            if (input.y > 0f)
            {
                isJumping = true;
                PlayAudioClip(jumpClip);
            }
            animator.SetBool("IsOnWall", false);
            if (input.x < 0f || input.x > 0f)
            {
                PlayAudioClip(runClip);
            }
        }

        if (isJumping && jumpDuration < jumpDurationThreshold)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpSpeed);
        }
    }

    void FixedUpdate()
    {
        var acceleration = 0f;
        var xVelocity = 0f;

        if (PlayerIsOnGround())
        {
            acceleration = accel;
        }
        else
        {
            acceleration = airAccel;
        }
        
        if(PlayerIsOnGround() && input.x == 0)
        {
            xVelocity = 0f;
        }
        else
        {
            // set current velocity to the x velocity of rb
            xVelocity = rb.velocity.x;
        }

        var yVelocity = 0f;
        if(PlayerIsTouchingGroundOrWall() && input.y == 1)
        {
            yVelocity = jump;
        }
        else
        {
            yVelocity = rb.velocity.y;
        }

        // force is added to rb by calculating current value of the horizontal axis controls multi by speed, then multi by accel
        rb.AddForce(new Vector2(((input.x * speed) - rb.velocity.x) * acceleration, 0));
        rb.velocity = new Vector2(xVelocity, yVelocity);

        if (IsWallToLeftOrRight() && !PlayerIsOnGround() && input.y == 1)
        {
            rb.velocity = new Vector2(-GetWallDirection() * speed * 0.75f, rb.velocity.y);

            animator.SetBool("IsOnWall", false);
            animator.SetBool("IsJumping", true);

            PlayAudioClip(jumpClip);
        }
        else if (!IsWallToLeftOrRight())
        {
            animator.SetBool("IsOnWall", false);
            animator.SetBool("IsJumping", true);
        }

        if (IsWallToLeftOrRight() && !PlayerIsOnGround())
        {
            animator.SetBool("IsOnWall", true);
            PlayAudioClip(slideClip);
        }
    }

    public bool PlayerIsOnGround()
    {
        bool groundCheck1 = Physics2D.Raycast(new Vector2(transform.position.x, transform.position.y - height), -Vector2.up, rayCastLengthCheck);

        bool groundCheck2 = Physics2D.Raycast(new Vector2(transform.position.x + (width - 0.2f), transform.position.y - height), -Vector2.up, rayCastLengthCheck);

        bool groundCheck3 = Physics2D.Raycast(new Vector2(transform.position.x - (width - 0.2f), transform.position.y - height), -Vector2.up, rayCastLengthCheck);

        if (groundCheck1 || groundCheck2 || groundCheck3)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool IsWallToLeftOrRight()
    {
        bool wallOnLeft = Physics2D.Raycast(new Vector2(transform.position.x - width, transform.position.y), -Vector2.right, rayCastLengthCheck);

        bool wallOnRight = Physics2D.Raycast(new Vector2(transform.position.x + width, transform.position.y), -Vector2.right, rayCastLengthCheck);

        if(wallOnLeft || wallOnRight)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool PlayerIsTouchingGroundOrWall()
    {
        if(PlayerIsOnGround() || IsWallToLeftOrRight())
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public int GetWallDirection()
    {
        bool isWallOnLeft = Physics2D.Raycast(new Vector2(transform.position.x - width, transform.position.y), -Vector2.right, rayCastLengthCheck);

        bool isWallOnRight = Physics2D.Raycast(new Vector2(transform.position.x + width, transform.position.y), -Vector2.right, rayCastLengthCheck);

        if(isWallOnLeft)
        {
            return -1;
        }
        else if(isWallOnRight)
        {
            return 1;
        }
        else
        {
            return 0;
        }
    }

    void PlayAudioClip(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            if (!audioSource.isPlaying) 
            { 
                audioSource.PlayOneShot(clip); 
            }
        }
    }
}


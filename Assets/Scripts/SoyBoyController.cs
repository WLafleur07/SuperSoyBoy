using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(SpriteRenderer), typeof(Rigidbody2D), typeof(Animator))]
public class SoyBoyController : MonoBehaviour
{
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
    }

    void FixedUpdate()
    {
        var acceleration = accel;
        var xVelocity = 0f;
        
        if(input.x == 0)
        {
            xVelocity = 0f;
        }
        else
        {
            // set current velocity to the x velocity of rb
            xVelocity = rb.velocity.x;
        }

        // force is added to rb by calculating current value of the horizontal axis controls multi by speed, then multi by accel
        rb.AddForce(new Vector2(((input.x * speed) - rb.velocity.x) * acceleration, 0));
        rb.velocity = new Vector2(xVelocity, rb.velocity.y);
    }
}

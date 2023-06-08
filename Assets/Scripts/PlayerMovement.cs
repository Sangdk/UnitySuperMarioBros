using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private new Camera camera;
    private new Rigidbody2D rigidbody;

    private Vector2 velocity;
    private float inputAxis;

    public float moveSpeed = 8f;
    public float maxJumpHeight = 5f;
    public float maxJumpTime = 1f;
    public float jumpForce => (2f * maxJumpHeight) / (maxJumpTime / 2f);
    public float gravity => (-2f *maxJumpHeight) / Mathf.Pow((maxJumpTime /2f), 2);

    public bool grounded { get; private set; }
    public bool jumping { get; private set; }
    public bool running => Mathf.Abs(velocity.x) > 0.25f || Mathf.Abs(inputAxis) > 0.25f;
    public bool sliding => (inputAxis > 0f && velocity.x < 0f) || (inputAxis < 0f && velocity.x > 0f);

    private void Awake()
    {
        rigidbody = GetComponent<Rigidbody2D>();
        camera = Camera.main;
        // Kiem tra gia tri isKinematic
        /*Debug.Log("isKinematic: " + rigidbody.isKinematic);*/
    }

    private void Update()
    {
        HorizontalMovement();

        grounded = rigidbody.Raycast(Vector2.down);
        /*Debug.Log("grounded = "+ grounded);*/
        if (grounded)
        {
            //Debug.Log("grounded true");
            groundedMovement();
        }

        applyGravity();
    }  

    private void HorizontalMovement()
    {
        inputAxis = Input.GetAxis("Horizontal");
        // moveSpeed *Time.deltaTime de dieu chinh luc quan tinh cua mario
        velocity.x = Mathf.MoveTowards(velocity.x, inputAxis * moveSpeed, moveSpeed * (Time.deltaTime * 4/3));

        if(rigidbody.Raycast(Vector2.right * velocity.x))
        {
            velocity.x = 0;
        }

        if(velocity.x > 0f)
        {
            transform.eulerAngles = Vector3.zero;
        } else if (velocity.x <0f)
        {
            transform.eulerAngles = new Vector3(0f, 180f, 0f);
        }
    }

    private void groundedMovement()
    {
        // doan nay gioi han trong luc cua player khi cham dat
        velocity.y = Mathf.Max(velocity.y, 0f);
        jumping = velocity.y > 0f;

        if (Input.GetButtonDown("Jump"))
        {
            velocity.y = jumpForce;
            jumping = true;
           /* Debug.Log("jump with velocity.y= " + velocity.y);*/
        }
    }

    private void applyGravity()
    {
        bool falling = velocity.y < 0f || !Input.GetButton("Jump");
        /*Debug.Log("falling= " + falling + " velocity.y= " + velocity.y + " velocity.x= " + velocity.x);*/
        float multiplier = falling ? 2f : 1f;
        velocity.y += gravity * multiplier * Time.deltaTime;
        velocity.y = Mathf.Max(velocity.y, gravity);
    }

    private void FixedUpdate()
    {
        Vector2 position = rigidbody.position;
        position += velocity * Time.fixedDeltaTime;

        Vector2 leftEdge = camera.ScreenToWorldPoint(Vector2.zero);
        Vector2 rightEdge = camera.ScreenToWorldPoint(new Vector2(Screen.width, Screen.height));
        position.x = Mathf.Clamp(position.x, leftEdge.x + 0.5f, rightEdge.x - 0.5f);

       /* Debug.Log("leftEdge.x: " + leftEdge.x);
        Debug.Log("rightEdge.x: " + rightEdge.x);
        Debug.Log("position.x: " + position.x);*/

        rigidbody.MovePosition(position);
        /*Debug.Log("Velocity X: " + velocity.x + "Time.fixedDeltaTime= "+ Time.fixedDeltaTime);*/
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            if(transform.DotTest(collision.transform, Vector2.down))
            {
                velocity.y = jumpForce / 2;
                jumping = true;
            }
        }

        if(collision.gameObject.layer != LayerMask.NameToLayer("PowerUp"))
        {
            if(transform.DotTest(collision.transform, Vector2.up))
            {
                velocity.y = 0f;
            }
        }
    }
}

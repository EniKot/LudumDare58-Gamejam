using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Move : MonoBehaviour
{
    
    public float moveSpeed = 5f;
    public float jumpForce = 7f;
    public int maxJumps = 2;

    private Rigidbody2D player;
    private int jumpCount = 0;
    private bool isGrounded = false;

    void Start()
    {
        player = GetComponent<Rigidbody2D>();
    }


    void Update()
    {
        float moveInput = Input.GetAxis("Horizontal");
        player.velocity = new Vector2(moveInput * moveSpeed, player.velocity.y);

        if (Input.GetButtonDown("Jump") && jumpCount < maxJumps)
        {
            player.velocity = new Vector2(player.velocity.x, jumpForce);
            jumpCount++;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Ground"))
        {
            isGrounded = true;
            jumpCount = 0;
        }
    }
    

        private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Ground"))
        {
            isGrounded = false;
        }
    }

}

using UnityEngine;
using System.Collections;
using System;

public class BlockScript : MonoBehaviour
{
    public int hitsToKill;
    public int points;
    private int numberOfHits;
    private float currentWobbleTimer;
    private float wobbleTimer = .05f;
    private bool isWobbling;
    private float wobbleAmount = .22f;
    private Vector3 blockLocation;

    private Animator blockAnim;

    private float rotationSpeed = 40;

    // Use this for initialization
    void Start()
    {
        blockLocation = transform.position;
        numberOfHits = 0;

        blockAnim = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (currentWobbleTimer > 0)
        {
            currentWobbleTimer -= Time.deltaTime;
        }
        else if (isWobbling)
        {
            isWobbling = false;
            blockAnim.SetBool("IsWobbling", isWobbling);
        }
        
        // Rotatate the block when it is destroyed based on the balls velocity 
        if (numberOfHits == hitsToKill)
        {
            transform.Rotate(Vector3.forward, 90 * Time.deltaTime * rotationSpeed);
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {

        if (collision.gameObject.tag == "Ball")
        {
            numberOfHits++;
            // get reference of player object
            GameObject player = GameObject.FindGameObjectsWithTag("Player")[0];

            // add points to score
            player.SendMessage("AddPoints", points);

            if (numberOfHits == hitsToKill)
            {
                // add points to score
                player.SendMessage("AddPoints", points);

                // update number of blocks destroyed
                player.SendMessage("IncrementBlocksDestroyed");

                // make block move using the velocity from the ball
                GetComponent<Rigidbody2D>().isKinematic = false;
                GetComponent<Rigidbody2D>().mass = .025f;

                // Prevent collision with unrelated gameobjects
                gameObject.layer = 8;

                // Get rotation speed based on player velocity
                Vector2 vol = collision.gameObject.GetComponent<Rigidbody2D>().velocity;
                GetComponent<Rigidbody2D>().AddForce(new Vector2(-vol.x, -vol.y));
                rotationSpeed = vol.x;

                // make the block transparent
                Transform blockSprite = transform.GetChild(0);
                Color col = blockSprite.GetComponent<Renderer>().material.color;
                blockSprite.GetComponent<Renderer>().material.color = new Color(col.r, col.g, col.b, col.a - 0.5f);

                // slow down time for impact

                // destroy the rigidbody after a couple seconds
                Destroy(this.gameObject, 4f);
            }

            // Only wobble the ball if it is NOT destroyed

            Wobble();
        }
    }

    public void Wobble()
    {
        blockAnim.Play("BlockCollision", -1, 0f);
        isWobbling = true;
        currentWobbleTimer = wobbleTimer;

        blockAnim.SetBool("IsWobbling", isWobbling);
    }

}

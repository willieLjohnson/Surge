using UnityEngine;
using System.Collections;
using System;

public class BallScript : MonoBehaviour
{

    private bool ballIsActive;
    private Vector3 ballPosition;
    private Vector3 ballScale;
    private Vector2 ballInitialForce;
    public Color ballColor;

    private Quaternion ballRotation;

    // GameObjects
    private GameObject playerObject;
    private GameObject ballSprite;
    private GameObject ballSparks;

    private GameObject cam;
    private GameObject burstPrefab;

    private Transform oneBlocks;
    private Transform twoBlocks;
    private Transform threeBlocks;
    private Transform fourBlocks;

    private LevelManager world;

    // Animations
    private Animator ballAnim;
    private Animator playerAnim;

    // Sound 
    public AudioClip hitSound;
    public AudioClip hitBlockSound;
    private float currentWobbleTimer;
    private static float wobbleTimer = .2f;
    private bool isWobbling;
    private static float wobbleAmount = .4f;
    private float lastHitTime;
    private object flare = 1f;
    private bool bgMusicIsStarted;

    void Awake() {
        // get block parents
        oneBlocks = GameObject.Find("One").transform;
        twoBlocks = GameObject.Find("Two").transform;
        threeBlocks = GameObject.Find("Three").transform;
        fourBlocks = GameObject.Find("Four").transform;

        // get needed references
        playerObject = GameObject.Find("Player");
        cam = GameObject.Find("Main Camera");
        burstPrefab = GameObject.Find("Burst");

        world = GameObject.Find("LevelManager").GetComponent<LevelManager>();

        ballSprite = GameObject.Find("BallSprite");
        ballSparks = GameObject.Find("Static");
        ballColor = world.playerColor;

        // anim
        ballAnim = GetComponent<Animator>();
        playerAnim = playerObject.GetComponent<Animator>();
    }
    void Start()
    {
        // create the force
        ballInitialForce = new Vector2(100.0f, 450.0f);

        // set to inactive
        ballIsActive = false;
        isWobbling = false;

        // ball's original info
        ballPosition = transform.position;
        ballScale = transform.localScale;
        ballRotation = transform.rotation;

        setVisualEffectsEnabled(false);
    }

    private void setVisualEffectsEnabled(bool v)
    {
        ballSprite.GetComponent<TrailRenderer>().enabled = v;
        ParticleSystem.EmissionModule emission = ballSparks.GetComponent<ParticleSystem>().emission;
        emission.enabled = v;
    }

    void Update()
    {
        // check for user input
        if (Input.GetButtonDown("Jump") == true || (Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Began && Input.GetTouch(0).phase != TouchPhase.Moved && playerObject.GetComponent<PlayerScript>().IsPlayerReady()))
        {
            // check if is the first play
            if (!ballIsActive)
            {
                // reset the force
                GetComponent<Rigidbody2D>().isKinematic = false;

                // add a force
                // roate
                if (transform.position.x >= 0)
                {
                    ballInitialForce = new Vector2(-175, 375);
                }
                else
                {
                    ballInitialForce = new Vector2(175, 375);
                }

                GetComponent<Rigidbody2D>().AddForce(ballInitialForce);

                setVisualEffectsEnabled(true);

                // set ball active
                ballIsActive = true;

                if (!bgMusicIsStarted) {
                    world.StartBackgroundMusic();
                    bgMusicIsStarted = true;
                } else {
                    world.MuffleBackgroundMusic();
                }

            }
        }

        if (!ballIsActive && playerObject != null)
        {

            // get and use the player position
            ballPosition.x = playerObject.transform.position.x;
            ballPosition.y = playerObject.transform.position.y + .45f;

            // apply player X position to the ball
            transform.position = ballPosition;
        }

        if (ballIsActive)
        {
            // rotate ball to the direction it moves
            Vector2 velocity = GetComponent<Rigidbody2D>().velocity;
            if (velocity != Vector2.zero)
            {
                float angle = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
            }

            // Wobble ball
            if (currentWobbleTimer > 0)
            {
                ballSprite.GetComponent<Renderer>().material.color = new Color(1 - ballColor.r, 1 - ballColor.g, 1 - ballColor.b);
                ballSprite.GetComponent<TrailRenderer>().startColor = new Color(1 - ballColor.r, 1 - ballColor.g, 1 - ballColor.b);
                currentWobbleTimer -= Time.deltaTime;
            }
            else if (isWobbling)
            {
                ballSprite.GetComponent<Renderer>().material.color = ballColor;
                ballSprite.GetComponent<TrailRenderer>().startColor = ballColor;

                isWobbling = false;
            }

            // Check if ball falls
            if (transform.position.y < -12)
            {
                //reset ball
                ballIsActive = false;
                ballPosition.x = playerObject.transform.position.x;
                ballPosition.y = playerObject.transform.position.y + .45f;
                GetComponent<Rigidbody2D>().velocity = Vector2.zero;
                transform.localScale = ballScale;

                // disable trail
                setVisualEffectsEnabled(false);

                // move ball back to original position
                transform.rotation = ballRotation;
                transform.position = ballPosition;

                GetComponent<Rigidbody2D>().isKinematic = true;

                playerObject.SendMessage("TakeLife");

                cam.SendMessage("ShakeCamera", 7);

                world.MuffleBackgroundMusic();
            }
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        GameObject collisionObj = collision.gameObject;

        if (ballIsActive)
        {
            // particle effet
            collisionParticle();

            Vector2 velocity = GetComponent<Rigidbody2D>().velocity;

            if (Time.time - lastHitTime > .01f) GetComponent<AudioSource>().PlayOneShot(hitSound);

            if (Time.time - lastHitTime < .5f)
            {
                GetComponent<AudioSource>().pitch += .1f;

                lastHitTime = Time.time;
            }
            else
            {
                GetComponent<AudioSource>().pitch = 1f;
                lastHitTime = Time.time;
            }


            float boostDirX = Math.Sign(velocity.x) != 0 ? Math.Sign(velocity.x) : 1;
            float boostDirY = Math.Sign(velocity.y) != 0 ? Math.Sign(velocity.y) : -1;

            // if (collision.gameObject == playerObject)
            // {
            //     float difference = (transform.position.x - playerObject.transform.position.x)*10;
            //     int differenceDir = Math.Sign(difference);

            //     Vector2 force = new Vector2(25f * difference, 0);

            //     if (boostDirX != differenceDir)
            //     {
            //         Debug.Log("Change");
            //         GetComponent<Rigidbody2D>().AddForce(force);
            //     }
            // }

            if (Math.Abs(velocity.y) < 20f && collision.gameObject.CompareTag("Border"))
            {
                Vector2 force = new Vector2(0, boostDirY * 37);
                GetComponent<Rigidbody2D>().AddForce(force);
                playerObject.SendMessage("Boost", 0.1f);
            }

            // Make sure that the ball does not get into a horizontal trap
            if (Math.Abs(velocity.y) < 52f && collision.gameObject == playerObject)
            {
                Vector2 force = new Vector2(0, boostDirY * 12);
                GetComponent<Rigidbody2D>().AddForce(force);
                playerObject.SendMessage("Boost", 0.1f);
            }

            // Make sure that the ball does not get into a vertical trap
            if (Math.Abs(velocity.x) < 20f && collision.gameObject.CompareTag("Player"))
            {
                Vector2 force = new Vector2(boostDirX * 37, 0);
                GetComponent<Rigidbody2D>().AddForce(force);
                playerObject.SendMessage("Boost", 0.1f);
            }

            if (collision.gameObject.CompareTag("Player")) playerAnim.Play("PeddleCollision", -1, 0);
            // Change block color

            if (collision.gameObject.CompareTag("Block"))
                switch (collision.gameObject.transform.parent.name)
                {
                    case "Four":
                        collisionObj.GetComponent<Transform>().SetParent(threeBlocks);
                        world.ChangeBlockColors();
                        break;
                    case "Three":
                        collisionObj.GetComponent<Transform>().SetParent(twoBlocks);
                        world.ChangeBlockColors();

                        break;
                    case "Two":
                        collisionObj.GetComponent<Transform>().SetParent(oneBlocks);
                        world.ChangeBlockColors();
                        break;
                    case "One":
                        flare = 2f;
                        playerObject.SendMessage("Boost", 1f);
                        break;
                }


            world.ShakeAllBlocks();

            // Wobble ball
            Wobble();


            // Screen shake
            cam.SendMessage("ShakeCamera", flare);
            flare = 1f;
        }
    }

    private void collisionParticle()
    {
        GameObject burst = Instantiate(burstPrefab, transform.position, Quaternion.identity);

        burst.GetComponent<ParticleSystem>().Play();
        Destroy(burst, burst.GetComponent<ParticleSystem>().main.duration);
    }

    void Wobble()
    {
        isWobbling = true;
        currentWobbleTimer = wobbleTimer;
        ballAnim.Play("BallCollision", -1, 0f);
    }
}

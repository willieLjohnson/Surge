using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuBallScript : MonoBehaviour
{
    public float ballSpeed;
    private float lastChange;
    // private Color newColor;
    private bool isWobbling;
    private float currentWobbleTimer;
    private float wobbleAmount;
    private Vector3 ballScale;
    private float wobbleTimer;

    // Use this for initialization
    void Start()
    {
        wobbleAmount = .1f;
        wobbleTimer = .2f;
        GetComponent<Rigidbody2D>().AddForce(new Vector2(UnityEngine.Random.insideUnitCircle.x * ballSpeed, UnityEngine.Random.insideUnitCircle.y * ballSpeed));
        ballScale = transform.localScale;
        // newColor = new Color(UnityEngine.Random.insideUnitCircle.y, UnityEngine.Random.insideUnitCircle.x, 1f);
    }

    private void Update()
    {
        // rotate ball to the direction it moves
        Vector2 velocity = GetComponent<Rigidbody2D>().velocity;
        if (velocity != Vector2.zero)
        {
            float angle = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }

        // // Change color of ball
        // if (Time.time - lastChange > 3f)
        // {
        //     newColor = new Color(UnityEngine.Random.insideUnitCircle.y, UnityEngine.Random.insideUnitCircle.x, UnityEngine.Random.insideUnitCircle.y);
        //     lastChange = Time.time;
        // }
        // GetComponent<Renderer>().material.color = Color.Lerp(GetComponent<Renderer>().material.color, newColor, Mathf.PingPong(Time.time,1.5f));

        // Wobble ball
        if (currentWobbleTimer > 0)
        {
            var wobblePos = UnityEngine.Random.insideUnitCircle * wobbleAmount;

            transform.localScale = new Vector3(ballScale.x + Math.Abs(wobblePos.x), ballScale.y + Math.Abs(wobblePos.x), ballScale.z);

            currentWobbleTimer -= Time.deltaTime;
        }
        else if (isWobbling)
        {
            if (transform.localScale != ballScale)
            {
                transform.localScale = ballScale;
            }
            isWobbling = false;
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {

        GameObject collisionObj = collision.gameObject;
        // particle effet
        GameObject burst = Instantiate(GameObject.Find("Burst"), transform.position, Quaternion.identity);

        burst.GetComponent<ParticleSystem>().Play();
		Destroy (burst, burst.GetComponent<ParticleSystem> ().main.duration);

        // Wobble ball
        Wobble();


    }

    void Wobble()
    {
        isWobbling = true;
        currentWobbleTimer = wobbleTimer;
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuBallScript : MonoBehaviour
{
    public float ballSpeed;
    private Vector3 ballOriginScale;
    
    private bool isWobbling;
    private float wobbleTimer;
    private float wobbleAmount;
    private float wobbleDuration;

    // Use this for initialization
    void Start()
    {
        wobbleAmount = .1f;
        wobbleDuration = .2f;

        ballOriginScale = transform.localScale;

        GetComponent<Rigidbody2D>().AddForce(new Vector2(UnityEngine.Random.insideUnitCircle.x * ballSpeed, UnityEngine.Random.insideUnitCircle.y * ballSpeed));
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

        if (wobbleTimer > 0)
        {
            var wobblePos = UnityEngine.Random.insideUnitCircle * wobbleAmount;

            transform.localScale = new Vector3(ballOriginScale.x + Math.Abs(wobblePos.x), ballOriginScale.y + Math.Abs(wobblePos.x), ballOriginScale.z);

            wobbleTimer -= Time.deltaTime;
        }
        else if (isWobbling)
        {
            if (transform.localScale != ballOriginScale)
            {
                transform.localScale = ballOriginScale;
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
        wobbleTimer = wobbleDuration;
    }
}

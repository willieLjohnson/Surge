using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenShake : MonoBehaviour
{
    private float shakeTimer = 0; // 0.10
    private float shakeAmount = 0.04f; // 0.04
    private bool isShaking;
    private Vector3 startPosition;
    private float currentShakeTimer;
    private float multiplier;

    void Start()
    {
        startPosition = transform.position;
        isShaking = false;
    }

    void Update()
    {
        if (currentShakeTimer > 0 && Time.timeScale > 0)
        {
            var shakePos = Random.insideUnitCircle * shakeAmount * multiplier;

            transform.position = new Vector3(startPosition.x + shakePos.x, startPosition.y + shakePos.y, startPosition.z);

            currentShakeTimer -= Time.deltaTime;
        }
        else if (isShaking)
        {
            if (transform.position != startPosition)
            {
                transform.position = startPosition;
            }
            isShaking = false;
        }
    }

    public void ShakeCamera(float amount)
    {
        multiplier = amount;
        isShaking = true;

        currentShakeTimer = shakeTimer;
    }
}


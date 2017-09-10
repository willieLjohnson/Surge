using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelManager : MonoBehaviour
{
    public Camera cam;
    public Color playerColor;
    private Color playerOpColor;

    public Color burstColor;

    public Color borderColor;
    public Color blockOneColor;
    public Color blockTwoColor;
    public Color blockThreeColor;
    public Color blockFourColor;

    public Color backgroundColor;

    private GameObject player;
    private GameObject ball;
    private GameObject borders;

    private GameObject blocks1;
    private GameObject blocks2;
    private GameObject blocks3;
    private GameObject blocks4;
    private GameObject burst;
    private GameObject staticEffect;
    public AudioClip backgroundMusic;
    private GameObject remainingLives;

    private GameObject hud;
    private Color burstOpColor;

    private GameObject pauseMenu;
    private Text levelDetailsText;
    private float intensity = 6f;
    private float freq;
    private bool muffled;
    private float maxFreq = 22000;
    private float minFreq = 400;
    private float[] samples = new float[512];
    private float[] freqBands = new float[8];
    private float[] bandBuffer = new float[8];
    private float[] bufferDecrease = new float[8];

    public string songName;

    public void Update()
    {
        if (SceneManager.GetActiveScene().buildIndex > 2)
        {
            GetComponent<AudioSource>().GetSpectrumData(samples, 0, FFTWindow.Hamming);
            MakeFreqBands();
            BandBuffer();

            float band1 = bandBuffer[0] / (intensity * 1.5f);
            float band2 = bandBuffer[1] / (intensity * 1.2f);
            float band3 = bandBuffer[2] / (intensity * 1.4f);
            float band4 = bandBuffer[3] / (intensity * 1.6f);
            float band5 = bandBuffer[4] / (intensity * 1.8f);
            float band6 = bandBuffer[5] / (intensity * 1f);
            float band7 = bandBuffer[6] / (intensity * 1.2f);
            float band8 = bandBuffer[7] / (intensity * 1.4f);

            foreach (Transform block in blocks1.transform)
            {
                block.localScale = new Vector3(1 + band1, 1 + band1, 1);
            }
            foreach (Transform block in blocks2.transform)
            {
                block.localScale = new Vector3(1 + band3, 1 + band3, 1);
            }
            foreach (Transform block in blocks3.transform)
            {
                block.localScale = new Vector3(1 + band4, 1 + band4, 1);
            }
            foreach (Transform block in blocks4.transform)
            {
                block.localScale = new Vector3(1 + band5, 1 + band5, 1);
            }

            foreach (Transform life in remainingLives.transform)
            {
                life.localScale = new Vector3(0.2f + band1 / (intensity * 1.35f), 0.2f + band1 / (intensity * 1.35f), 0.2f);
            }

            player.transform.localScale = new Vector3(1 + band1 / (intensity * 1.35f), 1 + band1 / (intensity * 1.35f), 1);
            ball.transform.parent.localScale = new Vector3(0.2f + band1 / (intensity * 5), 0.2f + band1 / (intensity * 5), 0.2f);

            if (freq < maxFreq && !muffled)
            {
                freq += (maxFreq / 1f) * Time.deltaTime;
            }
            else if (freq > minFreq && muffled)
            {
                freq -= (maxFreq / .5f) * Time.deltaTime;
            }

            freq = Mathf.Clamp(freq, minFreq, maxFreq);
            GetComponent<AudioSource>().GetComponent<AudioLowPassFilter>().cutoffFrequency = freq;
        }
    }

    void MakeFreqBands()
    {
        int count = 0;

        for (int i = 0; i < 8; i++)
        {
            float average = 0;
            int sampleCount = (int)Mathf.Pow(2, i) * 2;

            for (int j = 0; j < sampleCount; j++)
            {
                average += samples[count] * (count + 1);
                count++;

            }
            average /= count;

            freqBands[i] = average * 10;
        }

    }

    void BandBuffer()
    {
        for (int i = 0; i < 8; i++)
        {
            if (freqBands[i] > bandBuffer[i])
            {
                bandBuffer[i] = freqBands[i];
                bufferDecrease[i] = 0.01f;
            }

            if (freqBands[i] < bandBuffer[i])
            {
                bandBuffer[i] -= bufferDecrease[i];
                bufferDecrease[i] *= 1.5f;
            }
        }
    }
    // Change color of everything in the level
    public void ChangeColor()
    {
        // get all gameobjects
        player = GameObject.Find("PlayerSprite");
        ball = GameObject.Find("BallSprite");
        borders = GameObject.Find("Borders");

        blocks1 = GameObject.Find("One");
        blocks2 = GameObject.Find("Two");
        blocks3 = GameObject.Find("Three");
        blocks4 = GameObject.Find("Four");

        burst = GameObject.Find("Burst");
        staticEffect = GameObject.Find("Static");

        hud = GameObject.Find("UI");
        levelDetailsText = GameObject.Find("Details").GetComponent<Text>();

        remainingLives = GameObject.Find("Lives");

        // change player and ball color and particle effect
        player.GetComponent<Renderer>().material.color = playerColor;
        playerOpColor = new Color(1 - playerColor.r, 1 - playerColor.g, 1 - playerColor.b, 1);
        burstOpColor = new Color(1 - burstColor.r, 1 - burstColor.g, 1 - burstColor.b, 1);

        levelDetailsText.color = playerColor;

        ball.GetComponent<Renderer>().material.color = playerColor;

        ball.GetComponent<TrailRenderer>().startColor = playerColor;
        ball.GetComponent<TrailRenderer>().endColor = new Color(playerColor.r, playerColor.g, playerColor.b, .2f);

        var burstPS = burst.GetComponent<ParticleSystem>().main;
        burstPS.startColor = new ParticleSystem.MinMaxGradient(burstColor, burstOpColor);

        var staticPS = staticEffect.GetComponent<ParticleSystem>().main;
        staticPS.startColor = new ParticleSystem.MinMaxGradient(playerColor, playerOpColor);

        foreach (Transform hudElement in hud.transform)
        {
            if (hudElement.GetComponent<Text>() != null)
            {
                hudElement.GetComponent<Text>().color = playerColor;
                if (hudElement.name == "CurrentLevel") hudElement.GetComponent<Text>().text = "" + (SceneManager.GetActiveScene().buildIndex - 2);
            }
            else if (hudElement.GetComponent<Image>() != null)
            {
                hudElement.GetComponent<Image>().color = playerColor;
            }
        }

        foreach (Transform life in remainingLives.transform)
        {
            life.GetComponent<Renderer>().material.color = new Color(playerColor.r, playerColor.g, playerColor.b, .5f);
        }

        // change GetComponent<Camera>() background scolor
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = backgroundColor;

        // change boarder color
        foreach (Transform border in borders.transform)
        {
            border.GetComponent<Renderer>().material.color = borderColor;
        }

        ChangeBlockColors();
    }

    public void ChangeBlockColors()
    {
        Transform blockSprite;
        // change block 1 hit block color
        foreach (Transform block in blocks1.transform)
        {
            if (block.gameObject.layer != 8)
            {
                blockSprite = block.GetChild(0);
                blockSprite.GetComponent<Renderer>().material.color = blockOneColor;
            }
        }

        // change block 2 hit block color
        foreach (Transform block in blocks2.transform)
        {
            if (block.gameObject.layer != 8)
            {
                blockSprite = block.GetChild(0);
                blockSprite.GetComponent<Renderer>().material.color = blockTwoColor;
            }
        }

        // change block 3 hit block color
        foreach (Transform block in blocks3.transform)
        {
            if (block.gameObject.layer != 8)
            {
                blockSprite = block.GetChild(0);
                blockSprite.GetComponent<Renderer>().material.color = blockThreeColor;
            }
        }

        // change block 4 hit block color
        foreach (Transform block in blocks4.transform)
        {
            if (block.gameObject.layer != 8)
            {
                blockSprite = block.GetChild(0);
                blockSprite.GetComponent<Renderer>().material.color = blockFourColor;
            }
        }
    }

    public void StartBackgroundMusic()
    {
        GetComponent<AudioSource>().PlayOneShot(backgroundMusic);
    }

    public void MuffleBackgroundMusic()
    {
        if (GetComponent<AudioSource>().GetComponent<AudioLowPassFilter>().cutoffFrequency == maxFreq)
        {
            muffled = true;
        }
        else
        {
            muffled = false;
        }
    }

    public void StopBackgroundMusic()
    {
        GetComponent<AudioSource>().Stop();
    }
    internal void ShakeAllBlocks()
    {
        // change block 1 hit block color
        foreach (Transform block in blocks1.transform)
        {
            if (block.gameObject.layer != 8)
            {
                block.GetComponent<Animator>().Play("BlockShake", -1, UnityEngine.Random.insideUnitCircle.x);
            }
        }

        // change block 2 hit block color
        foreach (Transform block in blocks2.transform)
        {
            if (block.gameObject.layer != 8)
            {
                block.GetComponent<Animator>().Play("BlockShake", -1, UnityEngine.Random.insideUnitCircle.x);
            }
        }

        // change block 3 hit block color
        foreach (Transform block in blocks3.transform)
        {
            if (block.gameObject.layer != 8)
            {
                block.GetComponent<Animator>().Play("BlockShake", -1, UnityEngine.Random.insideUnitCircle.y);
            }
        }

        // change block 4 hit block color
        foreach (Transform block in blocks4.transform)
        {
            if (block.gameObject.layer != 8)
            {
                block.GetComponent<Animator>().Play("BlockShake", -1, UnityEngine.Random.insideUnitCircle.y);
            }
        }
    }
}

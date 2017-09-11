using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using System;
using UnityEngine.UI;

public class PlayerScript : MonoBehaviour
{
    // Magic numbers
    private float boundary = 4.7f;
    private float playerVelocity = 0.3f;

    private float levelInfoDisplayDuration = 20f;

    private float scoreWobbleAmount = 1.4f;
    private float scoreWobbleDuration = 0.1f;
    private float scoreLerpDuration = 0.1f * 2.1f;
    private float scoreMultiplier = 1f;

    private bool debug = false;

    // UI
    private GameObject levelInfoCanvas;
    private GameObject pauseMenuCanvas;
    private Text scoreText;
    private GameObject playerLives;
    private Text levelInfoText;
    private Text scoreMultiplierText;

    // Player
    private Vector3 playerPosition;
    private int playerScore;
    private int highScore;
    private int currentLevel;

    private float maintainedDist;

    // Sound Fx
    [Header("Sound Fx")]
    public AudioClip breakBlockSound;
    public AudioClip loseLifeSound;

    // Visual Fx
    private LevelManager levelManager;

    private float lastBreakTime;
    private int numberOfBlocksDestroyed;
    private int numberOfBlocksTotal;

    private Vector3 scoreTextOriginScale;
    private bool isScoreWobbling;
    private float scoreWobbleTimer;

    private float lastScoreTime;
    private int savedDisplayedScore;
    private float scoreLerpTimer;
    private float scoreBoostFactor;
    void Awake()
    {
        // UI
        playerLives = GameObject.Find("Lives");
        scoreText = GameObject.Find("Score").GetComponent<Text>();
        scoreMultiplierText = GameObject.Find("Multiplier").GetComponent<Text>();

        pauseMenuCanvas = GameObject.Find("PauseMenu");

        levelInfoCanvas = GameObject.Find("LevelDetails");
        levelInfoText = GameObject.Find("Details").GetComponent<Text>();

        // Visual Fx
        levelManager = GameObject.Find("LevelManager").GetComponent<LevelManager>();

        numberOfBlocksTotal = (GameObject.FindGameObjectsWithTag("Block")).Length;
    }
    void Start()
    {
        pauseMenuCanvas.SetActive(false);
        currentLevel = SceneManager.GetActiveScene().buildIndex;
        levelInfoText.text = "Level:\n" + (currentLevel - LevelManager.numberOfMenuScenes + 1) + "\n" + "Song:\n" + levelManager.songName + "\n" + "High Score:\n" + highScore;

        playerScore = 0;
        playerPosition = gameObject.transform.position;

        highScore = PlayerPrefs.GetInt("HighScoreLevel" + (currentLevel - LevelManager.numberOfMenuScenes + 1), 0);

        scoreTextOriginScale = scoreText.transform.localScale;

        levelManager.ChangeColor();
    }

    void Update()
    {
        if (!pauseMenuCanvas.activeInHierarchy)
        {
            HandleInput();
            UpdatePlayer();
            UpdateVisualFx();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            GameObject.Find("ButtonManager").GetComponent<ButtonManager>().PauseGame();
        }
    }

    private void HandleInput()
    {
        // Movement on pc
        playerPosition.x += Input.GetAxis("Horizontal") * playerVelocity;

        // Get first touch position of finger
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            Vector3 touchOrgPos = Camera.main.ScreenToWorldPoint(new Vector3(Input.GetTouch(0).position.x, Input.GetTouch(0).position.y, 5));
            maintainedDist = touchOrgPos.x - playerPosition.x;
        }

        // Move player based on first touch position
        if (Input.touchCount == 1)
        {
            if (levelInfoCanvas.activeInHierarchy == true)
            {
                levelInfoCanvas.SetActive(false);
                levelInfoDisplayDuration = 2;

                GameObject.Find("Ball").SendMessage("PlayerReady");
            }

            Vector3 touchPos = Camera.main.ScreenToWorldPoint(new Vector3(Input.GetTouch(0).position.x, Input.GetTouch(0).position.y, 5));
            playerPosition = new Vector3(touchPos.x - maintainedDist, playerPosition.y, playerPosition.z);
        }

        if (debug)
        {
            // Slow motion; Debug feature only
            if (Input.touchCount == 2 || Input.GetKeyDown(KeyCode.K))
            {
                if (Time.timeScale == 1)
                {
                    Time.timeScale = .2f;
                    Time.fixedDeltaTime = Time.timeScale * .02f;
                }
                else
                {
                    Time.timeScale = 1;
                    Time.fixedDeltaTime = Time.timeScale * .02f;
                }
            }

            // Switch level; Debug feature only
            if ((Input.touchCount == 3 && Input.GetTouch(0).phase == TouchPhase.Began) || Input.GetKeyDown("n"))
            {
                if (currentLevel == SceneManager.sceneCountInBuildSettings - 1) SceneManager.LoadScene(LevelManager.numberOfMenuScenes);
                else SceneManager.LoadScene(currentLevel + 1);
            }
        }
    }

    public void UpdatePlayer()
    {
        // Player boundaries
        if (playerPosition.x < -boundary)
        {
            playerPosition = new Vector3(-boundary, playerPosition.y, playerPosition.z);
        }
        if (playerPosition.x > boundary)
        {
            playerPosition = new Vector3(boundary, playerPosition.y, playerPosition.z);
        }

        transform.position = playerPosition;

        if (Time.time - lastScoreTime > 1f)
        {
            scoreMultiplier = 1f;
        }

        WinLose();
    }

    private void UpdateVisualFx()
    {
        if (levelInfoDisplayDuration > 0 && levelInfoCanvas.activeInHierarchy == true)
        {
            levelInfoCanvas.GetComponent<CanvasGroup>().alpha -= .001f;
            levelInfoDisplayDuration -= Time.deltaTime;
        }
        else
        {
            levelInfoCanvas.SetActive(false);
        }

        // Wobble score
        if (scoreWobbleTimer > 0 && Time.timeScale > 0)
        {
            var wobblePos = UnityEngine.Random.insideUnitCircle * (scoreWobbleAmount * Mathf.Clamp(scoreMultiplier / 2, 1, 2f));

            scoreText.transform.localScale = new Vector3(scoreTextOriginScale.x + Math.Abs(wobblePos.x), scoreTextOriginScale.y + Math.Abs(wobblePos.x), scoreTextOriginScale.z);

            scoreWobbleTimer -= Time.deltaTime;
        }
        else if (isScoreWobbling || scoreText.transform.localScale != scoreTextOriginScale)
        {
            scoreText.transform.localScale = scoreTextOriginScale;

            isScoreWobbling = false;
        }

        // Lerp Score 
        scoreLerpTimer += Time.deltaTime;
        float prcComplete = scoreLerpTimer / (scoreLerpDuration);
        scoreText.text = Math.Ceiling(Mathf.Lerp(savedDisplayedScore, playerScore, prcComplete)).ToString();

        scoreMultiplierText.text = "x" + (scoreMultiplier + scoreBoostFactor).ToString();
    }

    void AddPoints(int points)
    {
        savedDisplayedScore = Int32.Parse(scoreText.text);
        playerScore += (int)(points * (scoreMultiplier + scoreBoostFactor));
        scoreLerpTimer = 0f;

        if (Time.time - lastScoreTime < 1f)
        {
            scoreMultiplier += 0.5f;
        }
        else
        {
            scoreMultiplier = 1f;
        }

        lastScoreTime = Time.time;
        WobbleScore();
    }

    void Boost(int amount)
    {
        scoreBoostFactor += amount;
    }

    void IncrementBlocksDestroyed()
    {
        numberOfBlocksDestroyed++;

        if (Time.time - lastBreakTime > .01f)
            GetComponent<AudioSource>().PlayOneShot(breakBlockSound);

        if (Time.time - lastBreakTime < .7f)
        {
            GetComponent<AudioSource>().pitch += .15f;
            lastBreakTime = Time.time;
        }
        else
        {
            GetComponent<AudioSource>().pitch = 1f;
            lastBreakTime = Time.time;
        }
    }
    
    void TakeLife()
    {
        Destroy(playerLives.transform.GetChild(playerLives.transform.childCount - 1).gameObject, .1f);
        GetComponent<AudioSource>().pitch = 1f;
        GetComponent<AudioSource>().PlayOneShot(loseLifeSound);

        scoreBoostFactor = 0;
        scoreMultiplier = 1;

        scoreMultiplierText.text = "x" + (scoreMultiplier + scoreBoostFactor).ToString();
    }

    void WinLose()
    {
        // restart the game
        if (playerLives.transform.childCount == 0)
        {
            if (playerScore > PlayerPrefs.GetInt("HighestScore", 0))
            {
                PlayerPrefs.SetInt("HighestScore", playerScore);
            }

            if (playerScore > highScore)
            {
                PlayerPrefs.SetInt("HighScoreLevel" + (currentLevel - LevelManager.numberOfMenuScenes + 1), playerScore);
            }

            ResetLevel();
        }

        // chech if all blocks are destroyed
        if (numberOfBlocksTotal == numberOfBlocksDestroyed)
        {
            // next level or quit
            if (currentLevel > SceneManager.sceneCountInBuildSettings) Application.Quit();
            else SceneManager.LoadScene(currentLevel + 1);
        }
    }

    void WobbleScore()
    {
        isScoreWobbling = true;
        scoreWobbleTimer = scoreWobbleDuration;
    }

    public void ResetLevel()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(currentLevel);
    }
}

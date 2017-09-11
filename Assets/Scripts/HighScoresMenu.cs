using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class HighScoresMenu : MonoBehaviour
{
    int numberOfLevels;
    int highestScore;
    Text highestScoreText;
    Text highScores;

    void Awake()
    {
        highestScoreText = GameObject.Find("HighestScoreNum").GetComponent<Text>();
        highScores = GameObject.Find("Scores").GetComponent<Text>();
    }
    void Start()
    {
        numberOfLevels = SceneManager.sceneCountInBuildSettings - LevelManager.numberOfMenuScenes - 1;

        updateHighestScore();
        updateLevelScores();
    }

    void updateHighestScore()
    {
        highestScore = PlayerPrefs.GetInt("HighestScore", 0);
        highestScoreText.text = highestScore.ToString();
    }

    void updateLevelScores()
    {
        highScores.text = "";

        for (var i = 1; i <= numberOfLevels + 1; i++)
        {
            var level = "Level " + i;
            highScores.text += level + ": " + PlayerPrefs.GetInt("HighScoreLevel" + i).ToString() + "\n";
        }
    }
}

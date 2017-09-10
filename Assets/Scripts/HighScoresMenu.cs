using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class HighScoresMenu : MonoBehaviour {
	
	int numberOfLevels = 0;

	int highestScore;
	Text highestScoreText;
	Text highScores;
	// Use this for initialization
	void Start () {
		numberOfLevels = SceneManager.sceneCountInBuildSettings - LevelManager.numberOfMenuScenes - 1;
		highestScoreText = GameObject.Find("HighestScoreNum").GetComponent<Text>();
		highScores = GameObject.Find("Scores").GetComponent<Text>(); 
		updateHighestScore();
		updateLevelScores();
	}
	
	void Update () {
		
	}

	void updateHighestScore() {
		highestScore = PlayerPrefs.GetInt("HighestScore", 0);
		
		highestScoreText.text = highestScore.ToString();
	}

	void updateLevelScores() {
		highScores.text = "";
		print("Update Level Scores");
		print("Number of levels: " + numberOfLevels);
		for(var i = 1 ; i <= numberOfLevels + 1; i++) {
			var level = "Level " + i;
			highScores.text += level + ": " + PlayerPrefs.GetInt("HighScoreLevel" + i).ToString() + "\n";
		}
	}
}

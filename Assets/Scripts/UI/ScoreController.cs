using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ScoreController : MonoBehaviour
{
    public TMP_Text scoreText;
    public int currentScore;

    public TMP_Text finalScoreText;
    public TMP_Text highscoreText;

    public void AddScore(int scoreToAdd)
    {
        currentScore += scoreToAdd;
        scoreText.text = currentScore.ToString();
    }
}

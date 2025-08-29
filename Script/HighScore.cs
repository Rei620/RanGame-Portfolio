using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class HighScore : MonoBehaviour
{
    public CharaMove chara;
    public TextMeshProUGUI scoreText;
    public int highScore;
   
    public void Start()
    {

        int score = Score();
        highScore = PlayerPrefs.GetInt("SCORE", 0);
       
    }

    // Update is called once per frame
    public void Update()
    {

        int score = Score();
        scoreText.text = "HighScore: " + highScore + "m";

        if(highScore<score)
        {
            highScore = score;
            PlayerPrefs.SetInt("SCORE", highScore);
            PlayerPrefs.Save();

        }
       
    }

    int Score()
    {
        return (int)chara.transform.position.z;
    }

    
}

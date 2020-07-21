using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FeedbackCanvas : MonoBehaviour {

    [SerializeField]
    private TextMeshPro bounceText;

    [SerializeField]
    private Text scoreText;

    // High score for score
    //[SerializeField]
    //private Text hiScoreText;

    // High score for bounces
    //[SerializeField]
    //private Text hiBounceText;

    // The high scores for this game
    //private float hiScore = 0;
    //private int hiBounce = 0;

    public void UpdateScoreText(float curScore, int curBounces)
    {
        /**
        //Update high scores
        if (curScore > hiScore)
        {
            hiScore = curScore;
            hiScoreText.text = hiScore.ToString();
        }
        if (curBounces > hiBounce)
        {
            hiBounce = curBounces;
            hiBounceText.text = hiBounce.ToString();
        }
        **/

        // Update trial scores
        scoreText.text = curScore.ToString();
        bounceText.text = curBounces.ToString();
    }
}

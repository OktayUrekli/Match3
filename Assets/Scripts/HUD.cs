
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HUD : MonoBehaviour
{
    [SerializeField] GameOver gameOver;

    public Level level;

    public TextMeshProUGUI remainingText;
    public TextMeshProUGUI remainingSubText;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI warningText;

    public Image[] emptyStars;
    public Image[] fillStars;

    int visibleStarCount=0;

    void Start()
    {
        for (int i = 0; i < emptyStars.Length; i++)
        {
            emptyStars[i].gameObject.SetActive(true);
        }

        for (int i = 0; i < fillStars.Length; i++)
        {
            fillStars[i].gameObject.SetActive(false);
        }
    }

    public void SetScore(int _score)
    {      
        scoreText.text = _score.ToString();

        if (_score>=level.score1Star && _score<level.score2Star)
        {
            visibleStarCount = 1;
        }
        else if (_score>=level.score2Star && _score < level.score3Star)
        {
            visibleStarCount = 2;
        }
        else if (_score >= level.score3Star)
        {
            visibleStarCount = 3;
 
        }

        for (int i = 0; i < visibleStarCount; i++)
        {
            fillStars[i].gameObject.SetActive(true);
        }

    }

    public void SetReamining(int remainingMove)
    {
        remainingText.text = remainingMove.ToString();
    }

    public void SetReamining(string remainingTime)
    {
        remainingText.text = remainingTime.ToString();
    }

    public void SetLevelType(Level.LevelType type)
    {
        if (type==Level.LevelType.MOVES )
        {
            remainingSubText.text = "MOVES:";
            warningText.text = "WATCH OUT MOVES !!!";
        }
        else if (type == Level.LevelType.TIMER)
        {
            remainingSubText.text = "TIME:";
            warningText.text = "WATCH OUT TIME !!!";
        }
        else if (type == Level.LevelType.OBSTACLE)
        {
            remainingSubText.text = "MOVES:";
            warningText.text = "CLEAR ALL OBSTACLES !!!";
        }
    }

    public void OnGameWin(int score)
    {
        gameOver.OnGameWin(score,visibleStarCount);
    }

    public void OnGameLoose(int score)
    {
        gameOver.OnGameLoose(score);
    }

}

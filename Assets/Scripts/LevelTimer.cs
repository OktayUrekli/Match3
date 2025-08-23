using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelTimer : Level
{
    public int timeInSecond;
    public int targetScore;

    float timer;
    bool timeOut = false;
    // Start is called before the first frame update
    void Start()
    {
        type=LevelType.TIMER;
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;

        if (!timeOut)
        {
            if (timeInSecond - timer <= 0)
            {
                timeOut = true;
                if (currentScore >= targetScore)
                {
                    GameWin();
                }
                else
                { 
                    GameLose();
                    Debug.Log("Time is Over");
                    Debug.Log("Score: " + currentScore);
                }
            }
        }
    }

    //public override void OnPieceCleared(GamePiece piece)
    //{
    //    base.OnPieceCleared(piece);
    //    if (timeInSecond > 0) 
    //    {
    //    }
    //}
}

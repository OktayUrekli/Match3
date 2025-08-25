using System.Collections;
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
        hud.SetLevelType(type);
        hud.SetScore(currentScore);
        hud.SetReamining(string.Format("{0}:{1:00}",timeInSecond/60,timeInSecond%60));

        StartCoroutine(CountdownTimer());
    }

    

    //void Update()
    //{
    //    timer += Time.deltaTime;
    //    hud.SetReamining(string.Format("{0}:{1}", (int)Mathf.Max((timeInSecond - timer) / 60,0), (int)Mathf.Max((timeInSecond - timer) / 60, 0) % 60));

    //    if (!timeOut)
    //    {
    //        if (timeInSecond - timer <= 0)
    //        {
    //            timeOut = true;
    //            if (currentScore >= targetScore)
    //            {
    //                hud.SetScore(currentScore);
    //                GameWin();
    //            }
    //            else
    //            { 
    //                GameLose();
    //                Debug.Log("Time is Over");
    //                Debug.Log("Score: " + currentScore);
    //            }
    //        }
    //    }
    //}

    IEnumerator CountdownTimer()
    {
        

        while (!timeOut)
        {
            hud.SetReamining(string.Format("{0}:{1:00}", (int)Mathf.Max((timeInSecond - timer) / 60, 0), (int)Mathf.Max((timeInSecond - timer) % 60, 0)));

            timer++;

            if (timeInSecond - timer <= 0)
            {
                timeOut = true;
                if (currentScore >= targetScore)
                {
                    hud.SetScore(currentScore);
                    GameWin();
                }
                else
                {
                    GameLose();
                    Debug.Log("Time is Over");
                }
            }

            yield return new WaitForSeconds(1);
        }

        
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelMoves : Level
{
    public int numMoves;
    public int targetScore;

    int movesUsed = 0;

    private void Start()
    {
        type = LevelType.MOVES;
       
    }

    public override void OnMove()
    {
        movesUsed++;

        Debug.Log("Moves Remaining " + (numMoves - movesUsed).ToString());

        if (numMoves - movesUsed==0) 
        {
            if (currentScore>=targetScore)
            {

            }
            else
            {
                GameLose();
            }
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelObstacles : Level
{
    public int numMoves;
    public Grid.PieceType[] obstacleTypes;
    int movesUsed;
    int numObstaclesLeft;

    private void Start()
    {
        type = LevelType.OBSTACLE;
        hud.SetLevelType(type);
        hud.SetScore(currentScore);
        hud.SetReamining(numMoves);
        for (int i = 0; i <obstacleTypes.Length ; i++)
        {
            numObstaclesLeft += grid.GetPiecesOfType(obstacleTypes[i]).Count;
        }
    }

    public override void OnMove()
    {
        movesUsed++;

        hud.SetReamining((numMoves - movesUsed));

        if (numMoves-movesUsed==0 && numObstaclesLeft>0)
        {
            GameLose();
        }
    }

    public override void OnPieceCleared(GamePiece piece)
    {
        base.OnPieceCleared(piece);

        for (int i = 0; i < obstacleTypes.Length; i++)
        {
            if (obstacleTypes[i]==piece.Type)
            {
                numObstaclesLeft--;
                 
                if (numObstaclesLeft==0)
                {
                    currentScore += 1000*(numMoves-movesUsed);
                    hud.SetScore(currentScore);
                    GameWin();
                }
            }
        }
    }
}

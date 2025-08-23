using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level : MonoBehaviour
{
    public enum LevelType
    {
        TIMER,
        OBSTACLE,
        MOVES
    }

    public Grid grid;

    public int score1Star;
    public int score2Star;
    public int score3Star;

    protected LevelType type;
    public LevelType Type
    {
        get {return type;}
    }

    protected int currentScore;

    public virtual void GameWin()
    {
        grid.GameOver();
    }

    public virtual void GameLose()
    {
        grid.GameOver();
    }

    public virtual void OnMove()
    {
        Debug.Log("On Move");
    }


    public virtual void OnPieceCleared(GamePiece piece) 
    {
        currentScore += piece.score;
        Debug.Log("Current score: "+currentScore);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovablePiece : MonoBehaviour
{
    GamePiece piece;

    IEnumerator moveCoroutine;

    private void Awake()
    {
        piece = GetComponent<GamePiece>();
    }

    public void Move(int newX,int newY,float time)
    {
        //piece.X = newX;
        //piece.Y = newY;

        //piece.transform.localPosition = piece.GridRef.GetWorldPosition(newX,newY);


        if (moveCoroutine!=null)
        {
            StopCoroutine(moveCoroutine);
        }

        moveCoroutine = MoveCoroutine(newX,newY,time);
        StartCoroutine(moveCoroutine);
    }

    IEnumerator MoveCoroutine(int newX, int newY, float time)
    {
        piece.X=newX; piece.Y=newY;

        Vector3 startPos=transform.position;
        Vector3 endPos = piece.GridRef.GetWorldPosition(newX, newY);
        Vector3.Slerp(startPos, endPos, time); 
        piece.transform.position = endPos;
        yield return new WaitForSeconds(0);


    }
}
